using CentralAuth;

using LabExtended.API.Modules;
using LabExtended.Modules;

using LiteNetLib;

using Mirror.LiteNetLib4Mirror;

using PlayerRoles;

using UnityEngine;

namespace LabExtended.API
{
    public class ExPlayer : ModuleParent
    {
        private static readonly List<ExPlayer> _players = new List<ExPlayer>();

        public static IReadOnlyList<ExPlayer> Players => _players;
        public static int Count => _players.Count;

        private NetPeer _peer;
        private ReferenceHub _hub;
        private PlayerStorageModule _storage;

        public ExPlayer(GameObject component)
            : this(ReferenceHub.GetHub(component))
        { }

        public ExPlayer(ReferenceHub component)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            _hub = component;
            _peer = LiteNetLib4MirrorServer.Peers[_hub.connectionToClient.connectionId];

            var modules = TransientModule.Get(this);

            if (modules != null && modules.Count > 0)
            {
                foreach (var module in modules)
                {
                    module.Create(this);

                    if (module is PlayerStorageModule storageModule && _storage is null)
                        _storage = storageModule;
                }

                modules.Clear();
            }

            if (!HasModule<PlayerStorageModule>() || _storage is null)
                _storage = AddModule<PlayerStorageModule>();
        }

        public NetPeer Peer => _peer;
        public ReferenceHub Hub => _hub;
        public GameObject GameObject => _hub.gameObject;

        public PlayerRoleBase RoleBase => _hub.roleManager.CurrentRole;
        public PlayerStorageModule Storage => _storage;

        public ConnectionState ConnectionState => _peer.ConnectionState;

        public uint NetId => _hub.netId;

        public int ConnectionId => _hub.connectionToClient.connectionId;
        public int Ping => _peer.Ping;
        public int TripTime => _peer._rtt;

        public bool IsOnline => ConnectionState is ConnectionState.Connected;
        public bool IsOffline => ConnectionState != ConnectionState.Connected;

        public bool IsServer => InstanceMode is ClientInstanceMode.DedicatedServer || InstanceMode is ClientInstanceMode.Host;
        public bool IsVerified => InstanceMode is ClientInstanceMode.ReadyClient;
        public bool IsUnverified => InstanceMode is ClientInstanceMode.Unverified;

        public string Address => _hub.connectionToClient.address;

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

        public RoleTypeId Role
        {
            get => _hub.roleManager.CurrentRole.RoleTypeId;
            set => _hub.roleManager.ServerSetRole(value, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.All);
        }

        public ClientInstanceMode InstanceMode
        {
            get => _hub.authManager.InstanceMode;
            set => _hub.authManager.InstanceMode = value;
        }
    }
}