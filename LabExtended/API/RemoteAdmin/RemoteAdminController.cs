using System.Diagnostics;
using System.Text;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Interfaces;

using LabExtended.Events;
using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Utilities.Update;

using LabExtended.API.RemoteAdmin.Actions;
using LabExtended.API.RemoteAdmin.Buttons;

using NetworkManagerUtils.Dummies;

using RemoteAdmin.Communication;

using LabExtended.Utilities;
using NorthwoodLib.Pools;

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace LabExtended.API.RemoteAdmin;

/// <summary>
/// Controls custom Remote Admin functions.
/// </summary>
public class RemoteAdminController : IDisposable
{
    /// <summary>
    /// The prefix for the dummy icon.
    /// </summary>
    public const string DummyIconPrefix = "[<color=#fcba03>\ud83d\udcbb</color>] ";

    /// <summary>
    /// The prefix for the muted icon.
    /// </summary>
    public const string MutedIconPrefix = "<link=RA_Muted><color=white>[</color>\ud83d\udd07<color=white>]</color></link> ";

    /// <summary>
    /// The prefix for the Overwatch icon.
    /// </summary>
    public const string OverwatchIconPrefix = "<link=RA_OverwatchEnabled><color=white>[</color><color=#03f8fc>\uf06e</color><color=white>]</color></link> ";

    private static readonly UniqueList objectIdGenerator = new();
    private static readonly UniqueList listIdGenerator = new();

    // ReSharper disable once CollectionNeverUpdated.Local
    private static readonly HashSet<Type> globalObjects = new();
    private static readonly HashSet<Type> globalModules = new();

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
    public ExPlayer Player { get; }

    /// <summary>
    /// Gets the Remote Admin Action provider.
    /// </summary>
    public RemoteAdminActionProvider Actions { get; private set; }

    /// <summary>
    /// Gets a list of custom Remote Admin objects.
    /// </summary>
    public List<IRemoteAdminObject> Objects { get; private set; }

    internal RemoteAdminController(ExPlayer player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        Player = player;

        Actions = new(player);

        Objects = ListPool<IRemoteAdminObject>.Shared.Rent();

        globalObjects.ForEach(type => AddObject(type));

        InternalEvents.OnPlayerVerified += OnVerified;

        PlayerEvents.RequestingRaPlayerList += OnRequestingPlayerList;
        PlayerEvents.RequestedRaPlayerList += OnRequestedPlayerList;

        PlayerUpdateHelper.Component.OnUpdate += Update;
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        PlayerUpdateHelper.Component.OnUpdate -= Update;

        PlayerEvents.RequestingRaPlayerList -= OnRequestingPlayerList;
        PlayerEvents.RequestedRaPlayerList -= OnRequestedPlayerList;

        requestWatch?.Stop();
        requestWatch = null;

        Actions?.Dispose();
        Actions = null;

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

                listIdGenerator.Generated.Remove(obj.Id);
                objectIdGenerator.Generated.Remove(obj.Id);
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

        raObject.Id = objectIdGenerator.GetString(10);
        raObject.ListId = listIdGenerator.Get(() => UnityEngine.Random.Range(6000, 11000));

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
            remoteAdminObject.Id = objectIdGenerator.GetString(10);

        remoteAdminObject.ListId = listIdGenerator.Get(() => UnityEngine.Random.Range(6000, 11000));

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

        objectIdGenerator.Generated.Remove(remoteAdminObject.Id);
        listIdGenerator.Generated.Remove(remoteAdminObject.ListId);

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
                    builder.Append(DummyIconPrefix);

                if ((obj.Icons & RemoteAdminIconType.MutedIcon) != 0)
                    builder.Append(MutedIconPrefix);

                if ((obj.Icons & RemoteAdminIconType.OverwatchIcon) != 0)
                    builder.Append(OverwatchIconPrefix);
            }

            builder.Append($"({obj.ListId}) ");
            builder.Append(obj.GetName(Player).Replace("\n", string.Empty).Replace("RA_", string.Empty))
                .Append("</color>");
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
                    builder.Append(DummyIconPrefix);

                if ((obj.Icons & RemoteAdminIconType.MutedIcon) != 0)
                    builder.Append(MutedIconPrefix);

                if ((obj.Icons & RemoteAdminIconType.OverwatchIcon) != 0)
                    builder.Append(OverwatchIconPrefix);
            }

            builder.Append($"({obj.ListId}) ");
            builder.Append(obj.GetName(Player).Replace("\n", string.Empty).Replace("RA_", string.Empty))
                .Append("</color>");
            builder.AppendLine();
        }
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

    private void OnVerified(ExPlayer player)
    {
        if (player != Player)
            return;

        InternalEvents.OnPlayerVerified -= OnVerified;

        _ = DummyActionCollector.GetCache(player.ReferenceHub);
    }

    private void OnRequestingPlayerList(PlayerRequestingRaPlayerListEventArgs args)
    {
        if (args.Player != Player)
            return;

        requestWatch.Restart();

        if (Objects.Count > 0)
            PrependObjects(args.ListBuilder);
    }

    private void OnRequestedPlayerList(PlayerRequestedRaPlayerListEventArgs args)
    {
        if (args.Player != Player)
            return;

        if (Objects.Count > 0)
            AppendObjects(args.ListBuilder);
    }

    private static void OnAddingPlayer(PlayerRaPlayerListAddingPlayerEventArgs args)
    {
        if (args.Player is not ExPlayer player)
            return;

        if (args.Target is not ExPlayer target)
            return;

        if (!target.Toggles.IsVisibleInRemoteAdmin)
        {
            args.IsAllowed = false;
            return;
        }

        args.Prefix = RaPlayerList.GetPrefix(target.ReferenceHub,
            player.HasPermission(PlayerPermissions.ViewHiddenBadges),
            player.HasPermission(PlayerPermissions.ViewHiddenGlobalBadges));

        var icons = target.RemoteAdminForcedIcons;

        if (icons != RemoteAdminIconType.None)
        {
            if ((icons & RemoteAdminIconType.DummyIcon) == RemoteAdminIconType.DummyIcon)
                args.Prefix += DummyIconPrefix;

            if ((icons & RemoteAdminIconType.MutedIcon) == RemoteAdminIconType.MutedIcon)
                args.Prefix += MutedIconPrefix;

            if ((icons & RemoteAdminIconType.OverwatchIcon) == RemoteAdminIconType.OverwatchIcon)
                args.Prefix += OverwatchIconPrefix;
        }
    }

    internal static void Internal_Init()
    {
        PlayerEvents.RaPlayerListAddingPlayer += OnAddingPlayer;
    }
}