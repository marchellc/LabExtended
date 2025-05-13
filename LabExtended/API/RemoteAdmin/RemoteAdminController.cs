using System.Diagnostics;

using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Interfaces;

using LabExtended.Events;
using LabExtended.Extensions;

using LabExtended.Utilities.Update;
using LabExtended.Utilities.Generation;

using LabExtended.Patches.Functions.RemoteAdmin;

using NorthwoodLib.Pools;

using System.Text;
using LabExtended.API.RemoteAdmin.Buttons;

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace LabExtended.API.RemoteAdmin;

/// <summary>
/// Controls custom Remote Admin functions.
/// </summary>
public class RemoteAdminController : IDisposable
{
    private static readonly UniqueStringGenerator objectIdGenerator = new(10, false);
    private static readonly UniqueInt32Generator listIdGenerator = new(6000, 11000);

    // ReSharper disable once CollectionNeverUpdated.Local
    private static readonly HashSet<Type> globalObjects = new();
    
    /// <summary>
    /// Gets all Remote Admin buttons.
    /// </summary>
    public static Dictionary<RemoteAdminButtonType, IRemoteAdminButton> Buttons { get; } = new()
    {
        [RemoteAdminButtonType.Request] = new RemoteAdminButton(RemoteAdminButtonType.Request),
        [RemoteAdminButtonType.RequestIp] = new RemoteAdminButton(RemoteAdminButtonType.RequestIp),
        [RemoteAdminButtonType.RequestAuth] = new RemoteAdminButton(RemoteAdminButtonType.RequestAuth),
        [RemoteAdminButtonType.ExternalLookup] = new RemoteAdminButton(RemoteAdminButtonType.ExternalLookup)
    };
    
    /// <summary>
    /// Binds a Remote Admin object to a button.
    /// </summary>
    /// <param name="buttonType">The button to bind the object to.</param>
    /// <param name="remoteAdminObject">The Remote Admin object.</param>
    /// <returns>true if the button was bound</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool BindObject(RemoteAdminButtonType buttonType, IRemoteAdminObject remoteAdminObject)
    {
        if (remoteAdminObject is null)
            throw new ArgumentNullException(nameof(remoteAdminObject));

        if (!Buttons.TryGetValue(buttonType, out var button))
            return false;

        return button.BindObject(remoteAdminObject);
    }
    
    /// <summary>
    /// Unbinds a Remote Admin object from a button.
    /// </summary>
    /// <param name="buttonType">The button to unbind the object from.</param>
    /// <param name="remoteAdminObject">The Remote Admin object.</param>
    /// <returns>true if the button was unbound</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool UnbindObject(RemoteAdminButtonType buttonType, IRemoteAdminObject remoteAdminObject)
    {
        if (remoteAdminObject is null)
            throw new ArgumentNullException(nameof(remoteAdminObject));

        if (!Buttons.TryGetValue(buttonType, out var button))
            return false;

        return button.UnbindObject(remoteAdminObject);
    }

    private Stopwatch requestWatch = new();
    private bool wasOpen;

    /// <summary>
    /// Whether or not the Remote Admin panel is considered open.
    /// </summary>
    public bool IsOpen { get; private set; }

    /// <summary>
    /// Gets the parent player.
    /// </summary>
    public ExPlayer Player { get; private set; }

    /// <summary>
    /// Gets a list of custom Remote Admin objects.
    /// </summary>
    public List<IRemoteAdminObject> Objects { get; private set; }

    internal RemoteAdminController(ExPlayer player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        Player = player;
        Objects = ListPool<IRemoteAdminObject>.Shared.Rent();

        globalObjects.ForEach(type => AddObject(type));
        
        PlayerUpdateHelper.OnUpdate += Update;
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        PlayerUpdateHelper.OnUpdate -= Update;
        
        requestWatch?.Stop();
        requestWatch = null;

        wasOpen = false;

        if (Objects != null)
        {
            foreach (var obj in Objects)
            {
                if (obj.IsActive)
                {
                    obj.IsActive = false;
                    obj.OnDisabled();
                }

                objectIdGenerator.Free(obj.Id);
                listIdGenerator.Free(obj.ListId);
            }
            
            ListPool<IRemoteAdminObject>.Shared.Return(Objects);
        }
        
        Objects = null;
    }

    /// <summary>
    /// Adds a new Remote Admin object.
    /// </summary>
    /// <param name="objectType">The object type.</param>
    /// <param name="customId">Custom object ID.</param>
    /// <returns>The added Remote Admin object instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public IRemoteAdminObject AddObject(Type objectType, string? customId = null)
    {
        if (objectType is null)
            throw new ArgumentNullException(nameof(objectType));

        if (!objectType.InheritsType<IRemoteAdminObject>())
            throw new InvalidOperationException(
                $"Type {objectType.FullName} does not inherit interface IRemoteAdminObject");

        if (!string.IsNullOrWhiteSpace(customId)
            && Objects.Any(obj => !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == customId))
            throw new InvalidOperationException($"There's already an object with the same ID ({customId})");

        var raObject = objectType.Construct<IRemoteAdminObject>();

        raObject.CustomId = customId;

        raObject.Id = objectIdGenerator.Next();
        raObject.ListId = listIdGenerator.Next();

        raObject.IsActive = true;
        raObject.OnEnabled();

        Objects.Add(raObject);
        return raObject;
    }

    /// <summary>
    /// Adds a new Remote Admin object.
    /// </summary>
    /// <param name="customId">Custom object ID.</param>
    /// <returns>The added Remote Admin object instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public T AddObject<T>(string? customId = null) where T : IRemoteAdminObject
        => (T)AddObject(typeof(T), customId);

    /// <summary>
    /// Adds a new Remote Admin object.
    /// </summary>
    /// <param name="remoteAdminObject">The object instance to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void AddObject(IRemoteAdminObject remoteAdminObject)
    {
        if (remoteAdminObject is null)
            throw new ArgumentNullException(nameof(remoteAdminObject));

        if (Objects.Contains(remoteAdminObject))
            throw new InvalidOperationException("This object has already been added.");

        if (!string.IsNullOrWhiteSpace(remoteAdminObject.CustomId)
            && Objects.Any(obj => !string.IsNullOrWhiteSpace(obj.CustomId)
                                  && obj.CustomId == remoteAdminObject.CustomId))
            throw new InvalidOperationException(
                $"There's already an object with the same ID ({remoteAdminObject.CustomId})");

        if (!remoteAdminObject.IsActive)
        {
            remoteAdminObject.OnEnabled();
            remoteAdminObject.IsActive = true;
        }

        if (string.IsNullOrWhiteSpace(remoteAdminObject.Id))
            remoteAdminObject.Id = objectIdGenerator.Next();

        remoteAdminObject.ListId = listIdGenerator.Next();

        Objects.Add(remoteAdminObject);
    }

    /// <summary>
    /// Removes an active Remote Admin object.
    /// </summary>
    /// <param name="remoteAdminObject">The object to remove.</param>
    /// <returns>true if the object was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool RemoveObject(IRemoteAdminObject remoteAdminObject)
    {
        if (remoteAdminObject is null)
            throw new ArgumentNullException(nameof(remoteAdminObject));

        if (remoteAdminObject.IsActive)
        {
            remoteAdminObject.IsActive = false;
            remoteAdminObject.OnDisabled();
        }

        objectIdGenerator.Free(remoteAdminObject.Id);
        listIdGenerator.Free(remoteAdminObject.ListId);

        return Objects.Remove(remoteAdminObject);
    }

    /// <summary>
    /// Removes an active Remote Admin object.
    /// </summary>
    /// <returns>true if the object was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool RemoveObject<T>() where T : IRemoteAdminObject
    {
        if (!TryGetObject<T>(out var remoteAdminObject))
            return false;

        return RemoveObject(remoteAdminObject);
    }

    /// <summary>
    /// Removes an active Remote Admin object of a specific ID.
    /// </summary>
    /// <param name="customId">ID of the object to remove.</param>
    /// <returns>true if the object was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool RemoveObject(string customId)
    {
        if (!TryGetObject(customId, out var remoteAdminObject))
            return false;

        return RemoveObject(remoteAdminObject);
    }

    /// <summary>
    /// Removes an active Remote Admin object.
    /// </summary>
    /// <param name="listId">The player list ID of the object.</param>
    /// <returns>true if the object was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool RemoveObject(int listId)
    {
        if (!TryGetObject(listId, out var remoteAdminObject))
            return false;

        return RemoveObject(remoteAdminObject);
    }

    /// <summary>
    /// Attempts to retrieve an object.
    /// </summary>
    /// <param name="remoteAdminObject">The resolved object.</param>
    /// <typeparam name="T">Type to cast the object to.</typeparam>
    /// <returns>true if the object was found</returns>
    public bool TryGetObject<T>(out T remoteAdminObject) where T : IRemoteAdminObject
        => Objects.TryGetFirst(out remoteAdminObject);

    /// <summary>
    /// Attempts to retrieve an object.
    /// </summary>
    /// <param name="remoteAdminObject">The resolved object.</param>
    /// <param name="customId">The object's custom ID.</param>
    /// <returns>true if the object was found</returns>
    public bool TryGetObject(string customId, out IRemoteAdminObject remoteAdminObject)
        => Objects.TryGetFirst(obj => !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == customId,
            out remoteAdminObject);

    /// <summary>
    /// Attempts to retrieve an object.
    /// </summary>
    /// <param name="remoteAdminObject">The resolved object.</param>
    /// <param name="listId">The object's player list ID.</param>
    /// <returns>true if the object was found</returns>
    public bool TryGetObject(int listId, out IRemoteAdminObject remoteAdminObject)
        => Objects.TryGetFirst(obj => obj.ListId == listId, out remoteAdminObject);

    /// <summary>
    /// Attempts to retrieve an object.
    /// </summary>
    /// <param name="remoteAdminObject">The resolved object.</param>
    /// <param name="customId">The object's custom ID.</param>
    /// <typeparam name="T">Type to cast the object to.</typeparam>
    /// <returns>true if the object was found</returns>
    public bool TryGetObject<T>(string customId, out T remoteAdminObject) where T : IRemoteAdminObject
        => Objects.TryGetFirst(obj => !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == customId,
            out remoteAdminObject);

    /// <summary>
    /// Attempts to retrieve an object.
    /// </summary>
    /// <param name="remoteAdminObject">The resolved object.</param>
    /// <param name="listId">The object's player list ID.</param>
    /// <typeparam name="T">Type to cast the object to.</typeparam>
    /// <returns>true if the object was found</returns>
    public bool TryGetObject<T>(int listId, out T remoteAdminObject) where T : IRemoteAdminObject
        => Objects.TryGetFirst(obj => obj.ListId == listId, out remoteAdminObject);

    internal void ViewObjectHelp()
    {
        if (Objects.Count < 1)
            return;

        var list = ListPool<IRemoteAdminObject>.Shared.Rent();
        var builder = StringBuilderPool.Shared.Rent();
        var pos = 0;

        builder.Append("<line-height=0>");

        foreach (var button in Buttons)
        {
            button.Value.OnOpened(Player, builder, pos, list);
            pos += 26;
        }

        ListPool<IRemoteAdminObject>.Shared.Return(list);

        Player.SendRemoteAdminInfo(StringBuilderPool.Shared.ToStringReturn(builder));
    }
    
    internal void PrependObjects(StringBuilder builder)
    {
        for (var i = 0; i < Objects.Count; i++)
        {
            var obj = Objects[i];
            
            if (!obj.IsActive) 
                continue;
            
            if (!obj.Flags.Any(RemoteAdminObjectFlags.ShowOnTop))
                continue;
            
            if (!obj.Flags.Any(RemoteAdminObjectFlags.ShowToNorthwoodStaff) && Player.IsNorthwoodStaff) 
                continue;
            
            if (!obj.GetVisibility(Player))
                continue;

            if (obj.Icons != RemoteAdminIconType.None)
            {
                if ((obj.Icons & RemoteAdminIconType.DummyIcon) != 0)
                    builder.Append(RemoteAdminListPatch.DummyIconPrefix);

                if ((obj.Icons & RemoteAdminIconType.MutedIcon) != 0) 
                    builder.Append(RemoteAdminListPatch.MutedIconPrefix);
                
                if ((obj.Icons & RemoteAdminIconType.OverwatchIcon) != 0) 
                    builder.Append(RemoteAdminListPatch.OverwatchIconPrefix);
            }

            builder.Append($"({obj.ListId}) ");
            builder.Append(obj.GetName(Player).Replace("\n", string.Empty).Replace("RA_", string.Empty)).Append("</color>");
            builder.AppendLine();
        }
    }

    internal void AppendObjects(StringBuilder builder)
    {
        for (var i = 0; i < Objects.Count; i++)
        {
            var obj = Objects[i];
            
            if (!obj.IsActive)
                continue;
            
            if (obj.Flags.Any(RemoteAdminObjectFlags.ShowOnTop))
                continue;
            
            if (!obj.Flags.Any(RemoteAdminObjectFlags.ShowToNorthwoodStaff) && Player.IsNorthwoodStaff) 
                continue;
            
            if (!obj.GetVisibility(Player))
                continue;

            if (obj.Icons != RemoteAdminIconType.None)
            {
                if ((obj.Icons & RemoteAdminIconType.DummyIcon) != 0)
                    builder.Append(RemoteAdminListPatch.DummyIconPrefix);
                
                if ((obj.Icons & RemoteAdminIconType.MutedIcon) != 0) 
                    builder.Append(RemoteAdminListPatch.MutedIconPrefix);
                
                if ((obj.Icons & RemoteAdminIconType.OverwatchIcon) != 0) 
                    builder.Append(RemoteAdminListPatch.OverwatchIconPrefix);
            }

            builder.Append($"({obj.ListId}) ");
            builder.Append(obj.GetName(Player).Replace("\n", string.Empty).Replace("RA_", string.Empty)).Append("</color>");
            builder.AppendLine();
        }
    }

    internal void OnRequest()
    {
        requestWatch.Restart();
    }

    private void Update()
    {
        IsOpen = requestWatch.ElapsedMilliseconds >= 1100;

        if (IsOpen != wasOpen)
        {
            if (IsOpen)
                ExPlayerEvents.OnOpenedRemoteAdmin(new(Player));
            else
                ExPlayerEvents.OnClosedRemoteAdmin(new(Player));
            
            wasOpen = IsOpen;
        }
    }
}