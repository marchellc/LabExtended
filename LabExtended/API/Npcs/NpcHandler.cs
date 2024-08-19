using CentralAuth;

using Footprinting;

using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API.Containers;
using LabExtended.API.Modules;
using LabExtended.API.Npcs.Navigation;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities.Generation;

using MEC;

using Mirror;

using PlayerRoles;
using PlayerRoles.FirstPersonControl;

using PluginAPI.Core;

using UnityEngine;

namespace LabExtended.API.Npcs
{
    /// <summary>
    /// A class used to manage NPCs.
    /// </summary>
    public class NpcHandler
    {
        private static readonly List<NpcHandler> _spawnedNpcs = new List<NpcHandler>(); // A list of all known NPCs.
        private static readonly UniqueInt32Generator _npcIdGen = new UniqueInt32Generator(9000, 100000); // A unique ID generator ranging from 9000 to 1000.

        /// <summary>
        /// Gets a new <see cref="SwitchContainer"/> instance configured for NPCs.
        /// </summary>
        public static SwitchContainer NpcSwitches => new SwitchContainer
        {
            CanBePocketDimensionItemTarget = false,
            CanBeRespawned = false,
            CanBeScp049Target = false,
            CanBeStrangledBy3114 = false,
            CanBeResurrectedBy049 = false,
            CanBlockRoundEnd = false,
            CanCountAs079ExpTarget = false,
            CanBlockScp173 = false,
            CanTriggerScp096 = false,
            CanBeRecontainedAs079 = false,
            CanBeCapturedBy106 = false,
            CanBeConsumedByZombies = false,

            CanTriggerTesla = false,
            IsVisibleToScp939 = true,
            CanChangeRoles = false,

            IsVisibleInRemoteAdmin = false,
            IsVisibleInSpectatorList = false,

            ShouldReceivePositions = false
        };

        /// <summary>
        /// A list of all NPCs.
        /// </summary>
        public static IReadOnlyList<NpcHandler> Npcs => _spawnedNpcs;

        /// <summary>
        /// Gets a list of all spawned NPCs.
        /// </summary>
        public static IEnumerable<NpcHandler> SpawnedNpcs => _spawnedNpcs.Where(n => n.IsSpawned);

        /// <summary>
        /// Gets a list of all despawned NPCs.
        /// </summary>
        public static IEnumerable<NpcHandler> DespawnedNpcs => _spawnedNpcs.Where(n => !n.IsSpawned);

        /// <summary>
        /// The amount of spawned NPCs.
        /// </summary>
        public static int Count => _spawnedNpcs.Count;

        #region Get
        /// <summary>
        /// Gets a value indicating whether or not a given <see cref="ReferenceHub"/> is that of an NPC.
        /// </summary>
        /// <param name="hub">The <see cref="ReferenceHub"/> instance to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="hub"/> is an NPC, otherwise <see langword="false"/>.</returns>
        public static bool IsNpc(ReferenceHub hub)
            => Get(hub) != null;

        /// <summary>
        /// Gets a value indicating whether or not a given <see cref="PluginAPI.Core.Player"/> is an NPC.
        /// </summary>
        /// <param name="arg">The <see cref="PluginAPI.Core.Player"/> instance to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="arg"/> is an NPC, otherwise <see langword="false"/>.</returns>
        public static bool IsNpc(Player arg)
            => Get(arg) != null;

        /// <summary>
        /// Gets a value indicating whether or not a given <see cref="GameObject"/> is an NPC.
        /// </summary>
        /// <param name="arg">The <see cref="GameObject"/> instance to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="arg"/> is an NPC, otherwise <see langword="false"/>.</returns>
        public static bool IsNpc(GameObject arg)
            => Get(arg) != null;

        /// <summary>
        /// Gets a value indicating whether or not a given <see cref="Collider"/> is an NPC.
        /// </summary>
        /// <param name="arg">The <see cref="Collider"/> instance to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="arg"/> is an NPC, otherwise <see langword="false"/>.</returns>
        public static bool IsNpc(Collider arg)
            => Get(arg) != null;

        /// <summary>
        /// Gets a value indicating whether or not a given <see cref="NetworkConnection"/> is that of an NPC.
        /// </summary>
        /// <param name="arg">The <see cref="NetworkConnection"/> instance to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="arg"/> is an NPC, otherwise <see langword="false"/>.</returns>
        public static bool IsNpc(NetworkConnection arg)
            => Get(arg) != null;

        /// <summary>
        /// Gets a value indicating whether or not a given <see cref="NetworkIdentity"/> is that of an NPC.
        /// </summary>
        /// <param name="arg">The <see cref="NetworkIdentity"/> instance to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="arg"/> is an NPC, otherwise <see langword="false"/>.</returns>
        public static bool IsNpc(NetworkIdentity arg)
            => Get(arg) != null;

        /// <summary>
        /// Gets a value indicating whether or not a given <see cref="Footprint"/> is an NPC.
        /// </summary>
        /// <param name="arg">The <see cref="Footprint"/> instance to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="arg"/> is an NPC, otherwise <see langword="false"/>.</returns>
        public static bool IsNpc(Footprint arg)
            => Get(arg) != null;

        /// <summary>
        /// Gets a value indicating whether or not a given player ID is an NPC.
        /// </summary>
        /// <param name="arg">The player ID to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="playerId"/> is an NPC, otherwise <see langword="false"/>.</returns>
        public static bool IsNpc(int playerId)
            => Get(playerId) != null;

        /// <summary>
        /// Gets a value indicating whether or not a given network ID is an NPC.
        /// </summary>
        /// <param name="arg">The network ID to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="networkId"/> is an NPC, otherwise <see langword="false"/>.</returns>
        public static bool IsNpc(uint networkId)
            => Get(networkId) != null;

        /// <summary>
        /// Gets a value indicating whether or not a given item belongs to an NPC.
        /// </summary>
        /// <param name="arg">The <see cref="ItemBase"/> instance to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="arg"/> belongs to an NPC, otherwise <see langword="false"/>.</returns>
        public static bool IsNpc(ItemBase arg)
            => Get(arg) != null;

        /// <summary>
        /// Gets a value indicating whether or not a given item was dropped by an NPC.
        /// </summary>
        /// <param name="arg">The <see cref="ItemPickupBase"/> instance to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="arg"/> was dropped by an NPC, otherwise <see langword="false"/>.</returns>
        public static bool IsNpc(ItemPickupBase arg)
            => Get(arg) != null;

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance tied to the specified <see cref="ReferenceHub"/>.
        /// </summary>
        /// <param name="hub">The <see cref="ReferenceHub"/> instance to get an NPC of.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(ReferenceHub hub)
            => _spawnedNpcs.FirstOrDefault(npc => npc._hub != null && npc._hub == hub);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance tied to the specified <see cref="ExPlayer"/>.
        /// </summary>
        /// <param name="player">The <see cref="ExPlayer"/> instance to get an NPC of.</param>
        /// <returns>The <see cref="ExPlayer"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(ExPlayer player)
            => Get(player.Hub);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance tied to the specified <see cref="PluginAPI.Core.Player"/>.
        /// </summary>
        /// <param name="player">The <see cref="PluginAPI.Core.Player"/> instance to get an NPC of.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(Player player)
            => Get(player.ReferenceHub);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance tied to the specified <see cref="GameObject"/>.
        /// </summary>
        /// <param name="player">The <see cref="GameObject"/> instance to get an NPC of.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(GameObject gameObject)
            => _spawnedNpcs.FirstOrDefault(n => n.Player.GameObject != null && n.Player.GameObject == gameObject);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance tied to the specified <see cref="Collider"/>.
        /// </summary>
        /// <param name="player">The <see cref="Collider"/> instance to get an NPC of.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(Collider collider)
            => Get(collider.transform.root.gameObject);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance tied to the specified <see cref="NetworkConnection"/>.
        /// </summary>
        /// <param name="player">The <see cref="NetworkConnection"/> instance to get an NPC of.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(NetworkConnection connection)
            => _spawnedNpcs.FirstOrDefault(n => n.Connection != null && n.Connection == connection);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance tied to the specified <see cref="NetworkIdentity"/>.
        /// </summary>
        /// <param name="player">The <see cref="NetworkIdentity"/> instance to get an NPC of.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(NetworkIdentity identity)
            => Get(identity.netId);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance tied to the specified <see cref="ItemBase"/>.
        /// </summary>
        /// <param name="player">The <see cref="ItemBase"/> instance to get an NPC of.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(ItemBase item)
            => Get(item.Owner);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance tied to the specified <see cref="ItemPickupBase"/>.
        /// </summary>
        /// <param name="player">The <see cref="ItemPickupBase"/> instance to get an NPC of.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(ItemPickupBase itemPickup)
            => Get(itemPickup.PreviousOwner);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance tied to the specified <see cref="GameObject"/>.
        /// </summary>
        /// <param name="player">The <see cref="GameObject"/> instance to get an NPC of.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(Footprint footprint)
            => Get(footprint.Hub);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance by a player ID.
        /// </summary>
        /// <param name="playerId">The player ID to find.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(int playerId)
            => _spawnedNpcs.FirstOrDefault(n => n.Id == playerId);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance by a network ID.
        /// </summary>
        /// <param name="networkId">The network ID to find.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler Get(uint networkId)
            => _spawnedNpcs.FirstOrDefault(n => n.Player.NetId == networkId);

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by a <see cref="ReferenceHub"/>.
        /// </summary>
        /// <param name="player">The instance to find.</param>
        /// <param name="handler">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="NpcHandler"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(ReferenceHub player, out NpcHandler handler)
            => (handler = Get(player)) != null;

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by a <see cref="ExPlayer"/>.
        /// </summary>
        /// <param name="player">The instance to find.</param>
        /// <param name="handler">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="NpcHandler"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(ExPlayer player, out NpcHandler handler)
            => (handler = Get(player)) != null;

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by a <see cref="PluginAPI.Core.Player"/>.
        /// </summary>
        /// <param name="player">The instance to find.</param>
        /// <param name="handler">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="NpcHandler"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(Player player, out NpcHandler handler)
            => (handler = Get(player)) != null;

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by a <see cref="GameObject"/>.
        /// </summary>
        /// <param name="player">The instance to find.</param>
        /// <param name="handler">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="NpcHandler"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(GameObject player, out NpcHandler handler)
            => (handler = Get(player)) != null;

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by a <see cref="NetworkIdentity"/>.
        /// </summary>
        /// <param name="identity">The instance to find.</param>
        /// <param name="handler">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="NpcHandler"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(NetworkIdentity identity, out NpcHandler handler)
            => (handler = Get(identity)) != null;

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by a <see cref="NetworkConnection"/>.
        /// </summary>
        /// <param name="conn">The instance to find.</param>
        /// <param name="handler">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="NpcHandler"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(NetworkConnection conn, out NpcHandler handler)
            => (handler = Get(conn)) != null;

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by a <see cref="Collider"/>.
        /// </summary>
        /// <param name="collider">The instance to find.</param>
        /// <param name="handler">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="NpcHandler"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(Collider collider, out NpcHandler handler)
            => (handler = Get(collider)) != null;

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by a player ID.
        /// </summary>
        /// <param name="playerId">The player ID to find.</param>
        /// <param name="handler">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="NpcHandler"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(int playerId, out NpcHandler handler)
            => (handler = Get(playerId)) != null;

        /// <summary>
        /// Tries to get an <see cref="NpcHandler"/> instance by a network ID.
        /// </summary>
        /// <param name="networkId">The network ID to find.</param>
        /// <param name="handler">The instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="NpcHandler"/> instance was found, otherwise <see langword="null"/>.</returns>
        public static bool TryGet(uint networkId, out NpcHandler handler)
            => (handler = Get(networkId)) != null;

        /// <summary>
        /// Gets a list of all NPC's that match the predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A list of all matching NPCs.</returns>
        public static IEnumerable<NpcHandler> Get(Func<NpcHandler, bool> predicate)
            => _spawnedNpcs.Where(predicate);

        /// <summary>
        /// Gets a list of all NPC's in the specified <paramref name="team"/>.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <returns>A list of all matching NPCs.</returns>
        public static IEnumerable<NpcHandler> Get(Team team)
            => Get(n => n.Player.Role.Team == team);

        /// <summary>
        /// Gets a list of all NPC's in the specified <paramref name="faction"/>.
        /// </summary>
        /// <param name="faction">The faction.</param>
        /// <returns>A list of all matching NPCs.</returns>
        public static IEnumerable<NpcHandler> Get(Faction faction)
            => Get(n => n.Player.Role.Faction == faction);

        /// <summary>
        /// Gets a list of all NPC's with the specified <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns>A list of all matching NPCs.</returns>
        public static IEnumerable<NpcHandler> Get(RoleTypeId role)
            => Get(n => n.Player.Role.Type == role);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance by it's ID.
        /// </summary>
        /// <param name="npcId">The ID of the NPC.</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static NpcHandler GetById(int npcId)
            => _spawnedNpcs.FirstOrDefault(npc => npc.Id == npcId);

        /// <summary>
        /// Gets an <see cref="NpcHandler"/> instance by it's ID.
        /// </summary>
        /// <param name="npcId">The ID of the NPC.</param>
        /// <param name="npcHandler">The found <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="NpcHandler"/> instance was found, otherwise <see langword="false"/>.</returns>
        public static bool TryGetById(int npcId, out NpcHandler npcHandler)
            => _spawnedNpcs.TryGetFirst(npc => npc.Id == npcId, out npcHandler);
        #endregion

        private ExPlayer _player;
        private ReferenceHub _hub;
        private NpcConnection _connection;
        private NavigationModule _navModule;

        // Used to avoid exceptions while removing all NPCs when the round ends.
        internal bool _removeFromList = true;
        internal bool _isDestroying;

        internal NpcHandler(ReferenceHub hub, NpcConnection connection, ExPlayer player) : base()
        {
            _hub = hub;
            _player = player;
            _connection = connection;

            _player.NpcHandler = this;

            Id = connection.connectionId;
            PreviousRole = RoleTypeId.None;
        }

        /// <summary>
        /// Gets the NPC's ID.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the NPC's role before being despawned by <see cref="Despawn"/>.
        /// </summary>
        public RoleTypeId PreviousRole { get; private set; }

        /// <summary>
        /// Gets the <see cref="ExPlayer"/> instance tied to this NPC.
        /// </summary>
        public ExPlayer Player => _player;

        /// <summary>
        /// Gets the <see cref="ReferenceHub"/> instance tied to this NPC.
        /// </summary>
        public ReferenceHub Hub => _hub;

        /// <summary>
        /// Gets the NPC's fake connection instance.
        /// </summary>
        public NpcConnection Connection => _connection;

        /// <summary>
        /// Gets the NPC's navigation module.
        /// </summary>
        public NavigationModule Navigation => _navModule;

        /// <summary>
        /// Gets a value indicating whether or not the NPC is spawned.
        /// </summary>
        public bool IsSpawned => _hub.roleManager.CurrentRole.RoleTypeId != RoleTypeId.None;

        /// <summary>
        /// Enables/disables the navigation module.
        /// </summary>
        public bool IsNavigationActive
        {
            get => _navModule.IsActive && !_navModule.NavAgent.isStopped;
            set
            {
                _navModule.IsActive = value;
                _navModule.NavAgent.isStopped = !value;
            }
        }

        /// <summary>
        /// Enables/disables NPC interactions.
        /// </summary>
        public bool IsNavigationInteractable
        {
            get => _navModule.AllowInteractions;
            set => _navModule.AllowInteractions = value;
        }

        /// <summary>
        /// Gets or sets the NPC's current positional navigation target.
        /// </summary>
        public Vector3? NavigationDestination
        {
            get => _navModule.TargetPosition;
            set => _navModule.TargetPosition = value;
        }

        /// <summary>
        /// Gets or sets the NPC's current player target.
        /// </summary>
        public ExPlayer NavigationTarget
        {
            get => _navModule.PlayerTarget;
            set => _navModule.PlayerTarget = value;
        }

        /// <summary>
        /// Despawns the NPC.
        /// </summary>
        public void Despawn()
        {
            if (IsSpawned)
            {
                PreviousRole = Player.Role.Type;
                Player.Role.Type = RoleTypeId.None;
            }
        }

        /// <summary>
        /// Spawns the NPC.
        /// </summary>
        /// <param name="role">The role to spawn the NPC as.</param>
        /// <param name="position">The position to spawn the NPC at.</param>
        /// <param name="callback">The function to call once the NPC spawns.</param>
        public void Spawn(RoleTypeId role, Vector3? position = null, Action callback = null)
        {
            if (IsSpawned)
                return;

            if (role is RoleTypeId.None)
                return;

            if (Player.Role.Type == role)
            {
                if (position.HasValue)
                {
                    Player.Position.Position = position.Value;

                    callback();
                }

                return;
            }

            Player.Role.Type = role;

            Timing.CallDelayed(0.15f, () =>
            {
                if (position.HasValue)
                {
                    Player.Position.Position = position.Value;

                    callback();
                }
                else
                {
                    callback();
                }
            });
        }

        /// <summary>
        /// Adds the <see cref="NavigationModule"/> to this NPC.
        /// </summary>
        public void UseNavigation()
        {
            _navModule = Player.AddModule<NavigationModule>();

            if (!_navModule.IsInitialized)
                _navModule.Initialize(this);
        }

        /// <summary>
        /// Resets the NPC's navigation module.
        /// </summary>
        public void ResetNavigation()
        {
            var wasActive = IsNavigationActive;
            var wasInteractive = IsNavigationInteractable;

            IsNavigationActive = false;
            IsNavigationInteractable = false;

            NavigationDestination = null;
            NavigationTarget = null;

            IsNavigationActive = wasActive;
            IsNavigationInteractable = wasInteractive;
        }

        /// <summary>
        /// Destroys this NPC instance.
        /// </summary>
        /// <param name="createRagdoll">Whether or not to spawn a ragdoll of the NPC's current role.</param>
        public virtual void Destroy(bool createRagdoll = false)
        {
            _isDestroying = true;

            if (IsSpawned && !createRagdoll)
                Despawn();

            if (_hub._playerId.Value <= RecyclablePlayerId._autoIncrement)
                _hub._playerId.Destroy();

            _hub.OnDestroy();

            if (_removeFromList)
                _spawnedNpcs.Remove(this);

            CustomNetworkManager.TypedSingleton.OnServerDisconnect(_connection);
            UnityEngine.Object.Destroy(_hub.gameObject);

            _hub = null;
            _player = null;
            _connection = null;
        }

        /// <summary>
        /// A method that fixes NPC rotation on the server, may be overriden by other classes.
        /// </summary>
        public virtual void OnRotationUpdated()
        {
            if (Player?.Role.FpcRole is null)
                return;

            Player.Role.MouseLook.CurrentHorizontal = Player.Transform.eulerAngles.y;
        }

        /// <summary>
        /// Spawns a new NPC. 
        /// There is about half a second of a required delay, which is why the method doesn't directly return the <see cref="NpcHandler"/> instance but instead uses
        /// a callback parameter.
        /// </summary>
        /// <param name="name">The name of the NPC. If not set (empty or <see langword="null"/>) defaults to NPC (ID: <see cref="Id"/>)</param>
        /// <param name="role">The role of the NPC to set. Defaults to <see cref="RoleTypeId.None"/>.</param>
        /// <param name="customId">The custom player ID of the NPC to set. If left as <see langword="null"/> defaults to the next available player ID.</param>
        /// <param name="customUserId">The user ID of the NPC to set. If not set (empty or <see langword="null"/>) defaults to npc@localhost.</param>
        /// <param name="position">The position to spawn the NPC at. Defaults to the role's default spawn position if left as <see langword="null"/>.</param>
        /// <param name="callback">The method to invoke once the NPC finishes spawning.</param>
        public static void Spawn(string? name = null, RoleTypeId role = RoleTypeId.None, int? customId = null, string? customUserId = null, Vector3? position = null, Action<NpcHandler> callback = null)
        {
            var id = customId.HasValue ? customId.Value : _npcIdGen.Next();
            var userId = !string.IsNullOrWhiteSpace(customUserId) ? customUserId : $"npc_{id}@localhost";

            var hubObject = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            var hubComponent = hubObject.GetComponent<ReferenceHub>();

            try
            {
                hubComponent.roleManager.InitializeNewRole(RoleTypeId.None, RoleChangeReason.None);
            }
            catch { }

            if (RecyclablePlayerId.FreeIds.Contains(id))
                RecyclablePlayerId.FreeIds.Remove(id);
            else if (RecyclablePlayerId._autoIncrement >= id)
                RecyclablePlayerId._autoIncrement = id = RecyclablePlayerId._autoIncrement + 1;

            var connection = new NpcConnection(id);

            NetworkServer.AddPlayerForConnection(connection, hubObject);

            try
            {
                hubComponent.authManager.UserId = userId;
            }
            catch { }

            if (string.IsNullOrWhiteSpace(name))
                name = $"NPC (ID: {id})";

            hubComponent.nicknameSync._firstNickname = name;
            hubComponent.nicknameSync.Network_myNickSync = name;
            hubComponent.nicknameSync.NickSet = true;

            hubComponent.authManager.NetworkSyncedUserId = "ID_Dedicated";
            hubComponent.syncMode = (SyncMode)ClientInstanceMode.DedicatedServer;

            var player = new ExPlayer(hubComponent, NpcSwitches);
            var npc = new NpcHandler(hubComponent, connection, player);

            if (TransientModule._cachedModules.TryGetValue(player.UserId, out var transientModules))
            {
                foreach (var module in transientModules)
                {
                    var type = module.GetType();

                    if (!player._modules.ContainsKey(type))
                    {
                        player._modules[type] = module;

                        module.Parent = player;
                        module.StartModule();

                        player.OnModuleAdded(module);
                    }
                    else
                    {
                        ApiLoader.Warn("Extended API", $"Could not add transient module &3{type.Name}&r to NPC &3{player.Name}&r (&6{player.UserId}&r) - active instance found.");
                    }
                }
            }

            ExPlayer._allPlayers.Add(player);

            _spawnedNpcs.Add(npc);

            Timing.CallDelayed(0.3f, () =>
            {
                if (role != RoleTypeId.None)
                {
                    hubComponent.roleManager.ServerSetRole(role, RoleChangeReason.RoundStart, position.HasValue ? RoleSpawnFlags.All : RoleSpawnFlags.AssignInventory);

                    if (position.HasValue)
                    {
                        Timing.CallDelayed(0.2f, () =>
                        {
                            hubComponent.TryOverridePosition(position.Value, Vector3.zero);
                            callback(npc);
                        });
                    }
                    else
                    {
                        callback(npc);
                    }
                }
                else
                {
                    callback(npc);
                }
            });
        }

        /// <summary>
        /// Converts a <see cref="ExPlayer"/> to an <see cref="NpcHandler"/>.
        /// </summary>
        /// <param name="exPlayer">The Player to convert</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static implicit operator NpcHandler(ExPlayer exPlayer)
            => Get(exPlayer);

        /// <summary>
        /// Converts a <see cref="PluginAPI.Core.Player"/> to an <see cref="NpcHandler"/>.
        /// </summary>
        /// <param name="player">The instance to convert</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static implicit operator NpcHandler(Player player)
            => Get(player);

        /// <summary>
        /// Converts a <see cref="ReferenceHub"/> to an <see cref="NpcHandler"/>.
        /// </summary>
        /// <param name="hub">The <see cref="ReferenceHub"/> to convert</param>
        /// <returns>The <see cref="NpcHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        public static implicit operator NpcHandler(ReferenceHub hub)
            => Get(hub);

        // Destroys all NPCs on round end.
        internal static void DestroyNpcs()
        {
            foreach (var npc in _spawnedNpcs)
            {
                npc._removeFromList = false;
                npc.Destroy();
            }

            _spawnedNpcs.Clear();
        }
    }
}