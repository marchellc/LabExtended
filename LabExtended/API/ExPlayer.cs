using LabApi.Features.Wrappers;
using LabExtended.API.Containers;

using CentralAuth;
using CommandSystem;
using Footprinting;
using GameCore;
using InventorySystem.Disarming;

using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API.Enums;
using LabExtended.API.Hints;
using LabExtended.API.RemoteAdmin;

using LabExtended.Core.Networking;

using LabExtended.Extensions;

using LabExtended.Utilities;

using LiteNetLib;

using Mirror;
using Mirror.LiteNetLib4Mirror;

using PlayerRoles;
using PlayerRoles.Spectating;
using PlayerRoles.FirstPersonControl;

using RemoteAdmin.Communication;

using UnityEngine;

using VoiceChat;

using Hints;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomRoles;
using LabExtended.API.CustomVoice;
using LabExtended.API.Hints.Elements.Personal;
using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Menus;

using LabExtended.Commands;
using LabExtended.Core.Networking.Synchronization.Position;
using LabExtended.Core.Pooling.Pools;
using LabExtended.Events;
using NorthwoodLib.Pools;
using RemoteAdmin;
using UserSettings.ServerSpecific;

namespace LabExtended.API;

public class ExPlayer : Player, IDisposable
{
    internal static Dictionary<string, string> preauthData = new(byte.MaxValue);
    internal static ExPlayer host;

    /// <summary>
    /// Gets a list of all players on the server.
    /// </summary>
    public static List<ExPlayer> Players { get; } = new(ExServer.MaxSlots * 2);

    /// <summary>
    /// Gets a list of all NPC players on the server.
    /// </summary>
    public static List<ExPlayer> NpcPlayers { get; } = new(byte.MaxValue);

    /// <summary>
    /// Gets a list of all player instances on the server (regular players, NPCs, LocalHub, HostHub).
    /// </summary>
    public static List<ExPlayer?> AllPlayers { get; } = new(ExServer.MaxSlots * 2);

    /// <summary>
    /// Gets a count of all players on the server.
    /// </summary>
    public new static int Count => Players.Count;

    /// <summary>
    /// Gets a count of all NPCs on the server.
    /// </summary>
    public static int NpcCount => NpcPlayers.Count;

    /// <summary>
    /// Gets a count of all players on the server (including real players, NPCs and the server player).
    /// </summary>
    public static int AllCount => AllPlayers.Count;

    /// <summary>
    /// Gets or sets ghosted player flags.
    /// </summary>
    public static int GhostedFlags { get; set; } = 0;

    /// <summary>
    /// Gets the host player.
    /// </summary>
    public new static ExPlayer? Host
    {
        get
        {
            if (host != null)
                return host;

            if (Server.Host != null)
                return host = (ExPlayer)Server.Host;

            if (ReferenceHub.TryGetHostHub(out var hostHub))
            {
                host = new(hostHub, SwitchContainer.GetNewNpcToggles(true));

                InternalEvents.InternalHandlePlayerJoin(host);

                Server.Host = host;
                return host;
            }

            throw new Exception("Could not fetch the host's ReferenceHub");
        }
    }

    #region Get
    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The <see cref="ReferenceHub"/> instance to get a player of.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public new static ExPlayer? Get(ReferenceHub hub)
        => AllPlayers.FirstOrDefault(p => p.ReferenceHub != null && p.ReferenceHub == hub);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="GameObject"/>.
    /// </summary>
    /// <param name="gameObject">The <see cref="UnityEngine.GameObject"/> instance to get a player of.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public new static ExPlayer? Get(GameObject gameObject)
        => AllPlayers.FirstOrDefault(p => p.GameObject != null && p.GameObject == gameObject);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="Collider"/>.
    /// </summary>
    /// <param name="collider">The <see cref="Collider"/> instance to a player of.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public static ExPlayer? Get(Collider collider)
        => collider?.transform is null
            ? null
            : Get(collider.transform.root != null ? collider.transform.root.gameObject : collider.transform.gameObject);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="NetworkConnection"/>.
    /// </summary>
    /// <param name="connection">The <see cref="NetworkConnection"/> instance to get a player of.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public static ExPlayer? Get(NetworkConnection connection)
        => AllPlayers.FirstOrDefault(p => p?.Connection != null && p.Connection == connection);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="NetworkIdentity"/>.
    /// </summary>
    /// <param name="identity">The <see cref="NetworkIdentity"/> instance to get a player of.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public new static ExPlayer? Get(NetworkIdentity identity)
        => Get(identity.netId);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="NetPeer"/>.
    /// </summary>
    /// <param name="peer">The <see cref="NetPeer"/> instance to get a player of.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public static ExPlayer Get(NetPeer peer)
        => Players.FirstOrDefault(p => p.Peer != null && p.Peer == peer);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="ItemBase"/>.
    /// </summary>
    /// <param name="item">The <see cref="ItemBase"/> instance to get a player of..</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public static ExPlayer? Get(ItemBase item)
        => Get(item.Owner);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="ItemPickupBase"/>.
    /// </summary>
    /// <param name="itemPickup">The <see cref="ItemPickupBase"/> instance to get a player of.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public static ExPlayer? Get(ItemPickupBase itemPickup)
        => Get(itemPickup.PreviousOwner);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="GameObject"/>.
    /// </summary>
    /// <param name="footprint">The <see cref="Footprinting.Footprint"/> instance to get a player of.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public static ExPlayer? Get(Footprint footprint)
        => footprint.IsSet && footprint.Hub ? Get(footprint.Hub) : null;

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance from a <see cref="ICommandSender"/>.
    /// </summary>
    /// <param name="sender">The <see cref="ICommandSender"/> instance to get a player of.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public new static ExPlayer? Get(ICommandSender sender)
        => sender is PlayerCommandSender playerSender ? Get(playerSender.ReferenceHub) : Host;

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance by a player ID.
    /// </summary>
    /// <param name="playerId">The player ID to find.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public new static ExPlayer? Get(int playerId)
        => AllPlayers.FirstOrDefault(p => p?.PlayerId == playerId);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance by a network ID.
    /// </summary>
    /// <param name="networkId">The network ID to find.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public new static ExPlayer? Get(uint networkId)
        => AllPlayers.FirstOrDefault(p => p?.NetworkId == networkId);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance by a connection ID.
    /// </summary>
    /// <param name="connectionId">The connection ID to find.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public static ExPlayer? GetByConnectionId(int connectionId)
        => AllPlayers.FirstOrDefault(p => p?.ConnectionId == connectionId);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance by a user ID.
    /// </summary>
    /// <param name="userId">The user ID to find.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public static ExPlayer? GetByUserId(string userId)
        => AllPlayers.FirstOrDefault(p => p?.UserId == userId || p?.ClearUserId == userId);

    /// <summary>
    /// Gets an <see cref="ExPlayer"/> instance by a user ID, player ID, network ID, IP or name.
    /// </summary>
    /// <param name="nameOrId">The network ID to find.</param>
    /// <param name="minNameScore">Name match precision.</param>
    /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
    public static ExPlayer? Get(string nameOrId, double minNameScore = 0.85)
    {
        for (var index = 0; index < AllPlayers.Count; index++)
        {
            var player = AllPlayers[index];

            if (player is null)
                continue;

            if (player.IsServer)
                continue;

            if (player.UserId == nameOrId || player.ClearUserId == nameOrId ||
                player.PlayerId.ToString() == nameOrId
                || player.ConnectionId.ToString() == nameOrId || player.NetworkId.ToString() == nameOrId)
                return player;

            if (player.Nickname.GetSimilarity(nameOrId) >= minNameScore)
                return player;
        }

        return null;
    }

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The instance to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGet(ReferenceHub hub, out ExPlayer? player)
        => (player = Get(hub)) != null;

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="GameObject"/>.
    /// </summary>
    /// <param name="obj">The instance to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGet(GameObject obj, out ExPlayer? player)
        => (player = Get(obj)) != null;

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="NetworkIdentity"/>.
    /// </summary>
    /// <param name="identity">The instance to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGet(NetworkIdentity identity, out ExPlayer? player)
        => (player = Get(identity)) != null;

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="NetworkConnection"/>.
    /// </summary>
    /// <param name="conn">The instance to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGet(NetworkConnection conn, out ExPlayer? player)
        => (player = Get(conn)) != null;

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="Collider"/>.
    /// </summary>
    /// <param name="collider">The instance to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGet(Collider collider, out ExPlayer? player)
        => (player = Get(collider)) != null;

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="ICommandSender"/>.
    /// </summary>
    /// <param name="sender">The instance to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGet(ICommandSender sender, out ExPlayer? player)
        => (player = Get(sender)) != null;

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by a player ID.
    /// </summary>
    /// <param name="playerId">The player ID to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGet(int playerId, out ExPlayer? player)
        => (player = Get(playerId)) != null;

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by a connection ID.
    /// </summary>
    /// <param name="connectionId">The connection ID to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGetByConnectionId(int connectionId, out ExPlayer? player)
        => (player = GetByConnectionId(connectionId)) != null;

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by a network ID.
    /// </summary>
    /// <param name="networkId">The network ID to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGet(uint networkId, out ExPlayer? player)
        => (player = Get(networkId)) != null;

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by a user ID.
    /// </summary>
    /// <param name="userId">The network ID to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGetByUserId(string userId, out ExPlayer? player)
        => (player = GetByUserId(userId)) != null;

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by their name, user ID, IP, network ID, player ID or connection ID.
    /// </summary>
    /// <param name="value">The value to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <param name="minScore">Name match precision.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGet(string value, double minScore, out ExPlayer? player)
        => (player = Get(value, minScore)) != null;

    /// <summary>
    /// Tries to get an <see cref="ExPlayer"/> instance by their name, user ID, IP, network ID, player ID or connection ID.
    /// </summary>
    /// <param name="value">The value to find.</param>
    /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
    public static bool TryGet(string value, out ExPlayer? player)
        => (player = Get(value)) != null;

    /// <summary>
    /// Gets a list of all players that match the predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A list of all matching players.</returns>
    public static IEnumerable<ExPlayer> Get(Func<ExPlayer, bool> predicate)
        => Players.Where(predicate);

    /// <summary>
    /// Gets a list of all players. in the specified <paramref name="team"/>.
    /// </summary>
    /// <param name="team">The team.</param>
    /// <returns>A list of all matching players.</returns>
    public static IEnumerable<ExPlayer> Get(Team team)
        => Get(n => n.Role.Team == team);

    /// <summary>
    /// Gets a list of all players in the specified <paramref name="faction"/>.
    /// </summary>
    /// <param name="faction">The faction.</param>
    /// <returns>A list of all matching players.</returns>
    public static IEnumerable<ExPlayer> Get(Faction faction)
        => Get(n => n.Role.Faction == faction);

    /// <summary>
    /// Gets a list of all players. with the specified <paramref name="role"/>.
    /// </summary>
    /// <param name="role">The role.</param>
    /// <returns>A list of all matching players.</returns>
    public static IEnumerable<ExPlayer> Get(RoleTypeId role)
        => Get(n => n.Role.Type == role);

    #endregion

    internal Dictionary<Type, CustomRoleInstance> customRoles = DictionaryPool<Type, CustomRoleInstance>.Shared.Rent();
    internal Dictionary<ItemBase, CustomItemInstance> customItems = DictionaryPool<ItemBase, CustomItemInstance>.Shared.Rent();
    
    internal Dictionary<string, SettingsMenu>? settingsMenuLookup = DictionaryPool<string, SettingsMenu>.Shared.Rent();
    internal Dictionary<string, SettingsEntry>? settingsIdLookup = DictionaryPool<string, SettingsEntry>.Shared.Rent();

    internal Dictionary<int, SettingsEntry>?
        settingsAssignedIdLookup = DictionaryPool<int, SettingsEntry>.Shared.Rent();

    public ExPlayer(string nickname) : this(DummyUtils.SpawnDummy(nickname), SwitchContainer.GetNewNpcToggles(true))
    {
        
    }

    public ExPlayer(ReferenceHub referenceHub) : this(referenceHub, SwitchContainer.GetNewPlayerToggles(true))
    {

    }

    public ExPlayer(ReferenceHub referenceHub, SwitchContainer toggles) : base(referenceHub)
    {
        if (referenceHub is null) throw new ArgumentNullException(nameof(referenceHub));

        Toggles = toggles ?? throw new ArgumentNullException(nameof(toggles));

        if (referenceHub.connectionToClient != null &&
            LiteNetLib4MirrorServer.Peers.TryPeekIndex(referenceHub.connectionToClient.connectionId, out var peer))
            Peer = peer;
        else if (referenceHub.connectionToServer != null &&
                 LiteNetLib4MirrorServer.Peers.TryPeekIndex(referenceHub.connectionToServer.connectionId, out peer))
            Peer = peer;

        Role = new(referenceHub.roleManager);
        Ammo = new(referenceHub.inventory);
        Stats = new(referenceHub.playerStats);
        Effects = new(referenceHub.playerEffectsController, this);

        Position = new(this);
        Rotation = new(this);

        Inventory = new(referenceHub.inventory);

        Subroutines = new(Role);

        TemporaryStorage = new(false, this);

        RemoteAdmin = new(this);
        Voice = new(this);

        if (!string.IsNullOrWhiteSpace(referenceHub.authManager.UserId))
        {
            if (PlayerStorage._persistentStorage.TryGetValue(referenceHub.authManager.UserId,
                    out var persistentStorage))
            {
                persistentStorage.Player = this;
                PersistentStorage = persistentStorage;
            }
            else
            {
                PersistentStorage = new(true, this);
            }

            CountryCode = preauthData.TryGetValue(referenceHub.authManager.UserId, out var region)
                ? region
                : string.Empty;
        }
        else
        {
            PersistentStorage = TemporaryStorage;
        }

        PersistentStorage.JoinTime = DateTime.Now;
        PersistentStorage.Lifes++;

        AllPlayers.Add(this);

        if (IsNpc)
        {
            NpcPlayers.Add(this);
        }
        else if (!IsServer)
        {
            Hints = ObjectPool<HintCache>.Shared.Rent(x => x.Player = this, () => new());
            
            Players.Add(this);
        }
        else
        {
            host = this;

            Toggles.ShouldSendPosition = false;
            
            Toggles.IsVisibleToScp939 = false;
            Toggles.IsVisibleInRemoteAdmin = false;
            Toggles.IsVisibleInSpectatorList = false;
        }
    }

    /// <summary>
    /// Gets the player's network peer.
    /// <para><b>null for NPCs!</b></para>
    /// </summary>
    public NetPeer? Peer { get; }

    /// <summary>
    /// Gets the player's hint cache.
    /// <para><i>null for players that cannot receive hints (ie. NPCs and the server player).</i></para>
    /// </summary>
    public HintCache Hints { get; }

    /// <summary>
    /// Gets the player's position container.
    /// </summary>
    public new PositionContainer Position { get; }

    /// <summary>
    /// Gets the player's rotation container.
    /// </summary>
    public new RotationContainer Rotation { get; }

    /// <summary>
    /// Gets the player's role container.
    /// </summary>
    public new RoleContainer Role { get; internal set; }

    /// <summary>
    /// Gets the player's ammo container.
    /// </summary>
    public new AmmoContainer Ammo { get; internal set; }

    /// <summary>
    /// Gets the player's stats container.
    /// </summary>
    public StatsContainer Stats { get; internal set; }

    /// <summary>
    /// Gets the player's effect container.
    /// </summary>
    public EffectContainer? Effects { get; internal set; }

    /// <summary>
    /// Gets the player's inventory container.
    /// </summary>
    public new InventoryContainer? Inventory { get; internal set; }

    /// <summary>
    /// Gets the player's subroutine container.
    /// </summary>
    public SubroutineContainer Subroutines { get; internal set; }

    /// <summary>
    /// Gets the player's voice chat controller.
    /// </summary>
    public VoiceController? Voice { get; internal set; }

    /// <summary>
    /// Gets the player's Remote Admin controller.
    /// </summary>
    public RemoteAdminController? RemoteAdmin { get; internal set; }

    /// <summary>
    /// Gets the player's temporary storage.
    /// </summary>
    public PlayerStorage? TemporaryStorage { get; internal set; }

    /// <summary>
    /// Gets the player's persistent storage. <i>(persistent until the next server restart)</i>
    /// </summary>
    public PlayerStorage? PersistentStorage { get; internal set; }

    /// <summary>
    /// Gets the player's toggles.
    /// </summary>
    public SwitchContainer Toggles { get; internal set; }

    /// <summary>
    /// Gets the player's ghost bit used in <see cref="PersonalGhostFlags"/> and <see cref="GhostedFlags"/>.
    /// </summary>
    public int GhostBit => 1 << PlayerId;

    /// <summary>
    /// Gets or sets personal ghost flags.
    /// </summary>
    public int PersonalGhostFlags { get; set; } = 0;

    /// <summary>
    /// Gets the player's network latency. <i>(-1 for NPCs)</i>
    /// </summary>
    public int Ping => Peer?.Ping ?? -1;

    /// <summary>
    /// Gets the player's network trip time. <i>(-1 for NPCs)</i>
    /// </summary>
    public int TripTime => Peer?._avgRtt ?? -1;

    /// <summary>
    /// Gets the player's network connection ID.
    /// </summary>
    public int ConnectionId => (ClientConnection ?? Connection)?.connectionId ?? -1;

    /// <summary>
    /// Gets the player's network identity.
    /// </summary>
    public NetworkIdentity? Identity => ReferenceHub?.connectionToClient?.identity;

    /// <summary>
    /// Gets the player's client connection.
    /// </summary>
    public NetworkConnectionToClient? ClientConnection => ReferenceHub?.connectionToClient;

    /// <summary>
    /// Gets a list of personal hint elements.
    /// </summary>
    public HashSet<PersonalHintElement> HintElements { get; internal set; } =
        HashSetPool<PersonalHintElement>.Shared.Rent();

    /// <summary>
    /// Gets the player's sent role cache.
    /// </summary>
    public Dictionary<uint, RoleTypeId>? SentRoles { get; internal set; } =
        DictionaryPool<uint, RoleTypeId>.Shared.Rent();

    /// <summary>
    /// Gets the player's sent positions cache.
    /// </summary>
    public Dictionary<uint, PositionData>? SentPositions { get; internal set; } =
        DictionaryPool<uint, PositionData>.Shared.Rent();

    /// <summary>
    /// Gets the currently spectated player.
    /// </summary>
    public ExPlayer SpectatedPlayer => Players.FirstOrDefault(p => p.IsSpectatedBy(this));

    /// <summary>
    /// Gets or sets the player that this player was disarmed by.
    /// </summary>
    public ExPlayer? Disarmer
    {
        get => Get(DisarmedPlayers.Entries.FirstOrDefault(e => e.DisarmedPlayer == NetworkId).Disarmer);
        set => ReferenceHub.inventory.SetDisarmedStatus(value?.ReferenceHub.inventory ?? null);
    }

    /// <summary>
    /// Gets a list of players that are currently spectating this player.
    /// </summary>
    public IEnumerator<ExPlayer> SpectatingPlayers
    {
        get
        {
            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                
                if (!player.IsSpectating(this))
                    continue;
                
                yield return player;
            }
        }
    }

    /// <summary>
    /// Gets the SCP-079 camera this player is currently using.
    /// </summary>
    public new Camera Camera => ExMap.GetCamera(Subroutines.Scp079CurrentCameraSync?.CurrentCamera);

    /// <summary>
    /// Gets the player's <see cref="UnityEngine.Transform"/>.
    /// </summary>
    public Transform Transform => ReferenceHub.transform;

    /// <summary>
    /// Gets the player's camera's <see cref="UnityEngine.Transform"/>.
    /// </summary>
    public Transform CameraTransform => ReferenceHub.PlayerCameraReference;

    /// <summary>
    /// Gets the player's <see cref="Footprinting.Footprint"/>.
    /// </summary>
    public Footprint Footprint => new(ReferenceHub);

    /// <summary>
    /// Gets the time of the player joining.
    /// </summary>
    public DateTime JoinTime => PersistentStorage.JoinTime;

    /// <summary>
    /// Gets the player's last received user settings report.
    /// </summary>
    public SSSUserStatusReport? SettingsReport { get; internal set; }

    /// <summary>
    /// Gets or sets the player's model scale.
    /// </summary>
    public Vector3 Scale
    {
        get => ReferenceHub.transform.localScale;
        set
        {
            if (value == ReferenceHub.transform.localScale)
                return;

            ReferenceHub.transform.localScale = value;

            Players.ForEach(target => Identity.SendSpawnMessage(target.Connection));
        }
    }

    /// <summary>
    /// Gets or sets icons that will be forced in the player list for this player.
    /// </summary>
    public RemoteAdminIconType RemoteAdminForcedIcons { get; set; } = RemoteAdminIconType.None;

    /// <summary>
    /// Gets the player's active Remote Admin panel icons.
    /// </summary>
    public RemoteAdminIconType RemoteAdminActiveIcons
    {
        get
        {
            if (RemoteAdminForcedIcons != RemoteAdminIconType.None)
                return RemoteAdminForcedIcons;

            var icons = RemoteAdminIconType.None;

            if (IsInOverwatch)
                icons |= RemoteAdminIconType.OverwatchIcon;

            if (IsMuted || IsIntercomMuted)
                icons |= RemoteAdminIconType.MutedIcon;

            return icons;
        }
    }

    /// <summary>
    /// Gets the player's connection state <i>(always <see cref="ConnectionState.Disconnected"/> for NPC players)</i>.
    /// </summary>
    public ConnectionState ConnectionState => Peer?.ConnectionState ?? ConnectionState.Disconnected;

    /// <summary>
    /// Gets the player's instance mode.
    /// </summary>
    public ClientInstanceMode InstanceMode => ReferenceHub?.Mode ?? ClientInstanceMode.Unverified;

    /// <summary>
    /// Gets or sets the player's enabled info area flags.
    /// </summary>
    public PlayerInfoArea InfoArea
    {
        get => ReferenceHub.nicknameSync.Network_playerInfoToShow;
        set => ReferenceHub.nicknameSync.Network_playerInfoToShow = value;
    }

    /// <summary>
    /// Gets or sets the player's Remote Admin permissions.
    /// </summary>
    public PlayerPermissions Permissions
    {
        get => (PlayerPermissions)ReferenceHub.serverRoles.Permissions;
        set
        {
            ReferenceHub.serverRoles.Permissions = (ulong)value;
            ReferenceHub.serverRoles.RefreshPermissions();
        }
    }

    /// <summary>
    /// Gets or sets the player's enabled voice mute flags.
    /// </summary>
    public VcMuteFlags VoiceMuteFlags
    {
        get => VoiceChatMutes.GetFlags(ReferenceHub);
        set => VoiceChatMutes.SetFlags(ReferenceHub, value);
    }

    /// <summary>
    /// Gets or sets this player's current voice pitch.
    /// </summary>
    public float VoicePitch
    {
        get => Voice?.Thread?.InstancePitch ?? 1f;
        set => Voice!.Thread!.InstancePitch = value;
    }

    /// <summary>
    /// Gets the player's screen's aspect ratio.
    /// </summary>
    public float ScreenAspectRatio => ReferenceHub.aspectRatioSync.AspectRatio;

    /// <summary>
    /// Gets the player's X axis screen edge.
    /// </summary>
    public float ScreenEdgeX => ReferenceHub.aspectRatioSync.XScreenEdge;

    /// <summary>
    /// Gets the player's screen's X and Y axis.
    /// </summary>
    public float ScreenXPlusY => ReferenceHub.aspectRatioSync.XplusY;

    /// <summary>
    /// Gets or sets the player's info area view range.
    /// </summary>
    public float InfoViewRange
    {
        get => ReferenceHub.nicknameSync.NetworkViewRange;
        set => ReferenceHub.nicknameSync.NetworkViewRange = value;
    }

    /// <summary>
    /// Gets or sets the player's kick power.
    /// </summary>
    public byte KickPower
    {
        get => ReferenceHub.serverRoles.Group?.KickPower ?? 0;
        set => ReferenceHub.serverRoles.Group!.KickPower = value;
    }

    /// <summary>
    /// Whether the player is a NPC.
    /// </summary>
    public new bool IsNpc => InstanceMode is ClientInstanceMode.Dummy;

    /// <summary>
    /// Whether the player is connected.
    /// </summary>
    public new bool IsOnline => Peer != null ? ConnectionState is ConnectionState.Connected : GameObject != null;

    /// <summary>
    /// Whether the player is disconnected.
    /// </summary>
    public new bool IsOffline => Peer != null ? ConnectionState != ConnectionState.Connected : GameObject is null;

    /// <summary>
    /// Whether the player is the server player.
    /// </summary>
    public new bool IsServer => InstanceMode is ClientInstanceMode.DedicatedServer || InstanceMode is ClientInstanceMode.Host || ReferenceHub.isLocalPlayer;

    /// <summary>
    /// Whether the player has fully connected.
    /// </summary>
    public bool IsVerified => InstanceMode is ClientInstanceMode.ReadyClient;

    /// <summary>
    /// Whether the player is still connecting.
    /// </summary>
    public bool IsUnverified => InstanceMode is ClientInstanceMode.Unverified;

    /// <summary>
    /// Whether the player is currently disarmed.
    /// </summary>
    public new bool IsDisarmed
    {
        get => ReferenceHub.inventory.IsDisarmed();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        set => ReferenceHub.inventory.SetDisarmedStatus(value ? Host.ReferenceHub.inventory : null);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    /// <summary>
    /// Whether the player is invisible to all players.
    /// </summary>
    public bool IsInvisible
    {
        get => (GhostedFlags & (1 << PlayerId)) != 0;
        set
        {
            if (value)
                GhostedFlags |= 1 << PlayerId;
            else
                GhostedFlags &= ~(1 << PlayerId);
        }
    }

    /// <summary>
    /// Whether the player is in the Overwatch mode.
    /// </summary>
    public bool IsInOverwatch
    {
        get => ReferenceHub.serverRoles.IsInOverwatch;
        set => ReferenceHub.serverRoles.IsInOverwatch = value;
    }

    /// <summary>
    /// Whether the player has NoClip permissions.
    /// </summary>
    public bool IsNoClipPermitted
    {
        get => FpcNoclip.IsPermitted(ReferenceHub);
        set
        {
            if (value)
                FpcNoclip.PermitPlayer(ReferenceHub);
            else
                FpcNoclip.UnpermitPlayer(ReferenceHub);
        }
    }

    /// <summary>
    /// Whether the player has a custom name applied.
    /// </summary>
    public bool HasCustomName => ReferenceHub.nicknameSync.HasCustomName;

    /// <summary>
    /// Whether the player has access to the Remote Admin panel.
    /// </summary>
    public bool HasRemoteAdminAccess => ReferenceHub.serverRoles.RemoteAdmin;

    /// <summary>
    /// Whether the player has the Remote Admin panel open.
    /// </summary>
    public bool HasRemoteAdminOpened => RemoteAdmin.IsRemoteAdminOpen;

    /// <summary>
    /// Whether the player has access to Admin Chat.
    /// </summary>
    public bool HasAdminChatAccess => ReferenceHub.serverRoles.AdminChatPerms;

    /// <summary>
    /// Gets the player's ISO 3166-1 alpha-2 country code (empty string for NPCs).
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets the player's user ID without it's identificator (<i>@steam, @discord etc.</i>)
    /// </summary>
    public string ClearUserId => UserIdHelper.GetClearId(UserId);

    /// <summary>
    /// Gets the player's user ID type (<i>steam, discord, northwood etc.</i>)
    /// </summary>
    public string UserIdType => UserIdHelper.GetIdType(UserId);

    /// <summary>
    /// Opens the player's Remote Admin panel.
    /// </summary>
    public void OpenRemoteAdmin()
        => ReferenceHub.serverRoles.TargetSetRemoteAdmin(true);

    /// <summary>
    /// Closes the player's Remote Admin Panel.
    /// </summary>
    public void CloseRemoteAdmin()
        => ReferenceHub.serverRoles.TargetSetRemoteAdmin(false);

    /// <summary>
    /// Whether or not this player is spectated by the specified player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns><see langword="true"/> if this player is spectated by <paramref name="player"/>.</returns>
    public bool IsSpectatedBy(ExPlayer player)
        => player != null && ReferenceHub.IsSpectatedBy(player.ReferenceHub);

    /// <summary>
    /// Whether or not this player is currently spectating the specified player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns><see langword="true"/> if <paramref name="player"/> is being spectated by this player.</returns>
    public bool IsSpectating(ExPlayer player)
        => player != null && player.ReferenceHub.IsSpectatedBy(ReferenceHub);

    /// <summary>
    /// Sends a new hint.
    /// </summary>
    /// <param name="content">Text of the hint.</param>
    /// <param name="duration">Duration of the hint.</param>
    public new void SendHint(string content, float duration)
        => SendHint(content, (ushort)duration, false);

    /// <summary>
    /// Sends a new hint.
    /// </summary>
    /// <param name="content">Text of the hint.</param>
    /// <param name="effects">Unused.</param>
    /// <param name="duration">Duration of the hint.</param>
    public new void SendHint(string content, HintEffect[] effects, float duration)
        => SendHint(content, (ushort)duration, false);

    /// <summary>
    /// Sends a new hint.
    /// </summary>
    /// <param name="content">Text of the hint.</param>
    /// <param name="duration">Duration of the hint.</param>
    /// <param name="isPriority">Whether to show the hint immediately.</param>
    public void SendHint(object content, ushort duration, bool isPriority = false)
        => this.ShowHint(content.ToString(), duration, isPriority);

    /// <summary>
    /// Sends a console message.
    /// </summary>
    /// <param name="content">Text of the message.</param>
    /// <param name="color">Color of the message (used in a color tag).</param>
    public void SendConsoleMessage(object content, string color = "red")
        => ReferenceHub.gameConsoleTransmission.SendToClient(content.ToString(), color);

    /// <summary>
    /// Sends a message into the "Request Data" section of the Remote Admin panel.
    /// </summary>
    /// <param name="content"></param>
    public void SendRemoteAdminInfo(object content)
        => SendRemoteAdminMessage($"$1 {content}", true, false);

    /// <summary>
    /// Sends a QR code to the player.
    /// </summary>
    /// <param name="data">Data of the QR code.</param>
    /// <param name="isBig">Whether or not to show the QR code fullscreen.</param>
    public void SendRemoteAdminQr(string data, bool isBig = false)
        => SendRemoteAdminMessage($"$2 {(isBig ? 1 : 0)} {data}", true, false);

    /// <summary>
    /// Sends text to the player's clipboard.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="type">Type of the data.</param>
    public void SendRemoteAdminClipboard(string data,
        RaClipboard.RaClipBoardType type = RaClipboard.RaClipBoardType.UserId)
        => SendRemoteAdminMessage($"$6 {(int)type} {data}", true, false);

    /// <summary>
    /// Sends a message to the player's text console part of the Remote Admin panel.
    /// </summary>
    /// <param name="content">The text of the message.</param>
    /// <param name="success">Whether or not to show the message as a warning.</param>
    /// <param name="show">Whether or not to show the message.</param>
    /// <param name="tag">The tag that will be shown in command name.</param>
    public void SendRemoteAdminMessage(object content, bool success = true, bool show = true, string tag = "")
    {
        if (content is null)
            return;

        if (IsServer)
        {
            ServerConsole.AddLog(content.ToString(), ConsoleColor.Green);
            return;
        }

        if (!HasRemoteAdminAccess)
            return;

        ReferenceHub.queryProcessor.SendToClient(content.ToString(), success, show, tag);
    }

    /// <summary>
    /// Sends a keybind to the player. Must have synchronized keybinds <i>(-allow-syncbind)</i> active for this to work.
    /// </summary>
    /// <param name="command">The command to bind to the key.</param>
    /// <param name="key">The key to bind the command to.</param>
    public void SendCommandKeyBind(string command, KeyCode key)
        => ReferenceHub.characterClassManager.TargetChangeCmdBinding(key, command);

    /// <summary>
    /// Whether or not the target player is invisible to ther player.
    /// </summary>
    /// <param name="otherPlayer">The player to check.</param>
    /// <returns><see langword="true"/> if the this player is invisible to the targeted player.</returns>
    public bool IsInvisibleTo(ExPlayer otherPlayer)
    {
        if (IsInvisible)
            return true;

        if (otherPlayer is null)
            throw new ArgumentNullException(nameof(otherPlayer));

        return (PersonalGhostFlags & (1 << otherPlayer.PlayerId)) != 0;
    }

    /// <summary>
    /// Makes this player invisible to the targeted player.
    /// </summary>
    /// <param name="player">The player to make this player invisible for.</param>
    public void MakeInvisibleFor(ExPlayer player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        var playerBit = 1 << player.PlayerId;

        if ((PersonalGhostFlags & playerBit) != 0)
            return;

        PersonalGhostFlags |= playerBit;
    }

    /// <summary>
    /// Makes this player visible to the targeted player.
    /// </summary>
    /// <param name="player">The player to make this player visible for.</param>
    public void MakeVisibleFor(ExPlayer player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        PersonalGhostFlags &= ~player.GhostBit;
    }

    /// <summary>
    /// Gets a component from the player's game object.
    /// </summary>
    /// <typeparam name="T">Type of the component to get.</typeparam>
    /// <returns>An instance of <see cref="T"/> if found, otherwise null.</returns>
    public T? GetComponent<T>()
        => GameObject.TryFindComponent<T>(out var component) ? component : default;

    /// <summary>
    /// Adds a new instance of a component to a player's game object or retrieves an existing one.
    /// </summary>
    /// <typeparam name="T">Type of the component to get / add.</typeparam>
    /// <returns>The instance of <see cref="T"/>.</returns>
    public T? GetOrAddComponent<T>()
        => GameObject.TryFindComponent<T>(out var component)
            ? component
            : (T)(object)GameObject.AddComponent(typeof(T));

    /// <summary>
    /// Removes a component instance of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <returns>true if the component was destroyed.</returns>
    public bool RemoveComponent<T>()
    {
        if (!GameObject.TryFindComponent<T>(out var component))
            return false;

        UnityEngine.Object.Destroy((UnityEngine.Object)(object)component!);
        return true;
    }

    /// <summary>
    /// Tries to find an active component instance.
    /// </summary>
    /// <param name="component">The found component instance.</param>
    /// <typeparam name="T">The type of the component to find.</typeparam>
    /// <returns>true if the component was found.</returns>
    public bool TryGetComponent<T>(out T component)
        => GameObject.TryFindComponent(out component);

    /// <summary>
    /// Sends data to the player's connection.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="channel">The channel to send the data to.</param>
    public void Send(ArraySegment<byte> data, int channel = 0)
        => (ClientConnection ?? Connection)?.Send(data, channel);

    /// <summary>
    /// Sends a message to the player's connection.
    /// </summary>
    /// <param name="message">Instance of the message.</param>
    /// <param name="channel">Channel to send the message to.</param>
    /// <typeparam name="T">The type of the message.</typeparam>
    public void Send<T>(T message, int channel = 0) where T : struct, NetworkMessage
        => (ClientConnection ?? Connection)?.Send(message, channel);

    public void Dispose()
    {
        if (host != null && host == this)
            host = null;

        GhostedFlags &= ~GhostBit;

        AllPlayers.Remove(this);

        if (IsNpc)
            NpcPlayers.Remove(this);
        else if (!IsServer)
            Players.Remove(this);

        if (!string.IsNullOrWhiteSpace(UserId))
            preauthData.Remove(UserId);

        AllPlayers.ForEach(ply =>
        {
            if (ply == null) 
                return;
            
            ply.SentRoles?.Remove(NetworkId);
            ply.SentPositions?.Remove(NetworkId);

            ply.PersonalGhostFlags &= ~GhostBit;
        });

        CustomCommand._continuedContexts.Remove(NetworkId);

        if (Hints != null)
            ObjectPool<HintCache>.Shared.Return(Hints);

        Effects?.Dispose();
        Effects = null;

        Inventory?.Dispose();
        Inventory = null;

        RemoteAdmin?.Dispose();
        RemoteAdmin = null;

        Voice?.Dispose();
        Voice = null;

        TemporaryStorage?.Dispose();
        TemporaryStorage = null;

        if (PersistentStorage != null)
        {
            if (!PlayerStorage._persistentStorage.ContainsKey(UserId))
                PlayerStorage._persistentStorage.Add(UserId, PersistentStorage);

            PersistentStorage.LeaveTime = DateTime.Now;
            PersistentStorage = null;
        }

        if (SentRoles != null)
        {
            DictionaryPool<uint, RoleTypeId>.Shared.Return(SentRoles);
            SentRoles = null;
        }

        if (SentPositions != null)
        {
            DictionaryPool<uint, PositionData>.Shared.Return(SentPositions);
            SentPositions = null;
        }

        if (settingsIdLookup != null)
        {
            DictionaryPool<string, SettingsEntry>.Shared.Return(settingsIdLookup);
            settingsIdLookup = null;
        }

        if (settingsAssignedIdLookup != null)
        {
            DictionaryPool<int, SettingsEntry>.Shared.Return(settingsAssignedIdLookup);
            settingsAssignedIdLookup = null;
        }

        if (settingsMenuLookup != null)
        {
            DictionaryPool<string, SettingsMenu>.Shared.Return(settingsMenuLookup);
            settingsMenuLookup = null;
        }

        if (customItems != null)
        {
            DictionaryPool<ItemBase, CustomItemInstance>.Shared.Return(customItems);
            customItems = null;
        }

        if (customRoles != null)
        {
            DictionaryPool<Type, CustomRoleInstance>.Shared.Return(customRoles);
            customRoles = null;
        }
        
        if (HintElements != null)
        {
            HintElements.ForEach(x => x.OnDisabled());

            HashSetPool<PersonalHintElement>.Shared.Return(HintElements);

            HintElements = null;
        }
    }

    #region Operators

    /// <inheritdoc/>
    public override string ToString()
        => $"{Role.Name} {Nickname} ({UserId})";

    /// <summary>
    /// Converts the <see cref="ReferenceHub"/> instance to it's corresponding <see cref="ExPlayer"/>.
    /// </summary>
    /// <param name="hub">The instance to convert.</param>
    public static implicit operator ExPlayer?(ReferenceHub hub)
        => Get(hub);

    /// <summary>
    /// Converts the <see cref="UnityEngine.GameObject"/> instance to it's corresponding <see cref="ExPlayer"/>.
    /// </summary>
    /// <param name="gameObject">The instance to convert.</param>
    public static implicit operator ExPlayer?(GameObject gameObject)
        => Get(gameObject);

    /// <summary>
    /// Converts the <see cref="Collider"/> instance to it's corresponding <see cref="ExPlayer"/>.
    /// </summary>
    /// <param name="collider">The instance to convert.</param>
    public static implicit operator ExPlayer?(Collider collider)
        => Get(collider);

    /// <summary>
    /// Converts the <see cref="NetworkIdentity"/> instance to it's corresponding <see cref="ExPlayer"/>.
    /// </summary>
    /// <param name="identity">The instance to convert.</param>
    public static implicit operator ExPlayer?(NetworkIdentity identity)
        => Get(identity);

    /// <summary>
    /// Converts the <see cref="NetworkConnection"/> instance to it's corresponding <see cref="ExPlayer"/>.
    /// </summary>
    /// <param name="connection">The instance to convert.</param>
    public static implicit operator ExPlayer?(NetworkConnection connection)
        => Get(connection);

    /// <summary>
    /// Converts the <see cref="NetPeer"/> instance to it's corresponding <see cref="ExPlayer"/>.
    /// </summary>
    /// <param name="peer">The instance to convert.</param>
    public static implicit operator ExPlayer(NetPeer peer)
        => Get(peer);

    /// <summary>
    /// Converts the <see cref="ItemBase"/> instance to it's corresponding <see cref="ExPlayer"/>.
    /// </summary>
    /// <param name="item">The instance to convert.</param>
    public static implicit operator ExPlayer?(ItemBase item)
        => Get(item);

    /// <summary>
    /// Converts the <see cref="ItemPickupBase"/> instance to it's corresponding <see cref="ExPlayer"/>.
    /// </summary>
    /// <param name="pickup">The instance to convert.</param>
    public static implicit operator ExPlayer?(ItemPickupBase pickup)
        => Get(pickup);

    /// <summary>
    /// Converts the <see cref="CommandSender"/> instance to it's corresponding <see cref="ExPlayer"/>.
    /// </summary>
    /// <param name="sender">The instance to convert.</param>
    public static implicit operator ExPlayer(CommandSender sender)
        => Get(sender);

    /// <summary>
    /// Converts the <see cref="PlayerRoleBase"/> instance to it's corresponding <see cref="ExPlayer"/>.
    /// </summary>
    /// <param name="role">The instance to convert.</param>
    public static implicit operator ExPlayer(PlayerRoleBase role)
        => (role is null || !role.TryGetOwner(out var owner) ? null : Get(owner));

    /// <summary>
    /// Converts the <see cref="Footprint"/> instance to it's corresponding <see cref="ExPlayer"/>.
    /// </summary>
    /// <param name="footprint">The instance to convert.</param>
    public static implicit operator ExPlayer?(Footprint footprint)
        => Get(footprint);

    /// <summary>
    /// Checks whether or not a specific player is valid.
    /// </summary>
    /// <param name="player">The player to check.</param>
    public static implicit operator bool(ExPlayer player)
        => player?.IsVerified ?? false;
    #endregion
}