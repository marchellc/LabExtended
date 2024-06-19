using CentralAuth;

using Common.Extensions;
using Common.IO.Collections;
using Common.Pooling.Pools;

using InventorySystem;
using InventorySystem.Disarming;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Pickups;

using LabExtended.API.Npcs;
using LabExtended.API.Modules;
using LabExtended.API.RemoteAdmin;

using LabExtended.API.Voice;
using LabExtended.API.Voice.Profiles;
using LabExtended.API.Voice.Processing;

using LabExtended.Modules;
using LabExtended.Utilities;
using LabExtended.Extensions;
using LabExtended.Patches.Functions;

using LiteNetLib;

using Mirror;
using Mirror.LiteNetLib4Mirror;

using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps;
using PlayerRoles.Spectating;
using PlayerStatsSystem;

using Footprinting;

using RelativePositioning;

using UnityEngine;

using Decals;

using VoiceChat;

using CustomPlayerEffects;

using Utils;

namespace LabExtended.API
{
    public class ExPlayer : ModuleParent
    {
        internal static readonly List<ExPlayer> _players;

        internal static ExPlayer _localPlayer;
        internal static ExPlayer _hostPlayer;

        /// <summary>
        /// Gets a list of all players on the server (including NPCs).
        /// </summary>
        public static IEnumerable<ExPlayer> Players => _players;

        /// <summary>
        /// Gets a list of all players on the server (exluding NPCs).
        /// </summary>
        public static IEnumerable<ExPlayer> RealPlayers => _players;

        /// <summary>
        /// Gets a count of all players on the server (including NPCs).
        /// </summary>
        public static int Count => _players.Count;

        /// <summary>
        /// Gets a count of all players on the server (exluding NPCs).
        /// </summary>
        public static int RealCount => _players.Count(p => !p.IsNpc);

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
                    _players.Add(_hostPlayer);
                }

                return _hostPlayer;
            }
        }

        /// <summary>
        /// Gets the local player.
        /// </summary>
        public static ExPlayer Local
        {
            get
            {
                if (_localPlayer is null)
                {
                    if (!ReferenceHub.TryGetLocalHub(out var localHub))
                        throw new Exception($"Failed to fetch the local ReferenceHub.");

                    _localPlayer = new ExPlayer(localHub);
                    _players.Add(_localPlayer);
                }

                return _localPlayer;
            }
        }

        static ExPlayer()
        {
            _players = new List<ExPlayer>();

            _hostPlayer = null;
            _localPlayer = null;

            UpdateEvent.OnUpdate += UpdateSentRoles;
        }

        public static ExPlayer Get(GameObject gameObject)
        {
            if (gameObject is null)
                return null;

            if (!ReferenceHub.TryGetHub(gameObject, out var hub) && !gameObject.TryGetComponent<ReferenceHub>(out hub))
                return null;

            if (ReferenceHub.HostHub != null && hub == ReferenceHub.HostHub)
                return Host;

            if (ReferenceHub.LocalHub != null && hub == ReferenceHub.LocalHub)
                return Local;

            return _players.FirstOrDefault(p => p.Hub != null && p.Hub == hub);
        }

        public static ExPlayer Get(ReferenceHub hub)
        {
            if (hub is null)
                return null;

            if (ReferenceHub.HostHub != null && hub == ReferenceHub.HostHub)
                return Host;

            if (ReferenceHub.LocalHub != null && hub == ReferenceHub.LocalHub)
                return Local;

            return _players.FirstOrDefault(p => p.Hub != null && p.Hub == hub);
        }

        public static ExPlayer Get(NetworkConnection connection)
        {
            if (connection is null)
                return null;

            return _players.FirstOrDefault(p => p.Connection != null && p.Connection == connection);
        }

        public static ExPlayer Get(NetPeer peer)
            => _players.FirstOrDefault(p => p.Peer != null && p.Peer == peer);

        public static ExPlayer Get(uint networkId)
            => _players.FirstOrDefault(p => p.NetId == networkId);

        public static ExPlayer Get(int playerId)
            => _players.FirstOrDefault(p => p.PlayerId == playerId);

        public static ExPlayer GetByConnectionId(int connectionId)
            => _players.FirstOrDefault(p => p.ConnectionId == connectionId);

        public static ExPlayer GetByUserId(string userId)
            => _players.FirstOrDefault(p => p.UserId == userId);

        public static ExPlayer Get(string nameOrId, double minNameScore = 0.85)
        {
            foreach (var player in _players)
            {
                if (player.UserId == nameOrId || player.PlayerId.ToString() == nameOrId)
                    return player;

                if (player.Name.GetSimilarity(nameOrId) >= minNameScore)
                    return player;
            }

            return null;
        }

        private NetPeer _peer;
        private ReferenceHub _hub;
        private PlayerStorageModule _storage;

        private RemoteAdminIconType _forcedIcons;
        private VoiceFlags _voiceFlags;

        #region Internal VC stuff
        internal bool _wasSpeaking;

        internal DateTime _wasSpeakingAt;

        internal List<byte[]> _speakingCapture;

        internal VoiceProfileBase _voiceProfile;
        internal VoicePitchHandler _voicePitch;
        #endregion

        internal LockedDictionary<uint, RoleTypeId> _sentRoles; // A custom way of sending roles to other players so it's easier to manage them.

        public ExPlayer(ReferenceHub component) : base()
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            _hub = component;
            _forcedIcons = RemoteAdminIconType.None;
            _sentRoles = new LockedDictionary<uint, RoleTypeId>();
            _speakingCapture = ListPool<byte[]>.Shared.Rent();
            _voicePitch = new VoicePitchHandler(this);

            ArrayExtensions.TryPeekIndex(LiteNetLib4MirrorServer.Peers, ConnectionId, out _peer); // In case the hub is an NPC.

            InfoArea = new EnumValue<PlayerInfoArea>(() => _hub.nicknameSync.Network_playerInfoToShow, value => _hub.nicknameSync.Network_playerInfoToShow = value);
            MuteFlags = new EnumValue<VcMuteFlags>(() => VoiceChatMutes.GetFlags(_hub), value => VoiceChatMutes.SetFlags(_hub, value));

            ForcedRaIcons = new EnumValue<RemoteAdminIconType>(() => _forcedIcons, value => _forcedIcons = value);
            VoiceFlags = new EnumValue<VoiceFlags>(() => _voiceFlags, value => _voiceFlags = value);

            FakePosition = new FakeValue<Vector3>();
            FakeRole = new FakeValue<RoleTypeId>();

            Role = new Utilities.PlayerRoles(component.roleManager);
            Stats = new Utilities.PlayerStats(component.playerStats);

            var modules = TransientModule.Get(this);

            if (modules != null && modules.Count > 0)
            {
                foreach (var module in modules)
                {
                    module.Player = this;
                    module.Create(this);

                    if (module is PlayerStorageModule storageModule && _storage is null)
                        _storage = storageModule;
                }
            }

            _storage ??= AddModule<PlayerStorageModule>();
        }

        /// <inheritdoc/>
        public override bool KeepTransientModules => true;

        /// <inheritdoc/>
        public override bool UpdateModules => true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player is visible in the Remote Admin player list.
        /// </summary>
        public bool IsVisibleInRemoteAdmin { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player is visible in the Spectator List.
        /// </summary>
        public bool IsVisibleInSpectatorList { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player should count in the next respawn wave.
        /// </summary>
        public bool CanRespawn { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player prevents the round from ending.
        /// </summary>
        public bool CanBlockRoundEnd { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can trigger SCP-096.
        /// </summary>
        public bool CanTriggerScp096 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can block SCP-173's movement.
        /// </summary>
        public bool CanBlockScp173 { get; set; } = true;

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
        public EnumValue<VoiceFlags> VoiceFlags { get; }

        public FakeValue<Vector3> FakePosition { get; }
        public FakeValue<RoleTypeId> FakeRole { get; }

        public NpcHandler NpcHandler { get; internal set; }

        public Utilities.PlayerRoles Role { get; }
        public Utilities.PlayerStats Stats { get; }

        public ReferenceHub Hub => _hub;
        public GameObject GameObject => _hub.gameObject;

        public NetPeer Peer => _peer;
        public NetworkIdentity Identity => _hub.netIdentity;
        public NetworkConnectionToClient Connection => _hub.connectionToClient;

        public PlayerStorageModule Storage => _storage;

        public ExPlayer SpectatedPlayer => _players.FirstOrDefault(p => p.IsSpectatedBy(this));
        public ExPlayer LookingAtPlayer => _players.FirstOrDefault(p => IsLookingAt(p));

        public ExPlayer ClosestPlayer => _players.Where(p => p.NetId != NetId).OrderBy(DistanceTo).FirstOrDefault();
        public ExPlayer ClosestScp => _players.Where(p => p.NetId != NetId && p.Role.IsScp).OrderBy(DistanceTo).FirstOrDefault();

        public ExPlayer Disarmer => Get(DisarmedPlayers.Entries.FirstOrDefault(d => d.DisarmedPlayer == NetId).Disarmer);

        public ConnectionState ConnectionState => _peer?.ConnectionState ?? ConnectionState.Disconnected;

        public IEnumerable<ExPlayer> SpectatingPlayers => _players.Where(IsSpectatedBy);
        public IEnumerable<ExPlayer> PlayersInLineOfSight => _players.Where(p => p.IsInLineOfSight(this));

        public IEnumerable<Firearm> Firearms => _hub.inventory.UserInventory.Items.Where<Firearm>();

        public IEnumerable<StatusEffectBase> InactiveEffects => _hub.playerEffectsController.AllEffects.Where(e => !e.IsEnabled);
        public IEnumerable<StatusEffectBase> ActiveEffects => _hub.playerEffectsController.AllEffects.Where(e => e.IsEnabled);
        public IEnumerable<StatusEffectBase> AllEffects => _hub.playerEffectsController.AllEffects;

        public Transform Transform => _hub.transform;
        public Transform Camera => _hub.PlayerCameraReference;

        public Vector3 Velocity => _hub.GetVelocity();

        public Footprint Footprint => new Footprint(Hub);

        public uint NetId => _hub.netId;

        public int Ping => _peer?.Ping ?? -1;
        public int TripTime => _peer?._avgRtt ?? -1;
        public int ConnectionId => _hub.connectionToClient.connectionId;

        public bool IsOnline => _peer != null ? ConnectionState is ConnectionState.Connected : GameObject != null;
        public bool IsOffline => _peer != null ? ConnectionState != ConnectionState.Connected : GameObject is null;

        public bool IsServer => InstanceMode is ClientInstanceMode.DedicatedServer || InstanceMode is ClientInstanceMode.Host;
        public bool IsVerified => InstanceMode is ClientInstanceMode.ReadyClient;
        public bool IsUnverified => InstanceMode is ClientInstanceMode.Unverified;

        public bool IsNpc => NpcHandler != null;

        public bool IsDisarmed => _hub.inventory.IsDisarmed();

        public bool IsSpeaking => Role.VoiceModule?.ServerIsSending ?? false;

        public bool HasCustomName => _hub.nicknameSync.HasCustomName;
        public bool HasAnyItems => _hub.inventory.UserInventory.Items.Count > 0;
        public bool HasAnyAmmo => _hub.inventory.UserInventory.ReserveAmmo.Any(p => p.Value > 0);
        public bool HasRemoteAdminAccess => _hub.serverRoles.RemoteAdmin;
        public bool HasAdminChatAccess => _hub.serverRoles.AdminChatPerms;

        public bool DoNotTrack => _hub.authManager.DoNotTrack;

        public string Address => _hub.connectionToClient.address;

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

                var instance = value.GetInstance<ItemBase>();

                instance.SetupItem(Hub);
                instance.OnAdded(null);

                CurrentItemIdentifier = new ItemIdentifier(instance.ItemTypeId, instance.ItemSerial);
            }
        }

        public ItemIdentifier CurrentItemIdentifier
        {
            get => _hub.inventory.NetworkCurItem;
            set => _hub.inventory.NetworkCurItem = value;
        }

        public VoiceProfileBase VoiceProfile
        {
            get => _voiceProfile;
            set => VoiceSystem.SetProfile(this, value);
        }

        public Dictionary<ItemType, ushort> Ammo
        {
            get => _hub.inventory.UserInventory.ReserveAmmo;
            set
            {
                _hub.inventory.UserInventory.ReserveAmmo = value;
                _hub.inventory.ServerSendAmmo();
            }
        }

        public bool IsSpectatedBy(ExPlayer player)
            => Hub.IsSpectatedBy(player.Hub);

        public bool IsSpectating(ExPlayer player)
            => player.Hub.IsSpectatedBy(Hub);

        public bool Damage(float damageAmount, string reason = null)
            => Hub.playerStats.DealDamage(!string.IsNullOrWhiteSpace(reason) ? new CustomReasonDamageHandler(reason, damageAmount) : new UniversalDamageHandler(damageAmount, DeathTranslations.Bleeding));

        public void Kill(string reason = null)
            => Hub.playerStats.KillPlayer(!string.IsNullOrWhiteSpace(reason) ? new CustomReasonDamageHandler(reason, -1f) : new WarheadDamageHandler());

        public void Disintegrate(ExPlayer attacker = null)
            => Hub.playerStats.KillPlayer(new DisruptorDamageHandler(attacker?.Footprint ?? Footprint, -1f));

        public void Explode(ExPlayer attacker = null)
            => ExplosionUtils.ServerExplode(Position, attacker?.Footprint ?? Footprint);

        #region Messaging Methods
        public void Hint(object content, ushort duration)
            => ProxyMethods.ShowHint(this, content.ToString(), duration);

        public void Broadcast(object content, ushort duration, bool clearPrevious = true)
            => ProxyMethods.ShowBroadcast(this, content.ToString(), duration, clearPrevious);

        public void ConsoleMessage(object content, string color = "red")
            => _hub.encryptedChannelManager.TrySendMessageToClient(content.ToString(), EncryptedChannelManager.EncryptedChannel.GameConsole);

        public void RemoteAdminMessage(object content)
        {
            if (!HasRemoteAdminAccess)
                return;

            _hub.encryptedChannelManager.TrySendMessageToClient(content.ToString(), EncryptedChannelManager.EncryptedChannel.RemoteAdmin);
        }
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
        public bool IsInLineOfSight(ExPlayer player, bool countSpectating = true)
        {
            if (countSpectating && player.IsSpectatedBy(this))
                return true;

            var vision = VisionInformation.GetVisionInformation(Hub, Camera, player.Camera.position, 0.12f, 60f);

            if (vision.IsInLineOfSight || vision.IsLooking)
                return true;

            return false;
        }

        public bool IsLookingAt(ExPlayer player, bool countSpectating = true)
        {
            if (countSpectating && player.IsSpectatedBy(this))
                return true;

            var vision = VisionInformation.GetVisionInformation(Hub, Camera, player.Camera.position, 0.12f, 60f);

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
        public bool HasItem(ItemType type)
            => Items.Any(it => it.ItemTypeId == type);

        public int CountItems(ItemType type)
            => Items.Count(it => it.ItemTypeId == type);

        public void ClearInventory(IEnumerable<ItemType> newInventory = null)
        {
            _hub.inventory.UserInventory.Items.Clear();

            if (newInventory != null)
            {
                foreach (var item in newInventory)
                {
                    if (item != ItemType.None)
                    {
                        var instance = item.GetInstance<ItemBase>();

                        instance.Owner = _hub;
                        instance.OnAdded(null);
                        instance.SetupItem(Hub);

                        _hub.inventory.UserInventory.Items[instance.ItemSerial] = instance;
                    }
                }
            }

            _hub.inventory.ServerSendItems();
        }

        public void DropInventory()
            => _hub.inventory.ServerDropEverything();

        public ItemBase AddItem(ItemType type)
        {
            var item = _hub.inventory.ServerAddItem(type);

            if (item is null)
                return null;

            item.SetupItem(Hub);
            return item;
        }

        public bool AddOrSpawnItem(ItemType type)
        {
            if (_hub.inventory.UserInventory.Items.Count >= 8)
            {
                if (InventoryItemLoader.TryGetItem<ItemBase>(type, out var itemPrefab))
                {
                    _hub.inventory.ServerCreatePickup(itemPrefab, new PickupSyncInfo(type, itemPrefab.Weight));
                    return false;
                }

                return false;
            }

            var item = _hub.inventory.ServerAddItem(type);

            if (item is null)
                return false;

            item.SetupItem(Hub);
            return item;
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
        public void PlayGunSound(ItemType gunType, byte volume = 100)
            => Connection.Send(new GunAudioMessage(Hub, (byte)gunType, volume, Hub));

        public void SpawnGunDecal()
            => Connection.Send(new GunDecalMessage(Position, Camera.forward, DecalPoolType.Bullet));

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
                NetworkUtils._writerExtensions[value.GetType()]?.Call(null, targetWriter, value);
            }
        }

        public void SendFakeTargetRpc(NetworkIdentity behaviorOwner, Type targetType, string rpcName, params object[] values)
        {
            if (!IsOnline)
                return;

            var writer = NetworkWriterPool.Get();

            foreach (var value in values)
                NetworkUtils._writerExtensions[value.GetType()].Call(null, writer, value);

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
            _hub.inventory.UserInventory.Items.Clear();

            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item != null)
                    {
                        if (item.Owner != null)
                            item.OnRemoved(null);

                        item.Owner = Hub;
                        item.OnAdded(null);
                        item.SetupItem(Hub);

                        _hub.inventory.UserInventory.Items[item.ItemSerial] = item;
                    }
                }
            }

            _hub.inventory.ServerSendItems();
        }

        private void SetScale(Vector3 scale)
        {
            if (scale == _hub.transform.localScale)
                return;

            _hub.transform.localScale = scale;

            foreach (var target in _players)
                NetworkServer.SendSpawnMessage(_hub.netIdentity, target.Connection);
        }
        #endregion

        #region Internal Methods
        internal RoleTypeId? GetRoleForJoinedPlayer(ExPlayer joinedPlayer)
        {
            if (joinedPlayer.NetId == NetId)
                return null;

            if (!joinedPlayer.Role.IsAlive && !IsVisibleInSpectatorList)
                return RoleTypeId.Spectator;

            return null;
        }

        private static void UpdateSentRoles()
        {
            foreach (var player in _players)
            {
                var curRoleId = player.Role.Type;

                if (player.Role.Role is IObfuscatedRole obfuscatedRole)
                    curRoleId = obfuscatedRole.GetRoleForUser(player.Hub);

                foreach (var other in _players)
                {
                    if (player.FakeRole.TryGetValue(other, out var fakedRole))
                        curRoleId = fakedRole;

                    if (!other.Role.IsAlive && !player.IsVisibleInSpectatorList)
                        curRoleId = RoleTypeId.Spectator;

                    if (player._sentRoles.TryGetValue(other.NetId, out var sentRole) && sentRole == curRoleId)
                        continue;

                    player._sentRoles[other.NetId] = curRoleId;
                    other.Connection.Send(new RoleSyncInfo(player.Hub, curRoleId, other.Hub));
                }
            }
        }
        #endregion
    }
}