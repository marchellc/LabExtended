using System.Collections.Concurrent;
using System.Diagnostics;
using LabExtended.Core;
using Newtonsoft.Json.Linq;

namespace LabExtended.API.FileStorage;

/// <summary>
/// Represents a component in a player's file storage.
/// </summary>
public abstract class FileStorageComponent
{
    internal volatile Stopwatch writeWatch = new();
    internal volatile ConcurrentDictionary<string, FileStoragePropertyBase> properties = new();
    internal volatile JObject componentData;
    
    /// <summary>
    /// Gets the parent storage instance.
    /// </summary>
    public FileStorageInstance Storage { get; internal set; }
    
    /// <summary>
    /// Gets the target player.
    /// </summary>
    public ExPlayer Player { get; internal set; }
    
    /// <summary>
    /// Gets the path to the component's file.
    /// </summary>
    public string Path { get; internal set; }

    /// <summary>
    /// Gets the name of the component (will be used as the file name).
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets called before <see cref="OnLoaded"/>. This method is used to register properties.
    /// </summary>
    public abstract void InitProperties();

    /// <summary>
    /// Gets called when the storage is loaded.
    /// </summary>
    public abstract void OnLoaded();

    /// <summary>
    /// Gets called when the storage gets unloaded.
    /// </summary>
    public abstract void OnUnloaded();

    /// <summary>
    /// Gets called when the target player joins.
    /// </summary>
    public abstract void OnJoined();

    /// <summary>
    /// Gets called when the target player leaves.
    /// </summary>
    public abstract void OnLeft();

    /// <summary>
    /// Gets called when the target file is reloaded (modified by another process).
    /// </summary>
    public virtual void OnReloaded()
    {
        
    }

    /// <summary>
    /// Gets called once per frame.
    /// </summary>
    public virtual void OnUpdate()
    {

    }

    internal void CheckDirty()
    {
        var anyDirty = false;
        
        foreach (var property in properties)
        {
            if (!property.Value.IsDirty)
                continue;
            
            ApiLog.Debug("File Storage", $"&3[{Name} - {Storage.UserId}]&r Property &6{property.Key}&r is dirty!");

            anyDirty = true;
            break;
        }

        if (!anyDirty)
            return;
        
        Storage.SaveFile(this, true);
    }
    
    /// <summary>
    /// Adds a new property to the component.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="defaultValue">The property value.</param>
    /// <typeparam name="T">Property value type.</typeparam>
    /// <returns>The added property instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public FileStorageProperty<T> AddProperty<T>(string name, T defaultValue)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        if (properties.TryGetValue(name, out var property))
            return (FileStorageProperty<T>)property;
        
        if (defaultValue is null)
            throw new ArgumentNullException(nameof(defaultValue));

        var newProperty = new FileStorageProperty<T>(name);

        newProperty.Value = defaultValue;
        newProperty.IsDirty = true;
        
        properties.TryAdd(name, newProperty);
        return newProperty;
    }

    /// <summary>
    /// Reloads the component's data file.
    /// </summary>
    public void ReloadFile()
        => Storage.LoadFile(this, Path);

    /// <summary>
    /// Saves current data to the component's file.
    /// <param name="onlyDirty">Whether or not to modify only properties whose value has changed.</param>
    /// </summary>
    public void SaveFile(bool onlyDirty = true)
        => Storage.SaveFile(this, onlyDirty);
}