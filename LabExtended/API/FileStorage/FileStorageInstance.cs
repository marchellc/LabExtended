using System.Collections.Concurrent;
using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;
using LabExtended.Utilities;
using LabExtended.Utilities.Update;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NorthwoodLib.Pools;

namespace LabExtended.API.FileStorage;

/// <summary>
/// A loaded file storage instance.
/// </summary>
public class FileStorageInstance : IDisposable
{
    private volatile bool isActive = false;
    
    internal volatile string userId = string.Empty;

    /// <summary>
    /// The target player.
    /// </summary>
    public ExPlayer Player
    {
        get;
        set
        {
            field = value;
            
            if (value != null && !string.IsNullOrWhiteSpace(value.UserId))
                userId = value.UserId;
        }
    }
    
    /// <summary>
    /// Path to the storage directory.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the target user ID.
    /// </summary>
    public string UserId => userId;

    /// <summary>
    /// Gets a list of all component instances.
    /// </summary>
    public ConcurrentDictionary<Type, FileStorageComponent> Components { get; } = new();

    /// <summary>
    /// Whether or not the instance is active.
    /// </summary>
    public bool IsActive
    {
        get => isActive;
        internal set => isActive = value;
    }

    internal FileStorageInstance(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));

        Path = path;
    }
    
    /// <summary>
    /// Loads all files from the directory.
    /// </summary>
    public void Load()
    {
        PlayerUpdateHelper.OnUpdate += Update;
        PlayerUpdateHelper.OnThreadUpdate += CheckDirty;

        foreach (var componentType in FileStorageManager.Components)
        {
            AddComponent(componentType);
        }
    }
    
    /// <summary>
    /// Loads the file of a specific component.
    /// </summary>
    /// <param name="component">The component.</param>
    /// <param name="filePath">The path to the component's file.</param>
    public void LoadFile(FileStorageComponent component, string filePath)
    {
        if (component is null)
            throw new ArgumentNullException(nameof(component));
        
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));
        
        component.Path = filePath;

        FileStorageManager.pathToComponent.TryRemove(filePath, out _);
        FileStorageManager.pathToComponent.TryAdd(filePath, component);

        if (!File.Exists(filePath))
        {
            SaveFile(component, false);
            return;
        }

        var fileText = File.ReadAllText(filePath);
        
        component.componentData = JsonConvert.DeserializeObject<JObject>(fileText);

        if (!component.componentData.TryGetValue("properties", StringComparison.InvariantCulture,
                out var propertiesToken))
        {
            ApiLog.Debug("File Storage", $"Missing &3properties&r token in component &3{component.Name}&r");
            
            SaveFile(component, false);
            return;
        }

        if (propertiesToken is not JArray properties)
        {
            ApiLog.Warn("File Storage", $"Invalid &3properties&r token in component &3{component.Name}&r!");
            
            SaveFile(component, false);
            return;
        }

        var addedProperties = ListPool<string>.Shared.Rent();
        var removeProperties = ListPool<JToken>.Shared.Rent();

        var anyModified = false;
        
        foreach (var propertyValue in properties)
        {
            if (propertyValue is not JObject propertyObject)
            {
                ApiLog.Warn("File Storage", $"Invalid property value token (&3{propertyValue.Path}&r) in component &3{component.Name}&r!");
                
                SaveFile(component, false);
                
                ListPool<string>.Shared.Return(addedProperties);
                ListPool<JToken>.Shared.Return(removeProperties);
                
                return;
            }

            if (!propertyObject.TryGetValue("name", StringComparison.InvariantCulture, out var nameToken))
            {
                ApiLog.Warn("File Storage", $"Missing &3name&r property on property &3{propertyValue.Path}&r in component &3{component.Name}&r!");

                SaveFile(component, false);
                
                ListPool<string>.Shared.Return(addedProperties);
                ListPool<JToken>.Shared.Return(removeProperties);
                
                return;
            }

            if (!propertyObject.TryGetValue("data", StringComparison.InvariantCulture, out var dataToken))
            {
                ApiLog.Warn("File Storage", $"Missing &3data&r property on property &3{propertyValue.Path}&r in component &3{component.Name}&r!");

                SaveFile(component, false);
                
                ListPool<string>.Shared.Return(addedProperties);
                ListPool<JToken>.Shared.Return(removeProperties);
                
                return;
            }

            var name = nameToken.Value<string>();

            if (component.properties.TryGetValue(name, out var property))
            {
                property.IsDirty = false;
                
                var newValue = dataToken.ToObject(property.GenericType);
                
                if (property.NonGenericValue is null && newValue != null ||
                    property.NonGenericValue != null && newValue is null ||
                    (property.NonGenericValue != null && newValue != null && !newValue.Equals(property.NonGenericValue)))
                    property.InvokeModified(newValue);
                
                property.NonGenericValue = newValue;
                property.propertyObject = propertyObject;
                
                addedProperties.Add(name);
            }
            else
            {
                ApiLog.Warn("File Storage", $"Could not find property &3{name}&r in component &3{component.Name}&r!");
                
                removeProperties.Add(propertyValue);
            }
        }

        foreach (var property in component.properties)
        {
            if (!addedProperties.Contains(property.Value.Name))
            {
                ApiLog.Warn("File Storage", $"Property &3{property.Value.Name}&r (in component &6{component.Name}&r) was not found in loaded data!");

                var propertyObj = new JObject();
                
                propertyObj.Add("name", property.Value.Name);
                propertyObj.Add("data", JToken.FromObject(property.Value.NonGenericValue));

                property.Value.propertyObject = propertyObj;

                anyModified = true;
            }
        }

        foreach (var propertyToken in removeProperties)
        {
            if (properties.Remove(propertyToken))
            {
                anyModified = true;
            }
        }

        if (anyModified)
        {
            component.writeLock = true;
            
            File.WriteAllText(filePath, component.componentData.ToString(Formatting.Indented));

            component.writeLock = false;
        }
        
        ListPool<string>.Shared.Return(addedProperties);
        ListPool<JToken>.Shared.Return(removeProperties);
    }

    /// <summary>
    /// Saves the file of a specific component.
    /// </summary>
    /// <param name="component">The component.</param>
    /// <param name="onlyDirty">Whether or not to save only dirty properties.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void SaveFile(FileStorageComponent component, bool onlyDirty = false)
    {
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        var anyChanged = false;

        if (component.componentData is null)
        {
            component.componentData = new JObject();
            anyChanged = true;
        }

        if (!component.componentData.TryGetValue("properties", StringComparison.InvariantCulture,
                out var propertiesToken)
            || propertiesToken is not JArray properties)
        {
            component.componentData.Remove("properties");
            component.componentData.Add("properties", properties = new JArray());

            anyChanged = true;
        }

        foreach (var property in component.properties)
        {
            if (!property.Value.IsDirty && onlyDirty)
                continue;

            if (property.Value.propertyObject is null)
            {
                property.Value.propertyObject = new JObject();
                
                property.Value.propertyObject.Add("name", property.Value.Name);
                property.Value.propertyObject.Add("data", JToken.FromObject(property.Value.NonGenericValue));
                
                properties.Add(property.Value.propertyObject);

                anyChanged = true;
            }
            else
            {
                property.Value.propertyObject.Remove("data");
                property.Value.propertyObject.Add("data", JToken.FromObject(property.Value.NonGenericValue));
                
                if (!properties.Contains(property.Value.propertyObject))
                    properties.Add(property.Value.propertyObject);

                anyChanged = true;
            }
            
            if (property.Value.IsDirty)
                property.Value.InvokeSaved();
            
            property.Value.IsDirty = false;
        }

        if (anyChanged)
        {
            component.writeLock = true;
            
            File.WriteAllText(component.Path, component.componentData.ToString(Formatting.Indented));

            component.writeLock = false;
        }
    }
    
    /// <summary>
    /// Gets a specific component.
    /// </summary>
    /// <param name="predicate">The predicate filter.</param>
    /// <typeparam name="T">Component cast type.</typeparam>
    /// <returns>the resolved component instance (or null if not found)</returns>
    public T? GetComponent<T>(Predicate<T> predicate) where T : FileStorageComponent
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        if (TryGetComponent(predicate, out var component))
            return component;

        return null;
    }

    /// <summary>
    /// Gets a specific component.
    /// </summary>
    /// <param name="predicate">The predicate filter.</param>
    /// <returns>the resolved component instance (or null if not found)</returns>
    public FileStorageComponent? GetComponent(Predicate<FileStorageComponent> predicate)
    {        
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        if (TryGetComponent(predicate, out var component))
            return component;

        return null;
    }

    /// <summary>
    /// Attempts to get a specific component.
    /// </summary>
    /// <param name="predicate">The predicate filter.</param>
    /// <param name="component">The resolved instance.</param>
    /// <typeparam name="T">Component cast type.</typeparam>
    /// <returns>true if the instance was found</returns>
    public bool TryGetComponent<T>(Predicate<T> predicate, out T component) where T : FileStorageComponent
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        foreach (var target in Components)
        {
            if (target.Value is not T cast)
                continue;
            
            if (!predicate(cast))
                continue;

            component = cast;
            return true;
        }

        component = null;
        return false;
    }

    /// <summary>
    /// Attempts to get a specific component.
    /// </summary>
    /// <param name="predicate">The predicate filter.</param>
    /// <param name="component">The resolved instance.</param>
    /// <returns>true if the instance was found</returns>
    public bool TryGetComponent(Predicate<FileStorageComponent> predicate, out FileStorageComponent component)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        foreach (var target in Components)
        {
            if (!predicate(target.Value))
                continue;
            
            component = target.Value;
            return true;
        }

        component = null;
        return false;
    }

    /// <summary>
    /// Adds a component of a specific type.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <returns>The added (or found) component instance.</returns>
    /// <exception cref="Exception"></exception>
    public T AddComponent<T>() where T : FileStorageComponent
    {
        if (AddComponent(typeof(T)) is T component)
            return component;

        throw new Exception($"Could not add component {typeof(T).FullName}");
    }

    /// <summary>
    /// Adds a component of a specific type.
    /// </summary>
    /// <param name="componentType">The component type.</param>
    /// <returns>The added (or found) component instance.</returns>
    /// <exception cref="Exception"></exception>
    public FileStorageComponent AddComponent(Type componentType)
    {
        if (componentType is null)
            throw new ArgumentNullException(nameof(componentType));

        if (Components.TryGetValue(componentType, out var activeComponent))
            return activeComponent;

        if (Activator.CreateInstance(componentType) is not FileStorageComponent component)
            throw new Exception($"Could not instantiate FileStorageComponent {componentType.FullName}");

        component.Player = Player;
        component.Storage = this;
        
        component.InitProperties();

        Task.Run(() => { LoadFile(component, System.IO.Path.Combine(Path, component.Name)); }).ContinueWithOnMain(_ =>
        {
            component.OnLoaded();

            Components.TryAdd(componentType, component);
        });
        
        return component;
    }

    /// <summary>
    /// Removes a component of a specific type.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <returns>true if the component was removed</returns>
    public bool RemoveComponent<T>() where T : FileStorageComponent
        => RemoveComponent(typeof(T));

    /// <summary>
    /// Removes a specific component.
    /// </summary>
    /// <param name="componentType">The component type.</param>
    /// <returns>true if the component was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool RemoveComponent(Type componentType)
    {
        if (componentType is null)
            throw new ArgumentNullException(nameof(componentType));

        if (!Components.TryRemove(componentType, out var activeComponent))
            return false;
        
        try
        {
            if (activeComponent.Path != null)
                FileStorageManager.pathToComponent.TryRemove(activeComponent.Path, out _);
            
            activeComponent.OnUnloaded();
            
            SaveFile(activeComponent);
            
            if (activeComponent.properties != null)
            {
                foreach (var property in activeComponent.properties)
                {
                    property.Value.Dispose();
                }
                
                activeComponent.properties.Clear();
            }

            activeComponent.properties = null;
            
            activeComponent.componentData?.RemoveAll();
            activeComponent.componentData = null;
        }
        catch (Exception ex)
        {
            ApiLog.Error("File Storage", $"Could not unloaded component &3{activeComponent.GetType().FullName}&r:\n{ex}");
        }
        
        return true;
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (Components != null)
        {
            PlayerUpdateHelper.OnUpdate -= Update;
            PlayerUpdateHelper.OnThreadUpdate -= CheckDirty;

            foreach (var component in Components)
            {
                try
                {
                    if (component.Value.Path != null)
                        FileStorageManager.pathToComponent.TryRemove(component.Value.Path, out _);
                    
                    component.Value.OnUnloaded();
                }
                catch (Exception ex)
                {
                    ApiLog.Error("File Storage", $"Could not unloaded component &3{component.Key.FullName}&r:\n{ex}");
                }
            }
            
            Components.Clear();
        }
    }

    internal void OnJoined()
    {
        foreach (var component in Components)
        {
            try
            {
                component.Value.Player = Player;
                component.Value.OnJoined();
            }
            catch (Exception ex)
            {
                ApiLog.Error("File Storage", $"Component &1{component.Key.FullName}&r failed to handle player join!\n{ex}");
            }
        }
    }

    internal void OnLeft()
    {
        foreach (var component in Components)
        {
            try
            {
                component.Value.OnLeft();
                component.Value.Player = null;
            }
            catch (Exception ex)
            {
                ApiLog.Error("File Storage", $"Component &1{component.Key.FullName}&r failed to handle player leave!\n{ex}");
            }
        }
    }

    private void Update()
    {
        foreach (var component in Components)
        {
            try
            {
                component.Value.OnUpdate();
            }
            catch (Exception ex)
            {
                ApiLog.Error("File Storage", $"Could not update component &3{component.Key.FullName}&r:\n{ex}");
            }
        }
    }

    private async Task CheckDirty()
    {
        foreach (var component in Components)
        {
            try
            {
                component.Value.CheckDirty();
            }
            catch (Exception ex)
            {
                ApiLog.Error("File Storage", $"Could not check component dirtiness (&3{component.Key.FullName}&r):\n{ex}");
            }
        }
    }
}