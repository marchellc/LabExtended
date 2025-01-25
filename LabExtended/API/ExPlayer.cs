using CentralAuth;

using CommandSystem;

using Footprinting;

using InventorySystem.Disarming;

using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API.Containers;
using LabExtended.API.Enums;
using LabExtended.API.Hints;
using LabExtended.API.RemoteAdmin;

using LabExtended.Core;
using LabExtended.Core.Networking;

using LabExtended.Extensions;

using LabExtended.Utilities;
using LabExtended.Utilities.Values;

using LiteNetLib;

using Mirror;
using Mirror.LiteNetLib4Mirror;

using PlayerRoles;
using PlayerRoles.Spectating;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using PlayerStatsSystem;

using RelativePositioning;

using RemoteAdmin;
using RemoteAdmin.Communication;

using UnityEngine;

using Utils;

using VoiceChat;

using GameCore;

using LabApi.Features.Wrappers;

using LabExtended.API.Collections;
using LabExtended.API.CustomVoice;
using LabExtended.API.Hints.Elements;
using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Menus;
using LabExtended.Commands;
using LabExtended.Core.Networking.Synchronization.Position;
using LabExtended.Core.Pooling.Pools;

using NorthwoodLib.Pools;
using UserSettings.ServerSpecific;

namespace LabExtended.API
{
    /// <summary>
    /// A wrapper for the <see cref="ReferenceHub"/> class.
    /// </summary>
    public class ExPlayer
    {
        internal static readonly List<ExPlayer> players = new List<ExPlayer>(byte.MaxValue);
        internal static readonly List<ExPlayer> npcPlayers = new List<ExPlayer>(byte.MaxValue);
        internal static readonly List<ExPlayer> allPlayers = new List<ExPlayer>(byte.MaxValue);
        internal static readonly List<ExPlayer> ghostedPlayers = new List<ExPlayer>(byte.MaxValue);
        
        internal static readonly Dictionary<string, string> preauthData = new Dictionary<string, string>(byte.MaxValue);

        internal static ExPlayer? hostPlayer;

        /// <summary>
        /// Gets a list of all players on the server.
        /// </summary>
        public static IReadOnlyList<ExPlayer> Players => players;

        /// <summary>
        /// Gets a list of all NPC players on the server.
        /// </summary>
        public static IReadOnlyList<ExPlayer> NpcPlayers => npcPlayers;

        /// <summary>
        /// Gets a list of all player instances on the server (regular players, NPCs, LocalHub, HostHub).
        /// </summary>
        public static IReadOnlyList<ExPlayer> AllPlayers => allPlayers;

        /// <summary>
        /// Gets a count of all players on the server.
        /// </summary>
        public static int Count => players.Count;

        /// <summary>
        /// Gets a count of all NPCs on the server.
        /// </summary>
        public static int NpcCount => npcPlayers.Count;

        /// <summary>
        /// Gets a count of all players on the server (including real players, NPCs and the server player).
        /// </summary>
        public static int AllCount => allPlayers.Count;

        /// <summary>
        /// Gets the host player.
        /// </summary>
        public static ExPlayer Host
        {
            get
            {
                if (hostPlayer is null)
                {
                    if (!ReferenceHub.TryGetHostHub(out var hostHub))
                        throw new Exception($"Failed to fetch the host ReferenceHub.");

                    hostPlayer = new(hostHub);
                    
                    hostPlayer.Switches.IsVisibleInRemoteAdmin = false;
                    hostPlayer.Switches.IsVisibleInSpectatorList = false;

                    hostPlayer.Switches.ShouldSendPosition = false;
                    hostPlayer.Switches.ShouldReceivePositions = false;

                    allPlayers.Add(hostPlayer);
                }

                return hostPlayer;
            }
        }

        /// <summary>
        /// Gets a new <see cref="SwitchContainer"/> instance configured for Players copied from config.
        /// </summary>
        public static SwitchContainer PlayerSwitches 
        {
            get 
            {
                var switches = new SwitchContainer();

                switches.Copy(ApiLoader.ApiConfig.OtherSection.Players);
                return switches;
            }
        }

        /// <summary>
        /// Spawns a NPC player.
        /// </summary>
        /// <param name="nickName">The nickname to use for this NPC.</param>
        /// <returns>The spawned NPC player.</returns>
        public static ExPlayer SpawnNpc(string nickName = "Dummy")
        {
            var npcHub = DummyUtils.SpawnDummy(nickName);
            var npcPlayer = new ExPlayer(npcHub);

            npcHub.roleManager.ServerSetRole(RoleTypeId.None, RoleChangeReason.None, RoleSpawnFlags.All);

            npcPlayers.Add(npcPlayer);
            allPlayers.Add(npcPlayer);

            return npcPlayer;
        }

        #region Get
        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="ReferenceHub"/>.
        /// </summary>
        /// <param name="hub">The <see cref="ReferenceHub"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(ReferenceHub hub)
            => AllPlayers.FirstOrDefault(p => p.hub != null && p.hub == hub);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="LabApi.Features.Wrappers.Player"/>.
        /// </summary>
        /// <param name="player">The <see cref="LabApi.Features.Wrappers.Player"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(Player player)
            => Get(player.ReferenceHub);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="GameObject"/>.
        /// </summary>
        /// <param name="gameObject">The <see cref="UnityEngine.GameObject"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(GameObject gameObject)
            => AllPlayers.FirstOrDefault(p => p.GameObject != null && p.GameObject == gameObject);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="Collider"/>.
        /// </summary>
        /// <param name="collider">The <see cref="Collider"/> instance to a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(Collider collider)
        {
            if (collider is null)
                return null;

            if (collider.transform is null)
                return null;

            if (collider.transform.root != null)
                return Get(collider.transform.root.gameObject);

            return Get(collider.transform.gameObject);
        }

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance tied to the specified <see cref="NetworkConnection"/>.
        /// </summary>
        /// <param name="connection">The <see cref="NetworkConnection"/> instance to get a player of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(NetworkConnection connection)
            => AllPlayers.FirstOrDefault(p => p.Connection != null && p.Connection == connection);

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
            => Players.FirstOrDefault(p => p.Peer != null && p.Peer == peer);

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
            => footprint.IsSet && footprint.Hub ? Get(footprint.Hub) : null;

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
            => AllPlayers.FirstOrDefault(p => p.PlayerId == playerId);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance by a network ID.
        /// </summary>
        /// <param name="networkId">The network ID to find.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(uint networkId)
            => AllPlayers.FirstOrDefault(p => p.NetId == networkId);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance by a connection ID.
        /// </summary>
        /// <param name="connectionId">The connection ID to find.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer GetByConnectionId(int connectionId)
            => AllPlayers.FirstOrDefault(p => p.ConnectionId == connectionId);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance by a user ID.
        /// </summary>
        /// <param name="userId">The user ID to find.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer GetByUserId(string userId)
            => AllPlayers.FirstOrDefault(p => p.UserId == userId || p.ClearUserId == userId);

        /// <summary>
        /// Gets an <see cref="ExPlayer"/> instance by a user ID, player ID, network ID, IP or name.
        /// </summary>
        /// <param name="nameOrId">The network ID to find.</param>
        /// <param name="minNameScore">Name match precision.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static ExPlayer Get(string nameOrId, double minNameScore = 0.85)
        {
            foreach (var player in AllPlayers)
            {
                if (player.IsServer)
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
        /// Tries to get an <see cref="ExPlayer"/> instance by a <see cref="LabApi.Features.Wrappers.Player"/>.
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

        private NetPeer peer;
        private ReferenceHub hub;

        private RemoteAdminIconType forcedIcons;

        internal SSSUserStatusReport? sssReport;
        
        internal Dictionary<uint, RoleTypeId> sentRoles; // A custom way of sending roles to other players so it's easier to manage them.

        internal Dictionary<string, SettingsMenu> settingsMenuLookup;
        internal Dictionary<string, SettingsEntry> settingsIdLookup;
        internal Dictionary<int, SettingsEntry> settingsAssignedIdLookup;
        
        internal HashSet<ExPlayer> invisibility;
        internal HashSet<PersonalElement> elements;
        
        internal HintCache hintCache;

        /// <summary>
        /// Creates a new <see cref="ExPlayer"/> instance with default switches.
        /// </summary>
        /// <param name="hub"></param>
        public ExPlayer(ReferenceHub hub)
            : this(hub, PlayerSwitches) { }

        /// <summary>
        /// Creates a new <see cref="ExPlayer"/> instance with the specified switches.
        /// </summary>
        /// <param name="component">The <see cref="ReferenceHub"/> component instance.</param>
        /// <param name="switches">The <see cref="SwitchContainer"/> instance.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ExPlayer(ReferenceHub component, SwitchContainer switches)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (AllPlayers.Any(x => x.hub && x.hub == component))
                throw new Exception("Attempted to insert a duplicate ExPlayer instance");

            hub = component;

            forcedIcons = RemoteAdminIconType.None;

            sentRoles = DictionaryPool<uint, RoleTypeId>.Shared.Rent();
            
            settingsIdLookup = DictionaryPool<string, SettingsEntry>.Shared.Rent();
            settingsMenuLookup = DictionaryPool<string, SettingsMenu>.Shared.Rent();
            settingsAssignedIdLookup = DictionaryPool<int, SettingsEntry>.Shared.Rent();
            
            invisibility = HashSetPool<ExPlayer>.Shared.Rent();
            elements = HashSetPool<PersonalElement>.Shared.Rent();

            LiteNetLib4MirrorServer.Peers.TryPeekIndex(ConnectionId, out peer);

            InfoArea = new EnumValue<PlayerInfoArea>(() => hub.nicknameSync.Network_playerInfoToShow,
                value => hub.nicknameSync.Network_playerInfoToShow = value);
            MuteFlags = new EnumValue<VcMuteFlags>(() => VoiceChatMutes.GetFlags(hub),
                value => VoiceChatMutes.SetFlags(hub, value));

            ForcedRaIcons = new EnumValue<RemoteAdminIconType>(() => forcedIcons, value => forcedIcons = value);

            Role = new RoleContainer(component.roleManager);
            Ammo = new AmmoContainer(component.inventory);
            Stats = new StatsContainer(component.playerStats);
            Effects = new EffectContainer(component.playerEffectsController, this);

            Position = new PositionContainer(this);
            Rotation = new RotationContainer(this);

            Inventory = new InventoryContainer(component.inventory);

            Subroutines = new SubroutineContainer(Role);

            Switches = switches;

            TemporaryStorage = new PlayerStorage(false, this);

            RemoteAdmin = new RemoteAdminController(this);
            Voice = new VoiceController(this);

            if (!string.IsNullOrWhiteSpace(component.authManager.UserId))
            {
                if (PlayerStorage._persistentStorage.TryGetValue(component.authManager.UserId, out var persistentStorage))
                {
                    persistentStorage.Player = this;
                    PersistentStorage = persistentStorage;
                }
                else
                {
                    PersistentStorage = new PlayerStorage(true, this);
                }

                if (preauthData.TryGetValue(component.authManager.UserId, out var region))
                    CountryCode = region;
                else
                    CountryCode = string.Empty;
            }
            else
            {
                PersistentStorage = TemporaryStorage;
            }
            
            PersistentStorage.JoinTime = DateTime.Now;
            PersistentStorage.Lifes++;

            hintCache = ObjectPool<HintCache>.Shared.Rent(null, () => new HintCache());
            hintCache.Player = this;
            hintCache.RefreshRatio();

            allPlayers.Add(this);

            if (IsNpc)
            {
                npcPlayers.Add(this);
            }
            else if (!IsServer)
            {
                players.Add(this);
            }
            else
            {
                hostPlayer = this;
            }
        }

        /// <summary>
        /// Gets or sets this player's current voice pitch.
        /// </summary>
        public float VoicePitch
        {
            get => Voice?.Pitch?.InstancePitch ?? 1f;
            set => Voice!.Pitch!.InstancePitch = value;
        }
        
        /// <summary>
        /// Gets the time of the player joining.
        /// </summary>
        public DateTime JoinTime { get; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the icons that will always be shown in the Remote Admin player list.
        /// </summary>
        public EnumValue<RemoteAdminIconType> ForcedRaIcons { get; }

        /// <summary>
        /// Gets or sets the player's info area.
        /// </summary>
        public EnumValue<PlayerInfoArea> InfoArea { get; }

        /// <summary>
        /// Gets or sets the player's voice mute flags.
        /// </summary>
        public EnumValue<VcMuteFlags> MuteFlags { get; }

        /// <summary>
        /// Gets the player's <see cref="RoleContainer"/>.
        /// </summary>
        public RoleContainer Role { get; internal set; }

        /// <summary>
        /// Gets the player's <see cref="AmmoContainer"/>.
        /// </summary>
        public AmmoContainer Ammo { get; internal set; }

        /// <summary>
        /// Gets the player's <see cref="StatsContainer"/>.
        /// </summary>
        public StatsContainer Stats { get; internal set; }

        /// <summary>
        /// Gets the player's <see cref="EffectContainer"/>.
        /// </summary>
        public EffectContainer Effects { get; internal set; }

        /// <summary>
        /// Gets the player's <see cref="SwitchContainer"/>.
        /// </summary>
        public SwitchContainer Switches { get; internal set; }

        /// <summary>
        /// Gets the player's <see cref="PositionContainer"/>.
        /// </summary>
        public PositionContainer Position { get; internal set; }

        /// <summary>
        /// Gets the player's <see cref="RotationContainer"/>.
        /// </summary>
        public RotationContainer Rotation { get; internal set; }

        /// <summary>
        /// Gets the player's <see cref="InventoryContainer"/>.
        /// </summary>
        public InventoryContainer Inventory { get; internal set; }

        /// <summary>
        /// Gets the player's <see cref="SubroutineContainer"/>.
        /// </summary>
        public SubroutineContainer Subroutines { get; internal set; }
        
        /// <summary>
        /// Gets the player's temporary storage.
        /// </summary>
        public PlayerStorage TemporaryStorage { get; internal set; }
        
        /// <summary>
        /// Gets the player's persistent storage.
        /// </summary>
        public PlayerStorage PersistentStorage { get; internal set; }

        /// <summary>
        /// Gets the player's <see cref="ReferenceHub"/> component.
        /// </summary>
        public ReferenceHub Hub => hub;

        /// <summary>
        /// Gets the player's <see cref="UnityEngine.GameObject"/>.
        /// </summary>
        public GameObject GameObject => hub.gameObject;

        /// <summary>
        /// Gets the player's <see cref="NetPeer"/> (<see langword="null"/> if the player is an NPC).
        /// </summary>
        public NetPeer Peer => peer;

        /// <summary>
        /// Gets the player's <see cref="NetworkIdentity"/>.
        /// </summary>
        public NetworkIdentity Identity => hub.netIdentity;

        /// <summary>
        /// Gets the player's <see cref="NetworkConnection"/>.
        /// </summary>
        public NetworkConnectionToClient Connection => hub.connectionToClient;

        /// <summary>
        /// Gets the player's <see cref="RemoteAdminController"/>.
        /// </summary>
        public RemoteAdminController RemoteAdmin { get; internal set; }

        /// <summary>
        /// Gets the player's <see cref="VoiceController"/>.
        /// </summary>
        public VoiceController Voice { get; internal set; }

        /// <summary>
        /// Gets the currently spectated player.
        /// </summary>
        public ExPlayer SpectatedPlayer => Players.FirstOrDefault(p => p.IsSpectatedBy(this));

        /// <summary>
        /// Gets the SCP-079 camera this player is currently using.
        /// </summary>
        public Camera Camera => ExMap.GetCamera(Subroutines.Scp079CurrentCameraSync?.CurrentCamera);

        /// <summary>
        /// Gets the player's connection state <i>(always <see cref="ConnectionState.Disconnected"/> for NPC players)</i>.
        /// </summary>
        public ConnectionState ConnectionState => peer?.ConnectionState ?? ConnectionState.Disconnected;

        /// <summary>
        /// Gets a list of players that are currently spectating this player.
        /// </summary>
        public IEnumerable<ExPlayer> SpectatingPlayers => Players.Where(IsSpectatedBy);

        /// <summary>
        /// Gets a list of players that this player will be invisible to.
        /// </summary>
        public HashSet<ExPlayer> InvisibleToTargets => invisibility;

        /// <summary>
        /// Gets the player's <see cref="UnityEngine.Transform"/>.
        /// </summary>
        public Transform Transform => hub.transform;

        /// <summary>
        /// Gets the player's camera's <see cref="UnityEngine.Transform"/>.
        /// </summary>
        public Transform CameraTransform => hub.PlayerCameraReference;

        /// <summary>
        /// Gets the player's current velocity.
        /// </summary>
        public Vector3 Velocity => hub.GetVelocity();

        /// <summary>
        /// Gets the player's <see cref="Footprinting.Footprint"/>.
        /// </summary>
        public Footprint Footprint => new Footprint(Hub);

        /// <summary>
        /// Gets the player's network ID.
        /// </summary>
        public uint NetId => hub.netId;

        /// <summary>
        /// Gets the player's network latency. <i>(-1 for NPCs)</i>
        /// </summary>
        public int Ping => peer?.Ping ?? -1;

        /// <summary>
        /// Gets the player's network trip time. <i>(-1 for NPCs)</i>
        /// </summary>
        public int TripTime => peer?._avgRtt ?? -1;

        /// <summary>
        /// Gets the player's player ID.
        /// <para>You should <b>never</b> change this unless you know what you are doing.</para>
        /// </summary>
        public int PlayerId => hub.Network_playerId.Value;

        /// <summary>
        /// Gets the player's network connection ID.
        /// </summary>
        public int ConnectionId => hub.connectionToClient.connectionId;

        /// <summary>
        /// Gets the player's screen's aspect ratio.
        /// </summary>
        public float ScreenAspectRatio => hub.aspectRatioSync.AspectRatio;

        /// <summary>
        /// Gets the player's X axis screen edge.
        /// </summary>
        public float ScreenEdgeX => hub.aspectRatioSync.XScreenEdge;

        /// <summary>
        /// Gets the player's screen's X and Y axis.
        /// </summary>
        public float ScreenXPlusY => hub.aspectRatioSync.XplusY;

        /// <summary>
        /// Whether or not the player is a NPC.
        /// </summary>
        public bool IsNpc => InstanceMode is ClientInstanceMode.Dummy;

        /// <summary>
        /// Whether or not the player is connected.
        /// </summary>
        public bool IsOnline => peer != null ? ConnectionState is ConnectionState.Connected : GameObject != null;

        /// <summary>
        /// Whether or not the player is disconnected.
        /// </summary>
        public bool IsOffline => peer != null ? ConnectionState != ConnectionState.Connected : GameObject is null;

        /// <summary>
        /// Whether or not the player is the server player.
        /// </summary>
        public bool IsServer => InstanceMode is ClientInstanceMode.DedicatedServer || InstanceMode is ClientInstanceMode.Host;

        /// <summary>
        /// Whether or not the player has fully connected.
        /// </summary>
        public bool IsVerified => InstanceMode is ClientInstanceMode.ReadyClient;

        /// <summary>
        /// Whether or not the player is still connecting.
        /// </summary>
        public bool IsUnverified => InstanceMode is ClientInstanceMode.Unverified;

        /// <summary>
        /// Whether or not the player is currently speaking.
        /// </summary>
        public bool IsSpeaking => Role.VoiceModule?.ServerIsSending ?? false;

        /// <summary>
        /// Whether or not the player is a Northwood Staff member.
        /// </summary>
        public bool IsNorthwoodStaff => hub.authManager.NorthwoodStaff;

        /// <summary>
        /// Whether or not the player is a Northwood Global Moderator.
        /// </summary>
        public bool IsNorthwoodModerator => hub.authManager.RemoteAdminGlobalAccess;

        /// <summary>
        /// Whether or not the player has a custom name applied.
        /// </summary>
        public bool HasCustomName => hub.nicknameSync.HasCustomName;

        /// <summary>
        /// Whether or not the player has access to the Remote Admin panel.
        /// </summary>
        public bool HasRemoteAdminAccess => hub.serverRoles.RemoteAdmin;

        /// <summary>
        /// Whether or not the player has the Remote Admin panel open.
        /// </summary>
        public bool HasRemoteAdminOpened => RemoteAdmin.IsRemoteAdminOpen;

        /// <summary>
        /// Whether or not the player has access to Admin Chat.
        /// </summary>
        public bool HasAdminChatAccess => hub.serverRoles.AdminChatPerms;

        /// <summary>
        /// Whether or not the player has sent the Do Not Track flag.
        /// </summary>
        public bool DoNotTrack => hub.authManager.DoNotTrack;

        /// <summary>
        /// Gets the player's IP address.
        /// </summary>
        public string Address => hub.connectionToClient.address;

        /// <summary>
        /// Gets the player's ISO 3166-1 alpha-2 country code (empty string for NPCs).
        /// </summary>
        public string CountryCode { get; }

        /// <summary>
        /// Gets the player's user ID without it's identificator (<i>@steam, @discord etc.</i>)
        /// </summary>
        public string ClearUserId => UserIdHelper.GetClearId(UserId);

        /// <summary>
        /// Gets the player's user ID type (<i>steam, discord, northwood etc.</i>)
        /// </summary>
        public string UserIdType => UserIdHelper.GetIdType(UserId);

        /// <summary>
        /// Whether or not the player is currently disarmed.
        /// </summary>
        public bool IsDisarmed
        {
            get => hub.inventory.IsDisarmed();
            set
            {
                if (value)
                    hub.inventory.SetDisarmedStatus(Host.Hub.inventory);
                else
                    hub.inventory.SetDisarmedStatus(null);
            }
        }

        /// <summary>
        /// Whether or not the player is invisible to all players.
        /// </summary>
        public bool IsInvisible
        {
            get => ghostedPlayers.Contains(this);
            set
            {
                if (value)
                    ghostedPlayers.Add(this);
                else
                    ghostedPlayers.Remove(this);
            }
        }

        /// <summary>
        /// Whether or not the player is locally or globally muted.
        /// </summary>
        public bool IsMuted
        {
            get => MuteFlags.HasFlag(VcMuteFlags.LocalRegular) || MuteFlags.HasFlag(VcMuteFlags.GlobalRegular);
            set => MuteFlags.SetFlag(VcMuteFlags.LocalRegular, value);
        }

        /// <summary>
        /// Whether or not the player is Intercom muted.
        /// </summary>
        public bool IsIntercomMuted
        {
            get => MuteFlags.HasFlag(VcMuteFlags.LocalIntercom) || MuteFlags.HasFlag(VcMuteFlags.GlobalIntercom);
            set => MuteFlags.SetFlag(VcMuteFlags.LocalIntercom, value);
        }

        /// <summary>
        /// Whether or not the player is in the Overwatch mode.
        /// </summary>
        public bool IsInOverwatch
        {
            get => hub.serverRoles.IsInOverwatch;
            set => hub.serverRoles.IsInOverwatch = value;
        }

        /// <summary>
        /// Whether or not the player has NoClip permissions.
        /// </summary>
        public bool IsNoclipPermitted
        {
            get => FpcNoclip.IsPermitted(hub);
            set
            {
                if (value)
                    FpcNoclip.PermitPlayer(hub);
                else
                    FpcNoclip.UnpermitPlayer(hub);
            }
        }

        /// <summary>
        /// Whether or not the player has active NoClip.
        /// </summary>
        public bool IsNoclipActive 
        {
            get => Role.NoClip?.IsActive ?? false;
            set => Role.NoClip!.IsActive = value;
        }

        /// <summary>
        /// Whether or not the player has Bypass Mode active.
        /// </summary>
        public bool HasBypassMode
        {
            get => hub.serverRoles.BypassMode;
            set => hub.serverRoles.BypassMode = value;
        }

        /// <summary>
        /// Whether or not the player has God Mode active.
        /// </summary>
        public bool HasGodMode
        {
            get => hub.characterClassManager.GodMode;
            set => hub.characterClassManager.GodMode = value;
        }

        /// <summary>
        /// Gets or sets the player's info area view range.
        /// </summary>
        public float InfoViewRange
        {
            get => hub.nicknameSync.NetworkViewRange;
            set => hub.nicknameSync.NetworkViewRange = value;
        }

        /// <summary>
        /// Gets or sets the player's kick power.
        /// </summary>
        public byte KickPower
        {
            get => hub.serverRoles.Group?.KickPower ?? 0;
            set => hub.serverRoles.Group!.KickPower = value;
        }

        /// <summary>
        /// Gets or sets the player's user ID.
        /// </summary>
        public string UserId
        {
            get => hub.authManager.UserId;
            set => hub.authManager.UserId = value;
        }

        /// <summary>
        /// Gets or sets the player's nickname.
        /// </summary>
        public string Name
        {
            get => hub.nicknameSync.Network_myNickSync;
            set => hub.nicknameSync.Network_myNickSync = value;
        }

        /// <summary>
        /// Gets or sets the name that is displayed to other players.
        /// </summary>
        public string DisplayName
        {
            get => hub.nicknameSync.Network_displayName;
            set => hub.nicknameSync.Network_displayName = value;
        }

        /// <summary>
        /// Gets or sets the player's custom info.
        /// </summary>
        public string CustomInfo
        {
            get => hub.nicknameSync.Network_customPlayerInfoString;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || !ValidateCustomInfo(value))
                {
                    hub.nicknameSync.Network_customPlayerInfoString = string.Empty;
                    hub.nicknameSync.Network_playerInfoToShow &= ~PlayerInfoArea.CustomInfo;
                }
                else
                {
                    if (!hub.nicknameSync.Network_playerInfoToShow.Any(PlayerInfoArea.CustomInfo))
                        hub.nicknameSync.Network_playerInfoToShow |= PlayerInfoArea.CustomInfo;

                    hub.nicknameSync.Network_customPlayerInfoString = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the player's instance mode.
        /// </summary>
        public ClientInstanceMode InstanceMode
        {
            get => hub.authManager.InstanceMode;
            set => hub.authManager.InstanceMode = value;
        }

        /// <summary>
        /// Gets or sets the player's last voice channel.
        /// </summary>
        public VoiceChatChannel VoiceChannel
        {
            get => Role.VoiceModule?.CurrentChannel ?? VoiceChatChannel.None;
            set => Role.VoiceModule!.CurrentChannel = value;
        }

        /// <summary>
        /// Gets the player's Remote Admin panel icons.
        /// </summary>
        public RemoteAdminIconType RaIcons
        {
            get
            {
                if (forcedIcons != RemoteAdminIconType.None)
                    return forcedIcons;

                var icons = RemoteAdminIconType.None;

                if (IsInOverwatch)
                    icons |= RemoteAdminIconType.OverwatchIcon;

                if (IsMuted || IsIntercomMuted)
                    icons |= RemoteAdminIconType.MutedIcon;

                return icons;
            }
        }

        /// <summary>
        /// Gets or sets the player's Remote Admin permissions.
        /// </summary>
        public PlayerPermissions Permissions
        {
            get => (PlayerPermissions)Hub.serverRoles.Permissions;
            set
            {
                Hub.serverRoles.Permissions = (ulong)value;
                Hub.serverRoles.RefreshPermissions();
            }
        }

        /// <summary>
        /// Gets or sets the player's model scale.
        /// </summary>
        public Vector3 Scale
        {
            get => hub.transform.localScale;
            set => SetScale(value);
        }

        /// <summary>
        /// Gets or sets the player that this player was disarmed by.
        /// </summary>
        public ExPlayer Disarmer
        {
            get => Get(DisarmedPlayers.Entries.FirstOrDefault(e => e.DisarmedPlayer == NetId).Disarmer);
            set => Hub.inventory.SetDisarmedStatus(value?.Hub.inventory ?? null);
        }

        /// <summary>
        /// Gets or sets the player's Remote Admin group.
        /// </summary>
        public UserGroup Group
        {
            get => Hub.serverRoles.Group;
            set
            {
                Hub.serverRoles.SetGroup(value, true);
                Hub.serverRoles.RefreshPermissions();
            }
        }

        /// <summary>
        /// Opens the player's Remote Admin panel.
        /// </summary>
        public void OpenRemoteAdmin()
            => Hub.serverRoles.TargetSetRemoteAdmin(true);

        /// <summary>
        /// Closes the player's Remote Admin Panel.
        /// </summary>
        public void CloseRemoteAdmin()
            => Hub.serverRoles.TargetSetRemoteAdmin(false);

        /// <summary>
        /// Whether or not this player is spectated by the specified player.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns><see langword="true"/> if this player is spectated by <paramref name="player"/>.</returns>
        public bool IsSpectatedBy(ExPlayer player)
            => player != null && Hub.IsSpectatedBy(player.Hub);

        /// <summary>
        /// Whether or not this player is currently spectating the specified player.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns><see langword="true"/> if <paramref name="player"/> is being spectated by this player.</returns>
        public bool IsSpectating(ExPlayer player)
            => player != null && player.Hub.IsSpectatedBy(Hub);

        /// <summary>
        /// Deals damage to the player. <i>Use -1 to kill the player.</i>
        /// </summary>
        /// <param name="damageAmount">The amount of damage to deal.</param>
        /// <param name="reason">The reason of this damage.</param>
        /// <returns><see langword="true"/> if any damage was dealt.</returns>
        public bool Damage(float damageAmount, string reason = null)
            => Hub.playerStats.DealDamage(!string.IsNullOrWhiteSpace(reason) ? new CustomReasonDamageHandler(reason, damageAmount) : new UniversalDamageHandler(damageAmount, DeathTranslations.Bleeding));

        /// <summary>
        /// Kills the player.
        /// </summary>
        /// <param name="reason">The reason that will be displayed to the player.</param>
        public void Kill(string reason = null)
            => Hub.playerStats.DealDamage(!string.IsNullOrWhiteSpace(reason) ? new CustomReasonDamageHandler(reason, -1f) : new WarheadDamageHandler());

        /// <summary>
        /// Disconnects (kicks) the player.
        /// </summary>
        /// <param name="message">The message to show as the kick reason.</param>
        public void Disconnect(string message = null)
            => ServerConsole.Disconnect(Connection, message ?? string.Empty);

        /// <summary>
        /// "Explodes" the player (spawns a live grenade).
        /// </summary>
        /// <param name="attacker">The attacking player.</param>
        public void Explode(ExPlayer attacker = null, ExplosionType type = ExplosionType.Grenade)
            => ExplosionUtils.ServerExplode(Position.Position, attacker?.Footprint ?? Footprint, type);

        /// <summary>
        /// Clears all broadcasts.
        /// </summary>
        public void ClearBroadcasts()
            => global::Broadcast.Singleton?.TargetClearElements(Connection);

        /// <summary>
        /// Clears all hints.
        /// </summary>
        public void ClearHints()
            => Connection.Send(HintController.EmptyMessage);

        /// <summary>
        /// Sends a new hint.
        /// </summary>
        /// <param name="content">Text of the hint.</param>
        /// <param name="duration">Duration of the hint.</param>
        /// <param name="isPriority">Whether or not to show the hint immediately.</param>
        public void SendHint(object content, ushort duration, bool isPriority = false)
            => HintController.Show(this, content.ToString(), duration, isPriority);

        /// <summary>
        /// Sends a new broadcast-
        /// </summary>
        /// <param name="content">Text of the broadcast.</param>
        /// <param name="duration">Duration of the broadcast.</param>
        /// <param name="clearPrevious">Whether or not to clear previous broadcasts.</param>
        /// <param name="broadcastFlags">Broadcast options.</param>
        public void SendBroadcast(object content, ushort duration, bool clearPrevious = true, global::Broadcast.BroadcastFlags broadcastFlags = global::Broadcast.BroadcastFlags.Normal)
        {
            if (clearPrevious)
                global::Broadcast.Singleton?.TargetClearElements(Connection);

            global::Broadcast.Singleton?.TargetAddElement(Connection, content.ToString(), duration, broadcastFlags);
        }

        /// <summary>
        /// Sends a console message.
        /// </summary>
        /// <param name="content">Text of the message.</param>
        /// <param name="color">Color of the message (used in a color tag).</param>
        public void SendConsoleMessage(object content, string color = "red")
            => hub.gameConsoleTransmission.SendToClient(content.ToString(), color);

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
        public void SendRemoteAdminQR(string data, bool isBig = false)
            => SendRemoteAdminMessage($"$2 {(isBig ? 1 : 0)} {data}", true, false);

        /// <summary>
        /// Sends text to the player's clipboard.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="type">Type of the data.</param>
        public void SendRemoteAdminClipboard(string data, RaClipboard.RaClipBoardType type = RaClipboard.RaClipBoardType.UserId)
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

            hub.queryProcessor.SendToClient(content.ToString(), success, show, tag);
        }

        /// <summary>
        /// Sends a keybind to the player. Must have synchronized keybinds <i>(-allow-syncbind)</i> active for this to work.
        /// </summary>
        /// <param name="command">The command to bind to the key.</param>
        /// <param name="key">The key to bind the command to.</param>
        public void SendCommandKeyBind(string command, KeyCode key)
            => hub.characterClassManager.TargetChangeCmdBinding(key, command);

        /// <summary>
        /// Whether or not the target player is invisible to ther player.
        /// </summary>
        /// <param name="otherPlayer">The player to check.</param>
        /// <returns><see langword="true"/> if the this player is invisible to the targeted player.</returns>
        public bool IsInvisibleTo(ExPlayer otherPlayer)
        {
            if (IsInvisible)
                return true;

            if (invisibility.Contains(otherPlayer))
                return true;

            return false;
        }

        /// <summary>
        /// Makes this player invisible to the targeted player.
        /// </summary>
        /// <param name="player">The player to make this player invisible for.</param>
        public void MakeInvisibleFor(ExPlayer player)
            => invisibility.Add(player);

        /// <summary>
        /// Makes this player visible to the targeted player.
        /// </summary>
        /// <param name="player">The player to make this player visible for.</param>
        public void MakeVisibleFor(ExPlayer player)
            => invisibility.Remove(player);

        public T GetComponent<T>()
            => GameObject.TryFindComponent<T>(out var component) ? component : default;

        public T AddComponent<T>()
            => (T)(object)GameObject.AddComponent(typeof(T));

        public T GetOrAddComponent<T>()
            => GameObject.TryFindComponent<T>(out var component) ? component : (T)(object)GameObject.AddComponent(typeof(T));

        public bool RemoveComponent<T>()
        {
            if (GameObject.TryFindComponent<T>(out var component))
            {
                UnityEngine.Object.Destroy((UnityEngine.Object)(object)component);
                return true;
            }

            return false;
        }
        
        public bool TryGetComponent<T>(out T component)
            => GameObject.TryFindComponent<T>(out component);

        public void Send(ArraySegment<byte> data, int channel = 0)
            => Connection?.Send(data, channel);
        
        public void Send<T>(T message, int channel = 0) where T : struct, NetworkMessage
            => Connection?.Send(message, channel);

        #region Helper Methods
        private void SetScale(Vector3 scale)
        {
            if (scale == hub.transform.localScale)
                return;

            hub.transform.localScale = scale;

            foreach (var target in Players)
                MirrorMethods.SendSpawnMessage(hub.netIdentity, target.Connection);
        }

        // Taken from EXILED
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
                            ApiLog.Error("Player API", $"Custom info has been REJECTED. \nreason: (Bad text reject) \ntoken: {token} \nInfo: {customInfo}");
                            return false;
                        }
                        else if (!Misc.AllowedColors.ContainsValue(token.Substring(6, 7)))
                        {
                            ApiLog.Error("Player API", $"Custom info has been REJECTED. \nreason: (Bad color reject) \ntoken: {token} \nInfo: {customInfo}");
                            return false;
                        }
                    }
                    else if (token.StartsWith("#", StringComparison.Ordinal))
                    {
                        if (token.Length < 8 || token[7] != '>')
                        {
                            ApiLog.Error("Player API", $"Custom info has been REJECTED. \nreason: (Bad text reject) \ntoken: {token} \nInfo: {customInfo}");
                            return false;
                        }
                        else if (!Misc.AllowedColors.ContainsValue(token.Substring(0, 7)))
                        {
                            ApiLog.Error("Player API", $"Custom info has been REJECTED. \nreason: (Bad color reject) \ntoken: {token} \nInfo: {customInfo}");
                            return false;
                        }
                    }
                    else
                    {
                        ApiLog.Error("Player API", $"Custom info has been REJECTED. \nreason: (Bad color reject) \ntoken: {token} \nInfo: {customInfo}");
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

        #region Internal Methods
        internal RoleTypeId? InternalGetRoleForJoinedPlayer(ExPlayer joinedPlayer)
        {
            if (!Switches.IsVisibleInSpectatorList)
                return RoleTypeId.Spectator;

            return null;
        }

        internal FpcSyncData InternalGetNewSyncData(FpcSyncData prev, PlayerMovementState state, Vector3 position, bool grounded, FpcMouseLook mouseLook)
            => new FpcSyncData(prev, state, grounded, new RelativePosition(position), mouseLook);

        internal void Dispose()
        {
            if (hostPlayer != null && hostPlayer == this)
                hostPlayer = null;
            
            allPlayers.Remove(this);
            
            if (IsNpc)
                npcPlayers.Remove(this);
            else if (!IsServer)
                players.Remove(this);

            if (!string.IsNullOrWhiteSpace(UserId))
                preauthData.Remove(UserId);
            
            foreach (var helper in PlayerCollection.m_Handlers)
                helper.Remove(NetId);
            
            foreach (var other in allPlayers)
            {
                other.sentRoles?.Remove(NetId);
                other.invisibility?.Remove(this);
            }
            
            CustomCommand._continuedContexts.Remove(NetId);
            
            if (hintCache != null)
                ObjectPool<HintCache>.Shared.Return(hintCache);

            hintCache = null;

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

            if (sentRoles != null)
            {
                DictionaryPool<uint, RoleTypeId>.Shared.Return(sentRoles);
                sentRoles = null;
            }

            if (invisibility != null)
            {
                HashSetPool<ExPlayer>.Shared.Return(invisibility);
                invisibility = null;
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

            if (elements != null)
            {
                elements.ForEach(x => x.OnDisabled());
                
                HashSetPool<PersonalElement>.Shared.Return(elements);
                elements = null;
            }
        }

        #endregion

        #region Operators
        /// <inheritdoc/>
        public override string ToString()
            => $"{Role.Name} {Name} ({UserId})";

        /// <summary>
        /// Converts the <see cref="ReferenceHub"/> instance to it's corresponding <see cref="ExPlayer"/>.
        /// </summary>
        /// <param name="hub">The instance to convert.</param>
        public static implicit operator ExPlayer(ReferenceHub hub)
            => Get(hub);

        /// <summary>
        /// Converts the <see cref="UnityEngine.GameObject"/> instance to it's corresponding <see cref="ExPlayer"/>.
        /// </summary>
        /// <param name="gameObject">The instance to convert.</param>
        public static implicit operator ExPlayer(GameObject gameObject)
            => Get(gameObject);

        /// <summary>
        /// Converts the <see cref="Player"/> instance to it's corresponding <see cref="ExPlayer"/>.
        /// </summary>
        /// <param name="player">The instance to convert.</param>
        public static implicit operator ExPlayer(Player player)
            => Get(player);

        /// <summary>
        /// Converts the <see cref="Collider"/> instance to it's corresponding <see cref="ExPlayer"/>.
        /// </summary>
        /// <param name="collider">The instance to convert.</param>
        public static implicit operator ExPlayer(Collider collider)
            => Get(collider);

        /// <summary>
        /// Converts the <see cref="NetworkIdentity"/> instance to it's corresponding <see cref="ExPlayer"/>.
        /// </summary>
        /// <param name="identity">The instance to convert.</param>
        public static implicit operator ExPlayer(NetworkIdentity identity)
            => Get(identity);

        /// <summary>
        /// Converts the <see cref="NetworkConnection"/> instance to it's corresponding <see cref="ExPlayer"/>.
        /// </summary>
        /// <param name="connection">The instance to convert.</param>
        public static implicit operator ExPlayer(NetworkConnection connection)
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
        public static implicit operator ExPlayer(ItemBase item)
            => Get(item);

        /// <summary>
        /// Converts the <see cref="ItemPickupBase"/> instance to it's corresponding <see cref="ExPlayer"/>.
        /// </summary>
        /// <param name="pickup">The instance to convert.</param>
        public static implicit operator ExPlayer(ItemPickupBase pickup)
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
        public static implicit operator ExPlayer(Footprint footprint)
            => Get(footprint);

        /// <summary>
        /// Checks whether or not a specific player is valid.
        /// </summary>
        /// <param name="player">The player to check.</param>
        public static implicit operator bool(ExPlayer player)
            => player != null && player.IsVerified;
        #endregion
    }
}