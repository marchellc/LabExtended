﻿using CentralAuth;

using CommandSystem;

using CustomPlayerEffects;

using Decals;

using Footprinting;

using Interactables.Interobjects.DoorUtils;

using InventorySystem;
using InventorySystem.Disarming;

using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Containers;
using LabExtended.API.Enums;
using LabExtended.API.Hints;
using LabExtended.API.Modules;
using LabExtended.API.Npcs;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.Voice;

using LabExtended.Core;
using LabExtended.Core.Hooking;
using LabExtended.Core.Ticking;

using LabExtended.Events.Player;
using LabExtended.Extensions;
using LabExtended.Patches.Functions;

using LabExtended.Utilities;
using LabExtended.Utilities.Values;

using LiteNetLib;

using MapGeneration;

using Mirror;
using Mirror.LiteNetLib4Mirror;

using NorthwoodLib.Pools;

using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps;
using PlayerRoles.Spectating;

using PlayerStatsSystem;

using PluginAPI.Core;
using PluginAPI.Events;

using RelativePositioning;

using RemoteAdmin;
using RemoteAdmin.Communication;

using UnityEngine;

using Utils;

using VoiceChat;

namespace LabExtended.API
{
    /// <summary>
    /// A wrapper for the <see cref="ReferenceHub"/> class.
    /// </summary>
    public class ExPlayer : Module
    {
        static ExPlayer()
        {
            _players = new List<ExPlayer>();
            _npcPlayers = new List<ExPlayer>();
            _allPlayers = new List<ExPlayer>();

            TickManager.SubscribeTick(UpdateSentRoles, TickTimer.None, "Player Role Sync");
            TickManager.Init();
        }

        internal static readonly List<ExPlayer> _players;
        internal static readonly List<ExPlayer> _npcPlayers;
        internal static readonly List<ExPlayer> _allPlayers;

        internal static ExPlayer _hostPlayer;

        /// <summary>
        /// Gets a list of all players on the server.
        /// </summary>
        public static IReadOnlyList<ExPlayer> Players => _players;

        /// <summary>
        /// Gets a list of all NPC players on the server.
        /// </summary>
        public static IReadOnlyList<ExPlayer> NpcPlayers => _npcPlayers;

        /// <summary>
        /// Gets a count of all players on the server.
        /// </summary>
        public static int Count => _players.Count;

        /// <summary>
        /// Gets a count of all NPCs on the server.
        /// </summary>
        public static int NpcCount => _npcPlayers.Count;

        /// <summary>
        /// Gets the host player.
        /// </summary>
        public static ExPlayer Host
        {
            get
            {
                if (_hostPlayer is null)
                {
                    if (!ReferenceHub.TryGetHostHub(out var hostHub))
                        throw new Exception($"Failed to fetch the host ReferenceHub.");

                    _hostPlayer = new ExPlayer(hostHub);

                    _hostPlayer.Switches.IsVisibleInRemoteAdmin = false;
                    _hostPlayer.Switches.IsVisibleInSpectatorList = false;

                    _allPlayers.Add(_hostPlayer);
                }

                return _hostPlayer;
            }
        }

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="ReferenceHub"/>.
        /// </summary>
        /// <param name="hub">The <see cref="ReferenceHub"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(ReferenceHub hub)
            => _allPlayers.FirstOrDefault(p => p._hub != null && p._hub == hub);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="PluginAPI.Core.Player"/>.
        /// </summary>
        /// <param name="player">The <see cref="PluginAPI.Core.Player"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(Player player)
            => Get(player.ReferenceHub);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="GameObject"/>.
        /// </summary>
        /// <param name="gameObject">The <see cref="UnityEngine.GameObject"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(GameObject gameObject)
            => _allPlayers.FirstOrDefault(p => p.GameObject != null && p.GameObject == gameObject);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="Collider"/>.
        /// </summary>
        /// <param name="collider">The <see cref="Collider"/> instance to a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(Collider collider)
            => Get(collider.transform.root.gameObject);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="NetworkConnection"/>.
        /// </summary>
        /// <param name="connection">The <see cref="NetworkConnection"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(NetworkConnection connection)
            => _allPlayers.FirstOrDefault(p => p.Connection != null && p.Connection == connection);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="NetworkIdentity"/>.
        /// </summary>
        /// <param name="identity">The <see cref="NetworkIdentity"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(NetworkIdentity identity)
            => Get(identity.netId);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="NetPeer"/>.
        /// </summary>
        /// <param name="peer">The <see cref="NetPeer"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(NetPeer peer)
            => _players.FirstOrDefault(p => p.Peer != null && p.Peer == peer);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="ItemBase"/>.
        /// </summary>
        /// <param name="item">The <see cref="ItemBase"/> instance to get a player of..</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(ItemBase item)
            => Get(item.Owner);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="ItemPickupBase"/>.
        /// </summary>
        /// <param name="itemPickup">The <see cref="ItemPickupBase"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(ItemPickupBase itemPickup)
            => Get(itemPickup.PreviousOwner);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="GameObject"/>.
        /// </summary>
        /// <param name="footprint">The <see cref="Footprinting.Footprint"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(Footprint footprint)
            => footprint.IsSet ? Get(footprint.Hub) : null;

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance from a <see cref="ICommandSender"/>.
        /// </summary>
        /// <param name="sender">The <see cref="ICommandSender"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(ICommandSender sender)
            => sender is PlayerCommandSender playerSender ? Get(playerSender.ReferenceHub) : Host;

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance by a player ID.
        /// </summary>
        /// <param name="playerId">The player ID to find.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(int playerId)
            => _allPlayers.FirstOrDefault(p => p.PlayerId == playerId);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance by a network ID.
        /// </summary>
        /// <param name="networkId">The network ID to find.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(uint networkId)
            => _allPlayers.FirstOrDefault(p => p.NetId == networkId);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance by a connection ID.
        /// </summary>
        /// <param name="connectionId">The connection ID to find.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer GetByConnectionId(int connectionId)
            => _allPlayers.FirstOrDefault(p => p.ConnectionId == connectionId);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance by a user ID.
        /// </summary>
        /// <param name="userId">The user ID to find.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer GetByUserId(string userId)
            => _allPlayers.FirstOrDefault(p => p.UserId == userId || p.ClearUserId == userId);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance by a user ID, player ID, network ID, IP or name.
        /// </summary>
        /// <param name="nameOrId">The network ID to find.</param>
        /// <param name="minNameScore">Name match precision.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(string nameOrId, double minNameScore = 0.85)
        {
            foreach (var player in _allPlayers)
            {
                if (player.IsNpc || player.IsServer)
                    continue;

                if (player.UserId == nameOrId || player.ClearUserId == nameOrId || player.PlayerId.ToString() == nameOrId
                    || player.ConnectionId.ToString() == nameOrId || player.NetId.ToString() == nameOrId)
                    return player;

                if (player.Name.GetSimilarity(nameOrId) >= minNameScore)
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
        public static bool TryGet(ReferenceHub hub, out ExPlayer player)
            => (player = Get(hub)) != null;

        /// <summary>
        /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="PluginAPI.Core.Player"/>.
        /// </summary>
        /// <param name="apiPlayer">The instance to find.</param>
        /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(Player apiPlayer, out ExPlayer player)
            => (player = Get(apiPlayer)) != null;

        /// <summary>
        /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="GameObject"/>.
        /// </summary>
        /// <param name="player">The instance to find.</param>
        /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(GameObject obj, out ExPlayer player)
            => (player = Get(obj)) != null;

        /// <summary>
        /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="NetworkIdentity"/>.
        /// </summary>
        /// <param name="identity">The instance to find.</param>
        /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(NetworkIdentity identity, out ExPlayer player)
            => (player = Get(identity)) != null;

        /// <summary>
        /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="NetworkConnection"/>.
        /// </summary>
        /// <param name="conn">The instance to find.</param>
        /// <param name="handler">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(NetworkConnection conn, out ExPlayer player)
            => (player = Get(conn)) != null;

        /// <summary>
        /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="Collider"/>.
        /// </summary>
        /// <param name="collider">The instance to find.</param>
        /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(Collider collider, out ExPlayer player)
            => (player = Get(collider)) != null;

        /// <summary>
        /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="ICommandSender"/>.
        /// </summary>
        /// <param name="sender">The instance to find.</param>
        /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(ICommandSender sender, out ExPlayer player)
            => (player = Get(sender)) != null;

        /// <summary>
        /// Tries to get an <see cref="ExPlayer"/> instance by a player ID.
        /// </summary>
        /// <param name="playerId">The player ID to find.</param>
        /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(int playerId, out ExPlayer player)
            => (player = Get(playerId)) != null;

        /// <summary>
        /// Tries to get an <see cref="ExPlayer"/> instance by a connection ID.
        /// </summary>
        /// <param name="connectionId">The connection ID to find.</param>
        /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGetByConnectionId(int connectionId, out ExPlayer player)
            => (player = GetByConnectionId(connectionId)) != null;

        /// <summary>
        /// Tries to get an <see cref="ExPlayer"/> instance by a network ID.
        /// </summary>
        /// <param name="networkId">The network ID to find.</param>
        /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(uint networkId, out ExPlayer player)
            => (player = Get(networkId)) != null;

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by a user ID.
        /// </summary>
        /// <param name="userId">The network ID to find.</param>
        /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGetByUserId(string userId, out ExPlayer player)
            => (player = GetByUserId(userId)) != null;

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by their name, user ID, IP, network ID, player ID or connection ID.
        /// </summary>
        /// <param name="value">The value to find.</param>
        /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
        /// <param name="minScore">Name match precision.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(string value, double minScore, out ExPlayer player)
            => (player = Get(value, minScore)) != null;

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by their name, user ID, IP, network ID, player ID or connection ID.
        /// </summary>
        /// <param name="value">The value to find.</param>
        /// <param name="player">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="ExPlayer"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(string value, out ExPlayer player)
            => (player = Get(value)) != null;

        /// <summary>
        /// Gets a list of all players that match the predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A list of all matching players.</returns>
        public static IEnumerable<ExPlayer> Get(Func<ExPlayer, bool> predicate)
            => _players.Where(predicate);

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

        private NetPeer _peer;
        private ReferenceHub _hub;

        private RemoteAdminIconType _forcedIcons;

        internal RemoteAdminModule _raModule;
        internal PlayerStorageModule _storage;
        internal VoiceModule _voice;
        internal HintModule _hints;

        internal readonly LockedDictionary<int, RoleTypeId> _sentRoles; // A custom way of sending roles to other players so it's easier to manage them.
        internal readonly LockedHashSet<ItemPickupBase> _droppedItems;

        public ExPlayer(ReferenceHub hub) : this(hub, new SwitchContainer()) { }

        public ExPlayer(ReferenceHub component, SwitchContainer switches) : base()
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            _hub = component;
            _forcedIcons = RemoteAdminIconType.None;

            _sentRoles = new LockedDictionary<int, RoleTypeId>();
            _droppedItems = new LockedHashSet<ItemPickupBase>();

            LiteNetLib4MirrorServer.Peers.TryPeekIndex(ConnectionId, out _peer);

            InfoArea = new EnumValue<PlayerInfoArea>(() => _hub.nicknameSync.Network_playerInfoToShow, value => _hub.nicknameSync.Network_playerInfoToShow = value);
            MuteFlags = new EnumValue<VcMuteFlags>(() => VoiceChatMutes.GetFlags(_hub), value => VoiceChatMutes.SetFlags(_hub, value));

            ForcedRaIcons = new EnumValue<RemoteAdminIconType>(() => _forcedIcons, value => _forcedIcons = value);

            FakePosition = new FakeValue<Vector3>();
            FakeRole = new FakeValue<RoleTypeId>();

            Role = new RoleContainer(component.roleManager);
            Stats = new StatsContainer(component.playerStats);
            Subroutines = new SubroutineContainer(Role);

            Switches = switches;
        }

        /// <summary>
        /// Gets or sets this player's current voice pitch. <b>This is very CPU intensive and should be used sparely.</b>
        /// </summary>
        public float VoicePitch { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the icons that will always be shown in the Remote Admin player list.
        /// </summary>
        public EnumValue<RemoteAdminIconType> ForcedRaIcons { get; }

        /// <summary>
        /// Gets or sets the player's info area.
        /// </summary>
        public EnumValue<PlayerInfoArea> InfoArea { get; }
        public EnumValue<VcMuteFlags> MuteFlags { get; }

        public FakeValue<Vector3> FakePosition { get; }
        public FakeValue<RoleTypeId> FakeRole { get; }

        public NpcHandler NpcHandler { get; internal set; }

        public RoleContainer Role { get; }
        public StatsContainer Stats { get; }
        public SwitchContainer Switches { get; }
        public SubroutineContainer Subroutines { get; }

        public ReferenceHub Hub => _hub;
        public GameObject GameObject => _hub.gameObject;

        public NetPeer Peer => _peer;
        public NetworkIdentity Identity => _hub.netIdentity;
        public NetworkConnectionToClient Connection => _hub.connectionToClient;

        public RemoteAdminModule RemoteAdmin => _raModule;
        public PlayerStorageModule Storage => _storage;
        public VoiceModule Voice => _voice;
        public HintModule Hints => _hints;

        public ExPlayer SpectatedPlayer => _players.FirstOrDefault(p => p.IsSpectatedBy(this));
        public ExPlayer LookingAtPlayer => _players.FirstOrDefault(p => IsLookingAt(p));

        public ExPlayer ClosestPlayer => _players.Where(p => p.NetId != NetId).OrderBy(DistanceTo).FirstOrDefault();
        public ExPlayer ClosestScp => _players.Where(p => p.NetId != NetId && p.Role.IsScp).OrderBy(DistanceTo).FirstOrDefault();

        public Door ClosestDoor => ExMap.Doors.OrderBy(d => DistanceTo(d.Position)).FirstOrDefault();
        public Camera ClosestCamera => ExMap.Cameras.OrderBy(x => Vector3.Distance(x.Position, Position)).First();

        public RoomIdentifier Room => RoomIdUtils.RoomAtPosition(Position);
        public Elevator Elevator => ExMap.Elevators.FirstOrDefault(elevator => elevator.Contains(this));
        public Camera Camera => ExMap.GetCamera(Subroutines.Scp079CurrentCameraSync?.CurrentCamera);

        public ConnectionState ConnectionState => _peer?.ConnectionState ?? ConnectionState.Disconnected;

        public KeycardPermissions HeldKeycardPermissions => CurrentItem != null && CurrentItem is KeycardItem keycardItem ? keycardItem.Permissions : KeycardPermissions.None;

        public IEnumerable<ExPlayer> SpectatingPlayers => _players.Where(IsSpectatedBy);
        public IEnumerable<ExPlayer> PlayersInSight => _players.Where(p => p.IsInLineOfSight(this));

        public IEnumerable<Firearm> Firearms => _hub.inventory.UserInventory.Items.Values.Where<Firearm>();
        public IEnumerable<KeycardItem> Keycards => _hub.inventory.UserInventory.Items.Values.Where<KeycardItem>();

        public IEnumerable<StatusEffectBase> InactiveEffects => _hub.playerEffectsController.AllEffects.Where(e => !e.IsEnabled);
        public IEnumerable<StatusEffectBase> ActiveEffects => _hub.playerEffectsController.AllEffects.Where(e => e.IsEnabled);
        public IEnumerable<StatusEffectBase> AllEffects => _hub.playerEffectsController.AllEffects;

        public IEnumerable<VoiceModifier> VoiceModifiers => _voice.Modifiers;
        public IEnumerable<VoiceProfile> VoiceProfiles => _voice.Profiles;

        public Transform Transform => _hub.transform;
        public Transform CameraTransform => _hub.PlayerCameraReference;

        public Vector3 Velocity => _hub.GetVelocity();

        public Footprint Footprint => new Footprint(Hub);

        public uint NetId => _hub.netId;

        public int Ping => _peer?.Ping ?? -1;
        public int TripTime => _peer?._avgRtt ?? -1;
        public int ConnectionId => _hub.connectionToClient.connectionId;

        public float AspectRatio => _hub.aspectRatioSync.AspectRatio;

        public bool IsOnline => _peer != null ? ConnectionState is ConnectionState.Connected : GameObject != null;
        public bool IsOffline => _peer != null ? ConnectionState != ConnectionState.Connected : GameObject is null;

        public bool IsServer => InstanceMode is ClientInstanceMode.DedicatedServer || InstanceMode is ClientInstanceMode.Host;
        public bool IsVerified => InstanceMode is ClientInstanceMode.ReadyClient;
        public bool IsUnverified => InstanceMode is ClientInstanceMode.Unverified;

        public bool IsNpc => NpcHandler != null;
        public bool IsSpeaking => Role.VoiceModule?.ServerIsSending ?? false;

        public bool IsNorthwoodStaff => _hub.authManager.NorthwoodStaff;
        public bool IsNorthwoodModerator => _hub.authManager.RemoteAdminGlobalAccess;

        public bool HasCustomName => _hub.nicknameSync.HasCustomName;
        public bool HasAnyItems => _hub.inventory.UserInventory.Items.Count > 0;
        public bool HasAnyAmmo => _hub.inventory.UserInventory.ReserveAmmo.Any(p => p.Value > 0);
        public bool HasRemoteAdminAccess => _hub.serverRoles.RemoteAdmin;
        public bool HasRemoteAdminOpened => _raModule.IsRemoteAdminOpen;
        public bool HasAdminChatAccess => _hub.serverRoles.AdminChatPerms;

        public bool DoNotTrack => _hub.authManager.DoNotTrack;

        public string Address => _hub.connectionToClient.address;

        public string ClearUserId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UserId))
                    throw new Exception("Empty user ID.");

                if (!UserId.TrySplit('@', true, 2, out var idParts))
                    return UserId;

                return idParts[0];
            }
        }

        public string UserIdType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UserId))
                    throw new Exception("Empty user ID.");

                if (!UserId.TrySplit('@', true, 2, out var idParts))
                    throw new Exception($"Invalid user ID.");

                return idParts[1];
            }
        }

        public bool IsDisarmed
        {
            get => _hub.inventory.IsDisarmed();
            set
            {
                if (value)
                    _hub.inventory.SetDisarmedStatus(Host.Hub.inventory);
                else
                    _hub.inventory.SetDisarmedStatus(null);
            }
        }

        public bool IsInvisible
        {
            get => GhostModePatch.GhostedPlayers.Contains(NetId);
            set
            {
                if (value && !GhostModePatch.GhostedPlayers.Contains(NetId))
                    GhostModePatch.GhostedPlayers.Add(NetId);
                else if (!value && GhostModePatch.GhostedPlayers.Contains(NetId))
                    GhostModePatch.GhostedPlayers.Remove(NetId);
            }
        }

        public bool IsMuted
        {
            get => VoiceMutes.HasFlag(VcMuteFlags.LocalRegular) || VoiceMutes.HasFlag(VcMuteFlags.GlobalRegular);
            set => VoiceMutes = value ? VoiceMutes.Combine(VcMuteFlags.LocalRegular) : VoiceMutes.Remove(VcMuteFlags.LocalRegular);
        }

        public bool IsIntercomMuted
        {
            get => VoiceMutes.HasFlag(VcMuteFlags.LocalIntercom) || VoiceMutes.HasFlag(VcMuteFlags.GlobalIntercom);
            set => VoiceMutes = value ? VoiceMutes.Combine(VcMuteFlags.LocalIntercom) : VoiceMutes.Remove(VcMuteFlags.LocalIntercom);
        }

        public bool IsInOverwatch
        {
            get => _hub.serverRoles.IsInOverwatch;
            set => _hub.serverRoles.IsInOverwatch = value;
        }

        public bool IsNoclipPermitted
        {
            get => FpcNoclip.IsPermitted(_hub);
            set
            {
                if (value)
                    FpcNoclip.PermitPlayer(_hub);
                else
                    FpcNoclip.UnpermitPlayer(_hub);
            }
        }

        public bool HasBypassMode
        {
            get => _hub.serverRoles.BypassMode;
            set => _hub.serverRoles.BypassMode = value;
        }

        public bool HasGodMode
        {
            get => _hub.characterClassManager.GodMode;
            set => _hub.characterClassManager.GodMode = value;
        }

        public bool CanMove
        {
            get => !IsEffectActive<Ensnared>();
            set
            {
                if (value && !CanMove)
                    DisableEffect<Ensnared>();
                else if (!value && CanMove)
                    EnableEffect<Ensnared>(1);
            }
        }

        public int PlayerId
        {
            get => _hub.PlayerId;
            set => _hub._playerId = new RecyclablePlayerId(value);
        }

        public float InfoViewRange
        {
            get => _hub.nicknameSync.NetworkViewRange;
            set => _hub.nicknameSync.NetworkViewRange = value;
        }

        public byte KickPower
        {
            get => _hub.serverRoles.Group?.KickPower ?? 0;
            set => _hub.serverRoles.Group!.KickPower = value;
        }

        public string UserId
        {
            get => _hub.authManager.UserId;
            set => _hub.authManager.UserId = value;
        }

        public string Name
        {
            get => _hub.nicknameSync.Network_myNickSync;
            set => _hub.nicknameSync.Network_myNickSync = value;
        }

        public string DisplayName
        {
            get => _hub.nicknameSync.Network_displayName;
            set => _hub.nicknameSync.Network_displayName = value;
        }

        public string CustomInfo
        {
            get => _hub.nicknameSync.Network_customPlayerInfoString;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || !ValidateCustomInfo(value))
                {
                    _hub.nicknameSync.Network_customPlayerInfoString = string.Empty;
                    _hub.nicknameSync.Network_playerInfoToShow &= ~PlayerInfoArea.CustomInfo;
                }
                else
                {
                    if (!_hub.nicknameSync.Network_playerInfoToShow.Any(PlayerInfoArea.CustomInfo))
                        _hub.nicknameSync.Network_playerInfoToShow |= PlayerInfoArea.CustomInfo;

                    _hub.nicknameSync.Network_customPlayerInfoString = value;
                }
            }
        }

        public PlayerInfoArea InfoToShow
        {
            get => _hub.nicknameSync.Network_playerInfoToShow;
            set => _hub.nicknameSync.Network_playerInfoToShow = value;
        }

        public ClientInstanceMode InstanceMode
        {
            get => _hub.authManager.InstanceMode;
            set => _hub.authManager.InstanceMode = value;
        }

        public VcMuteFlags VoiceMutes
        {
            get => VoiceChatMutes.GetFlags(Hub);
            set => VoiceChatMutes.SetFlags(Hub, value);
        }

        public VoiceChatChannel VoiceChannel
        {
            get => Role.VoiceModule?.CurrentChannel ?? VoiceChatChannel.None;
            set => Role.VoiceModule!.CurrentChannel = value;
        }

        public RemoteAdminIconType RaIcons
        {
            get
            {
                if (_forcedIcons != RemoteAdminIconType.None)
                    return _forcedIcons;

                var icons = RemoteAdminIconType.None;

                if (IsInOverwatch)
                    icons |= RemoteAdminIconType.OverwatchIcon;

                if (IsMuted || IsIntercomMuted)
                    icons |= RemoteAdminIconType.MutedIcon;

                return icons;
            }
        }

        public KeycardPermissions AllKeycardPermissions
        {
            get
            {
                var perms = KeycardPermissions.None;

                foreach (var keycard in Keycards)
                {
                    var cardPerms = keycard.Permissions.GetFlags();

                    foreach (var cardPerm in cardPerms)
                    {
                        if (!perms.HasFlagFast(cardPerm))
                            perms |= cardPerm;
                    }
                }

                return perms;
            }
        }

        public IEnumerable<ItemBase> Items
        {
            get => _hub.inventory.UserInventory.Items.Values;
            set => SetItemList(value);
        }

        public IEnumerable<ItemType> ItemTypes
        {
            get => _hub.inventory.UserInventory.Items.Values.Select(p => p.ItemTypeId);
            set => ClearInventory(value);
        }

        public Vector3 Scale
        {
            get => _hub.transform.localScale;
            set => SetScale(value);
        }

        public Vector3 Position
        {
            get => _hub.transform.position;
            set => _hub.TryOverridePosition(value, Vector3.zero);
        }

        public Quaternion Rotation
        {
            get => _hub.transform.rotation;
            set => _hub.TryOverridePosition(Position, value.eulerAngles);
        }

        public RelativePosition RelativePosition
        {
            get => Role.Motor?.ReceivedPosition ?? new RelativePosition(Position);
            set => Position = value.Position;
        }

        public ItemBase CurrentItem
        {
            get => _hub.inventory.CurInstance;
            set => _hub.inventory.CurInstance = value;
        }

        public ItemType CurrentItemType
        {
            get => CurrentItem?.ItemTypeId ?? ItemType.None;
            set
            {
                if (value is ItemType.None)
                {
                    CurrentItemIdentifier = ItemIdentifier.None;
                    return;
                }

                var instance = value.GetItemInstance<ItemBase>();

                instance.SetupItem(Hub);

                CurrentItemIdentifier = new ItemIdentifier(instance.ItemTypeId, instance.ItemSerial);
            }
        }

        public ItemIdentifier CurrentItemIdentifier
        {
            get => _hub.inventory.NetworkCurItem;
            set => _hub.inventory.NetworkCurItem = value;
        }

        public Dictionary<ItemType, ushort> Ammo
        {
            get => _hub.inventory.UserInventory.ReserveAmmo;
            set
            {
                if (value is null)
                    _hub.inventory.UserInventory.ReserveAmmo.Clear();
                else
                    _hub.inventory.UserInventory.ReserveAmmo = value;

                _hub.inventory.ServerSendAmmo();
            }
        }

        public ExPlayer Disarmer
        {
            get => Get(DisarmedPlayers.Entries.FirstOrDefault(e => e.DisarmedPlayer == NetId).Disarmer);
            set => Hub.inventory.SetDisarmedStatus(value?.Hub.inventory ?? null);
        }

        public void OpenRemoteAdmin()
            => Hub.serverRoles.TargetSetRemoteAdmin(true);

        public void CloseRemoteAdmin()
            => Hub.serverRoles.TargetSetRemoteAdmin(false);

        public bool IsSpectatedBy(ExPlayer player)
            => Hub.IsSpectatedBy(player.Hub);

        public bool IsSpectating(ExPlayer player)
            => player.Hub.IsSpectatedBy(Hub);

        public bool Damage(float damageAmount, string reason = null)
            => Hub.playerStats.DealDamage(!string.IsNullOrWhiteSpace(reason) ? new CustomReasonDamageHandler(reason, damageAmount) : new UniversalDamageHandler(damageAmount, DeathTranslations.Bleeding));

        public void Kill(string reason = null)
            => Hub.playerStats.DealDamage(!string.IsNullOrWhiteSpace(reason) ? new CustomReasonDamageHandler(reason, -1f) : new WarheadDamageHandler());

        public void Disintegrate(ExPlayer attacker = null)
            => Hub.playerStats.DealDamage(new DisruptorDamageHandler(attacker?.Footprint ?? Footprint, -1f));

        public void Explode(ExPlayer attacker = null)
            => ExplosionUtils.ServerExplode(Position, attacker?.Footprint ?? Footprint);

        #region Messaging Methods
        public void ClearBroadcasts()
            => global::Broadcast.Singleton?.TargetClearElements(Connection);

        public void ClearHints()
            => Connection.Send(Hints.EmptyMessage);

        public void SendHint(object content, ushort duration, bool isPriority = false)
            => Hints.Show(content.ToString(), duration, isPriority);

        public void SendBroadcast(object content, ushort duration, bool clearPrevious = true, global::Broadcast.BroadcastFlags broadcastFlags = global::Broadcast.BroadcastFlags.Normal)
        {
            if (clearPrevious)
                global::Broadcast.Singleton?.TargetClearElements(Connection);

            global::Broadcast.Singleton?.TargetAddElement(Connection, content.ToString(), duration, broadcastFlags);
        }

        public void SendConsoleMessage(object content, string color = "red")
            => _hub.gameConsoleTransmission.SendToClient(content.ToString(), color);

        public void SendRemoteAdminInfo(object content)
            => SendRemoteAdminMessage($"$1 {content}", true, false);

        public void SendRemoteAdminQR(string data, bool isBig = false)
            => SendRemoteAdminMessage($"$2 {(isBig ? 1 : 0)} {data}", true, false);

        public void SendRemoteAdminClipboard(string data, RaClipboard.RaClipBoardType type = RaClipboard.RaClipBoardType.UserId)
            => SendRemoteAdminMessage($"$6 {(int)type} {data}", true, false);

        public void SendRemoteAdminMessage(object content, bool success = true, bool show = true, string tag = "")
        {
            if (!HasRemoteAdminAccess)
                return;

            _hub.queryProcessor.SendToClient(content.ToString(), success, show, tag);
        }

        public void SendKeyBind(string command, KeyCode key)
            => _hub.characterClassManager.TargetChangeCmdBinding(key, command);
        #endregion

        #region Positional Methods
        public IEnumerable<ExPlayer> GetPlayersInRange(float range)
            => _players.Where(p => p.NetId != NetId && p.Role.IsAlive && p.DistanceTo(this) <= range);

        public float DistanceTo(Vector3 position)
            => Vector3.Distance(Position, position);

        public float DistanceTo(ExPlayer player)
            => Vector3.Distance(player.Position, Position);

        public float DistanceTo(Transform transform)
            => Vector3.Distance(transform.position, Position);

        public float DistanceTo(GameObject gameObject)
            => Vector3.Distance(gameObject.transform.position, Position);
        #endregion

        #region Effect Methods
        public T GetEffect<T>() where T : StatusEffectBase
            => _hub.playerEffectsController.GetEffect<T>();

        public bool IsEffectActive<T>() where T : StatusEffectBase
            => GetEffect<T>().IsEnabled;

        public void EnableEffect<T>(byte intensity, float duration = 0f, bool addDurationIfActive = false) where T : StatusEffectBase
            => GetEffect<T>().ServerSetState(intensity, duration, addDurationIfActive);

        public void DisableEffect<T>() where T : StatusEffectBase
            => GetEffect<T>().ServerDisable();

        public byte GetEffectIntensity<T>() where T : StatusEffectBase
            => GetEffect<T>()?.Intensity ?? 0;

        public void SetEffectIntensity<T>(byte intensity) where T : StatusEffectBase
            => GetEffect<T>()!.Intensity = intensity;

        public void AddEffectIntensity<T>(byte intensity) where T : StatusEffectBase
        {
            var effect = GetEffect<T>();

            if (effect is null)
                return;

            effect.ServerSetState((byte)Mathf.Clamp(0f, effect.Intensity + intensity, 255f));
        }

        public void ProlongueEffect<T>(float addedDuration) where T : StatusEffectBase
        {
            var effect = GetEffect<T>();
            effect.ServerSetState(effect.Intensity, addedDuration, true);
        }

        public void ShortenEffect<T>(float removedDuration) where T : StatusEffectBase
        {
            var effect = GetEffect<T>();

            if (!effect.IsEnabled)
                return;

            if ((effect.Duration - removedDuration) <= 0f)
            {
                effect.ServerDisable();
                return;
            }

            effect.ServerSetState(effect.Intensity, effect.Duration - removedDuration);
        }

        public TimeSpan GetRemainingEffectDuration<T>() where T : StatusEffectBase
        {
            var effect = GetEffect<T>();

            if (!effect.IsEnabled)
                return TimeSpan.Zero;

            return TimeSpan.FromSeconds(effect.TimeLeft);
        }
        #endregion

        #region Vision Methods
        public bool IsInLineOfSight(ExPlayer player, float radius = 0.12f, float distance = 60f, bool countSpectating = true)
        {
            if (countSpectating && player.IsSpectatedBy(this))
                return true;

            var vision = VisionInformation.GetVisionInformation(Hub, CameraTransform, player.CameraTransform.position, radius, distance);

            if (vision.IsInLineOfSight || vision.IsLooking)
                return true;

            return false;
        }

        public bool IsLookingAt(ExPlayer player, float radius = 0.12f, float distance = 60f, bool countSpectating = true)
        {
            if (countSpectating && player.IsSpectatedBy(this))
                return true;

            var vision = VisionInformation.GetVisionInformation(Hub, CameraTransform, player.CameraTransform.position, radius, distance);

            if (vision.IsLooking)
                return true;

            return false;
        }

        public bool IsInvisibleTo(ExPlayer otherPlayer)
        {
            if (IsInvisible)
                return true;

            if (GhostModePatch.GhostedTo.TryGetValue(NetId, out var ghostedToPlayers) && ghostedToPlayers.Contains(otherPlayer.NetId))
                return true;

            return false;
        }

        public void MakeInvisibleFor(ExPlayer player)
        {
            if (!GhostModePatch.GhostedTo.TryGetValue(NetId, out var ghostedToPlayers))
                GhostModePatch.GhostedTo[NetId] = ghostedToPlayers = new LockedList<uint>();

            if (!ghostedToPlayers.Contains(player.NetId))
                ghostedToPlayers.Add(player.NetId);
        }

        public void MakeVisibleFor(ExPlayer player)
        {
            if (!GhostModePatch.GhostedTo.TryGetValue(NetId, out var ghostedToPlayers))
                GhostModePatch.GhostedTo[NetId] = ghostedToPlayers = new LockedList<uint>();

            if (ghostedToPlayers.Contains(player.NetId))
                ghostedToPlayers.Remove(player.NetId);
        }
        #endregion

        #region Inventory Methods
        public ItemBase AddItem(ItemType type)
           => Hub.inventory.ServerAddItem(type);

        public ItemPickupBase DropItem(ushort serial)
            => Hub.inventory.ServerDropItem(serial);

        public ItemPickupBase DropItem(ItemBase item)
            => Hub.inventory.ServerDropItem(item.ItemSerial);

        public ItemPickupBase DropHeldItem()
            => Hub.inventory.ServerDropItem(CurrentItemIdentifier.SerialNumber);

        public List<ItemPickupBase> DropItems(Predicate<ItemBase> predicate = null)
        {
            var list = new List<ItemPickupBase>();
            var items = ListPool<ItemBase>.Shared.Rent(Items);

            foreach (var item in items)
            {
                if (predicate != null && !predicate(item))
                    continue;

                var pickup = _hub.inventory.ServerDropItem(item.ItemSerial);

                if (pickup is null)
                    continue;

                list.Add(pickup);
            }

            ListPool<ItemBase>.Shared.Return(items);
            return list;
        }

        public List<ItemPickupBase> DropItems(params ItemType[] types)
            => DropItems(item => types.Contains(item.ItemTypeId));

        public IEnumerable<T> DropItems<T>(params ItemType[] types) where T : ItemPickupBase
            => DropItems(item => item.PickupDropModel != null && item.PickupDropModel is T).Where<T>(item => types.Length < 1 || types.Contains(item.Info.ItemId));

        public IEnumerable<ItemBase> GetItems(params ItemType[] types)
            => Items.Where(item => types.Contains(item.ItemTypeId));

        public IEnumerable<T> GetItems<T>() where T : ItemBase
            => Items.Where<T>();

        public IEnumerable<T> GetItems<T>(ItemType type) where T : ItemBase
            => Items.Where<T>(item => item.ItemTypeId == type);

        public bool HasItem(ItemType type)
            => Items.Any(it => it.ItemTypeId == type);

        public bool HasItems(ItemType type, int count)
            => Items.Count(it => it.ItemTypeId == type) >= count;

        public bool HasKeycardPermission(KeycardPermissions keycardPermissions)
            => Keycards.Any(card => card.Permissions.HasFlagFast(keycardPermissions));

        public int CountItems(ItemType type)
            => Items.Count(it => it.ItemTypeId == type);

        public void RemoveItem(ushort serial)
            => _hub.inventory.ServerRemoveItem(serial, null);

        public void RemoveItem(ItemBase item, ItemPickupBase pickup = null)
            => _hub.inventory.ServerRemoveItem(item.ItemSerial, pickup);

        public void RemoveHeldItem()
            => _hub.inventory.ServerRemoveItem(CurrentItemIdentifier.SerialNumber, CurrentItem?.PickupDropModel ?? null);

        public void RemoveItems(Predicate<ItemBase> predicate = null)
        {
            var items = ListPool<ItemBase>.Shared.Rent(Items);

            foreach (var item in items)
            {
                if (predicate != null && !predicate(item))
                    continue;

                _hub.inventory.ServerRemoveItem(item.ItemSerial, item.PickupDropModel);
            }

            ListPool<ItemBase>.Shared.Return(items);
        }

        public void RemoveItems(ItemType type, int count = -1)
        {
            var items = ListPool<ItemBase>.Shared.Rent(Items);
            var removed = 0;

            foreach (var item in items)
            {
                if (item.ItemTypeId != type)
                    continue;

                if (count > 0 && removed >= count)
                    break;

                removed++;
                _hub.inventory.ServerRemoveItem(item.ItemSerial, item.PickupDropModel);
            }

            ListPool<ItemBase>.Shared.Return(items);
        }

        public void ClearInventory(IEnumerable<ItemType> newInventory = null)
        {
            while (_hub.inventory.UserInventory.Items.Count > 0)
                _hub.inventory.ServerRemoveItem(_hub.inventory.UserInventory.Items.ElementAt(0).Key, null);

            if (newInventory != null)
            {
                foreach (var item in newInventory)
                {
                    if (item != ItemType.None)
                        AddItem(item);
                }
            }

            _hub.inventory.SendItemsNextFrame = true;
        }

        public bool AddOrSpawnItem(ItemType type)
        {
            if (_hub.inventory.UserInventory.Items.Count > 7)
            {
                if (type.TryGetItemPrefab(out var itemPrefab))
                {
                    _hub.inventory.ServerCreatePickup(itemPrefab, new PickupSyncInfo(type, itemPrefab.Weight));
                    return false;
                }

                return false;
            }

            return _hub.inventory.ServerAddItem(type);
        }

        public T ThrowItem<T>(ItemBase item) where T : ItemPickupBase
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            Hub.inventory.ServerRemoveItem(item.ItemSerial, item.PickupDropModel);
            return ThrowItem<T>(item.ItemTypeId, item.ItemSerial);
        }

        public T ThrowItem<T>(ItemType itemType, ushort? itemSerial = null) where T : ItemPickupBase
        {
            var itemPrefab = itemType.GetItemPrefab<ItemBase>();
            var pickupInstance = itemType.GetPickupInstance<T>(null, null, null, itemSerial, true);
            var pickupRigidbody = pickupInstance?.GetRigidbody();

            if (pickupRigidbody is null)
                return null;

            if (!EventManager.ExecuteEvent(new PlayerThrowItemEvent(Hub, itemPrefab, pickupRigidbody)))
                return pickupInstance;

            var velocity = Velocity;
            var angular = Vector3.Lerp(itemPrefab.ThrowSettings.RandomTorqueA, itemPrefab.ThrowSettings.RandomTorqueB, UnityEngine.Random.value);

            velocity = velocity / 3f + CameraTransform.forward * 6f * (Mathf.Clamp01(Mathf.InverseLerp(7f, 0.1f, pickupRigidbody.mass)) + 0.3f);

            velocity.x = Mathf.Max(Mathf.Abs(velocity.x), Mathf.Abs(velocity.x)) * (float)((!(velocity.x < 0f)) ? 1 : (-1));
            velocity.y = Mathf.Max(Mathf.Abs(velocity.y), Mathf.Abs(velocity.y)) * (float)((!(velocity.y < 0f)) ? 1 : (-1));
            velocity.z = Mathf.Max(Mathf.Abs(velocity.z), Mathf.Abs(velocity.z)) * (float)((!(velocity.z < 0f)) ? 1 : (-1));

            var throwingEv = new PlayerThrowingItemArgs(this, itemPrefab, pickupInstance, pickupRigidbody, CameraTransform.position, velocity, angular);

            if (!HookRunner.RunCancellable(throwingEv, true))
                return pickupInstance;

            pickupRigidbody.position = throwingEv.Position;
            pickupRigidbody.velocity = throwingEv.Velocity;
            pickupRigidbody.angularVelocity = throwingEv.AngularVelocity;

            if (pickupRigidbody.angularVelocity.magnitude > pickupRigidbody.maxAngularVelocity)
                pickupRigidbody.maxAngularVelocity = pickupRigidbody.angularVelocity.magnitude;

            return pickupInstance;
        }
        #endregion

        #region Ammo Methods
        public ushort GetAmmo(ItemType ammoType)
            => Ammo.TryGetValue(ammoType, out var amount) ? amount : (ushort)0;

        public void SetAmmo(ItemType ammoType, ushort amount)
        {
            Ammo[ammoType] = amount;
            _hub.inventory.ServerSendAmmo();
        }

        public void AddAmmo(ItemType ammoType, ushort amount)
        {
            Ammo[ammoType] = (ushort)Mathf.Clamp(GetAmmo(ammoType) + amount, 0f, ushort.MaxValue);
            _hub.inventory.ServerSendAmmo();
        }
        public void SubstractAmmo(ItemType ammoType, ushort amount)
        {
            Ammo[ammoType] = (ushort)Mathf.Clamp(GetAmmo(ammoType) - amount, 0f, ushort.MaxValue);
            _hub.inventory.ServerSendAmmo();
        }

        public bool HasAmmo(ItemType itemType, ushort minAmount = 1)
            => GetAmmo(itemType) >= minAmount;

        public void ClearAmmo()
        {
            Ammo.Clear();
            _hub.inventory.ServerSendAmmo();
        }

        public void ClearAmmo(ItemType ammoType)
        {
            Ammo.Remove(ammoType);
            _hub.inventory.ServerSendAmmo();
        }

        public List<AmmoPickup> DropAllAmmo()
        {
            var droppedAmmo = new List<AmmoPickup>();

            foreach (var ammo in Ammo.Keys)
                droppedAmmo.AddRange(_hub.inventory.ServerDropAmmo(ammo, ushort.MaxValue));

            return droppedAmmo;
        }

        public List<AmmoPickup> DropAllAmmo(ItemType ammoType, ushort amount = ushort.MaxValue)
            => _hub.inventory.ServerDropAmmo(ammoType, amount);
        #endregion

        #region Network Methods
        public void SendFakeSyncVar(NetworkIdentity behaviorOwner, Type targetType, string propertyName, object value)
        {
            if (!IsOnline)
                return;

            var writer = NetworkWriterPool.Get();
            var writer2 = NetworkWriterPool.Get();

            behaviorOwner.MakeCustomSyncWriter(targetType, null, CustomSyncVarGenerator, writer, writer2);

            Connection.Send(new EntityStateMessage
            {
                netId = behaviorOwner.netId,
                payload = writer.ToArraySegment(),
            });

            NetworkWriterPool.Return(writer);
            NetworkWriterPool.Return(writer2);

            void CustomSyncVarGenerator(NetworkWriter targetWriter)
            {
                targetWriter.WriteULong(NetworkUtils._syncVars[$"{targetType.Name}.{propertyName}"]);
                NetworkUtils._writerExtensions[value.GetType()]?.Invoke(null, new object[] { targetWriter, value });
            }
        }

        public void SendFakeTargetRpc(NetworkIdentity behaviorOwner, Type targetType, string rpcName, params object[] values)
        {
            if (!IsOnline)
                return;

            var writer = NetworkWriterPool.Get();

            foreach (var value in values)
                NetworkUtils._writerExtensions[value.GetType()].Invoke(null, new object[] { writer, value });

            var msg = new RpcMessage()
            {
                netId = behaviorOwner.netId,

                componentIndex = (byte)behaviorOwner.GetComponentIndex(targetType),
                functionHash = (ushort)NetworkUtils._rpcNames[$"{targetType.Name}.{rpcName}"].GetStableHashCode(),

                payload = writer.ToArraySegment(),
            };

            Connection.Send(msg);

            NetworkWriterPool.Return(writer);
        }

        public void SendFakeSyncObject(NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customAction)
        {
            if (!IsOnline)
                return;

            var writer = NetworkWriterPool.Get();
            var writer2 = NetworkWriterPool.Get();

            behaviorOwner.MakeCustomSyncWriter(targetType, customAction, null, writer, writer2);

            Connection.Send(new EntityStateMessage() { netId = behaviorOwner.netId, payload = writer.ToArraySegment() });

            NetworkWriterPool.Return(writer);
            NetworkWriterPool.Return(writer2);
        }
        #endregion

        #region Private Methods
        private void SetItemList(IEnumerable<ItemBase> items)
        {
            ClearInventory();

            if (items != null)
            {
                foreach (var item in items)
                    item?.SetupItem(Hub, false);
            }

            _hub.inventory.SendItemsNextFrame = true;
        }

        private void SetScale(Vector3 scale)
        {
            if (scale == _hub.transform.localScale)
                return;

            _hub.transform.localScale = scale;

            foreach (var target in _players)
                NetworkServer.SendSpawnMessage(_hub.netIdentity, target.Connection);
        }

        private static bool ValidateCustomInfo(string customInfo)
        {
            if (customInfo.Contains('<'))
            {
                foreach (var token in customInfo.Split('<'))
                {
                    if (token.StartsWith("/", StringComparison.Ordinal) ||
                        token.StartsWith("b>", StringComparison.Ordinal) ||
                        token.StartsWith("i>", StringComparison.Ordinal) ||
                        token.StartsWith("size=", StringComparison.Ordinal) ||
                        token.Length is 0)
                        continue;

                    if (token.StartsWith("color=", StringComparison.Ordinal))
                    {
                        if (token.Length < 14 || token[13] != '>')
                        {
                            Log.Error($"Custom info has been REJECTED. \nreason: (Bad text reject) \ntoken: {token} \nInfo: {customInfo}");
                            return false;
                        }
                        else if (!Misc.AllowedColors.ContainsValue(token.Substring(6, 7)))
                        {
                            Log.Error($"Custom info has been REJECTED. \nreason: (Bad color reject) \ntoken: {token} \nInfo: {customInfo}");
                            return false;
                        }
                    }
                    else if (token.StartsWith("#", StringComparison.Ordinal))
                    {
                        if (token.Length < 8 || token[7] != '>')
                        {
                            Log.Error($"Custom info has been REJECTED. \nreason: (Bad text reject) \ntoken: {token} \nInfo: {customInfo}");
                            return false;
                        }
                        else if (!Misc.AllowedColors.ContainsValue(token.Substring(0, 7)))
                        {
                            Log.Error($"Custom info has been REJECTED. \nreason: (Bad color reject) \ntoken: {token} \nInfo: {customInfo}");
                            return false;
                        }
                    }
                    else
                    {
                        Log.Error($"Custom info has been REJECTED. \nreason: (Bad color reject) \ntoken: {token} \nInfo: {customInfo}");
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

        #region Internal Methods
        internal RoleTypeId? GetRoleForJoinedPlayer(ExPlayer joinedPlayer)
        {
            if (!Switches.IsVisibleInSpectatorList)
                return RoleTypeId.Spectator;

            return null;
        }

        internal static void UpdateSentRoles()
        {
            foreach (var player in _allPlayers)
            {
                var curRoleId = player.Role.Type;

                if (player.Role.Role is IObfuscatedRole obfuscatedRole)
                    curRoleId = obfuscatedRole.GetRoleForUser(player.Hub);

                foreach (var other in _allPlayers)
                {
                    if (player.FakeRole.TryGetValue(other, out var fakedRole))
                        curRoleId = fakedRole;

                    if (!other.Role.IsAlive && !player.Switches.IsVisibleInSpectatorList)
                        curRoleId = RoleTypeId.Spectator;

                    if (player._sentRoles.TryGetValue(other.PlayerId, out var sentRole) && sentRole == curRoleId)
                        continue;

                    player._sentRoles[other.PlayerId] = curRoleId;
                    other.Connection.Send(new RoleSyncInfo(player.Hub, curRoleId, other.Hub));
                }
            }
        }
        #endregion
    }
}