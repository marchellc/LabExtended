using AdminToys;

using Hazards;

using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;

using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;

using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Enums;
using LabExtended.API.Prefabs;
using LabExtended.API.Toys;

using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Networking;

using LabExtended.Events;
using LabExtended.Extensions;

using LightContainmentZoneDecontamination;

using MapGeneration;
using MapGeneration.Distributors;

using Mirror;

using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.Ragdolls;

using PlayerStatsSystem;

using RelativePositioning;

using UnityEngine;

using Utils;
using Utils.Networking;

namespace LabExtended.API
{
    public static class ExMap
    {
        internal static readonly LockedHashSet<ItemPickupBase> _pickups = new LockedHashSet<ItemPickupBase>();
        internal static readonly LockedHashSet<BasicRagdoll> _ragdolls = new LockedHashSet<BasicRagdoll>();
        internal static readonly LockedHashSet<Generator> _generators = new LockedHashSet<Generator>();
        internal static readonly LockedHashSet<ExTeslaGate> _gates = new LockedHashSet<ExTeslaGate>();
        internal static readonly LockedHashSet<Elevator> _elevators = new LockedHashSet<Elevator>();
        internal static readonly LockedHashSet<Airlock> _airlocks = new LockedHashSet<Airlock>();
        internal static readonly LockedHashSet<AdminToy> _toys = new LockedHashSet<AdminToy>();
        internal static readonly LockedHashSet<Locker> _lockers = new LockedHashSet<Locker>();
        internal static readonly LockedHashSet<Camera> _cams = new LockedHashSet<Camera>();
        internal static readonly LockedHashSet<Door> _doors = new LockedHashSet<Door>();

        public static IReadOnlyList<ItemPickupBase> Pickups => _pickups;
        public static IReadOnlyList<BasicRagdoll> Ragdolls => _ragdolls;
        public static IReadOnlyList<Generator> Generators => _generators;
        public static IReadOnlyList<ExTeslaGate> TeslaGates => _gates;
        public static IReadOnlyList<Elevator> Elevators => _elevators;
        public static IReadOnlyList<Airlock> Airlocks => _airlocks;
        public static IReadOnlyList<Locker> Lockers => _lockers;
        public static IReadOnlyList<Camera> Cameras => _cams;
        public static IReadOnlyList<AdminToy> Toys => _toys;
        public static IReadOnlyList<Door> Doors => _doors;

        public static IEnumerable<LockerChamber> LockerChambers => _lockers.SelectMany(x => x.Chambers);

        public static IEnumerable<Elevator> MovingElevators => GetElevators(x => x.IsMoving);
        public static IEnumerable<Elevator> ReadyElevators => GetElevators(x => x.Base.IsReady);

        public static IEnumerable<Door> OpenedDoors => GetDoors(x => x.IsOpened);
        public static IEnumerable<Door> ClosedDoors => GetDoors(x => !x.IsOpened);
        public static IEnumerable<Door> MovingDoors => GetDoors(x => x.IsMoving);

        public static IEnumerable<Generator> ActivatedGenerators => _generators.Where(x => x.IsEngaged);
        public static IEnumerable<Generator> ActivatingGenerators => _generators.Where(x => x.IsActivating);

        public static IEnumerable<Generator> OpenGenerators => _generators.Where(x => x.IsOpen);
        public static IEnumerable<Generator> ClosedGenerators => _generators.Where(x => !x.IsOpen);

        public static IEnumerable<Camera> ActiveCameras => GetCameras(x => x.IsUsed);
        public static IEnumerable<Camera> InactiveCameras => GetCameras(x => !x.IsUsed);

        public static IEnumerable<LightToy> LightToys => _toys.Where<LightToy>();
        public static IEnumerable<TargetToy> TargetToys => _toys.Where<TargetToy>();
        public static IEnumerable<PrimitiveToy> PrimitiveToys => _toys.Where<PrimitiveToy>();

        public static IEnumerable<Toys.SpeakerToy> SpeakerToys => _toys.Where<Toys.SpeakerToy>();

        public static IEnumerable<WaypointBase> Waypoints => WaypointBase.AllWaypoints.Where(x => x != null);
        public static IEnumerable<NetIdWaypoint> NetIdWaypoints => NetIdWaypoint.AllNetWaypoints.Where(x => x != null);

        public static int AmbientClipsCount => AmbientSoundPlayer.clips.Length;

        public static AmbientSoundPlayer AmbientSoundPlayer { get; private set; }

        public static Color DefaultLightColor { get; } = Color.clear;

        public static int Seed
        {
            get => SeedSynchronizer.Seed;
            set
            {
                if (SeedSynchronizer.MapGenerated)
                    return;

                SeedSynchronizer.Seed = value;
            }
        }

        public static void Broadcast(object msg, ushort duration, bool clearPrevious = true, Broadcast.BroadcastFlags flags = global::Broadcast.BroadcastFlags.Normal)
        {
            if (clearPrevious)
                global::Broadcast.Singleton?.RpcClearElements();

            global::Broadcast.Singleton?.RpcAddElement(msg.ToString(), duration, flags);
        }

        public static void ShowHint(object msg, ushort duration, bool isPriority = false)
            => ExPlayer.Players.ForEach(x => x.SendHint(msg, duration, isPriority));

        public static void StartDecontamination()
            => DecontaminationController.Singleton?.ForceDecontamination();

        public static void PlayAmbientSound()
            => AmbientSoundPlayer.GenerateRandom();

        public static void PlayAmbientSound(int id)
        {
            if (id < 0 || id >= AmbientSoundPlayer.clips.Length)
                throw new ArgumentOutOfRangeException(nameof(id));

            AmbientSoundPlayer.RpcPlaySound(AmbientSoundPlayer.clips[id].index);
        }

        public static TantrumEnvironmentalHazard PlaceTantrum(Vector3 position, bool setActive = true)
        {
            var instance = PrefabList.Tantrum.Spawn<TantrumEnvironmentalHazard>();

            if (!setActive)
                instance.SynchronizedPosition = new RelativePosition(position);
            else
                instance.SynchronizedPosition = new RelativePosition(position + (Vector3.up * 0.25f));

            instance._destroyed = !setActive;

            NetworkServer.Spawn(instance.gameObject);
            return instance;
        }

        public static void Explode(Vector3 position, ExplosionType type = ExplosionType.Grenade, ExPlayer attacker = null)
            => ExplosionUtils.ServerExplode(position, (attacker ?? ExPlayer.Host).Footprint, type);

        public static void SpawnExplosion(Vector3 position, ItemType type = ItemType.GrenadeHE)
            => ExplosionUtils.ServerSpawnEffect(position, type);

        public static DynamicRagdoll SpawnBonesRagdoll(Vector3 position, Vector3 scale, Quaternion rotation)
            => SpawnBonesRagdoll(position, scale, Vector3.zero, rotation);

        public static DynamicRagdoll SpawnBonesRagdoll(Vector3 position, Vector3 scale, Vector3 velocity, Quaternion rotation)
        {
            var ragdollInstance = SpawnRagdoll(RoleTypeId.Tutorial, position, scale, rotation, true, ExPlayer.Host, new UniversalDamageHandler(-1f, DeathTranslations.Warhead));

            if (ragdollInstance is null)
                throw new Exception($"Failed to spawn ragdoll.");

            var bonesRagdoll = SpawnRagdoll(RoleTypeId.Scp3114, position, scale, velocity, rotation, true, ExPlayer.Host, new Scp3114DamageHandler(ragdollInstance, false)) as DynamicRagdoll;

            _ragdolls.Remove(ragdollInstance);

            NetworkServer.Destroy(ragdollInstance.gameObject);

            if (bonesRagdoll is null)
                throw new Exception("Failed to spawn bones ragdoll.");

            Scp3114RagdollToBonesConverter.ServerConvertNew(ExPlayer.Host.Role.Scp3114, bonesRagdoll);

            ExPlayer.Host.Role.Set(RoleTypeId.None);
            return bonesRagdoll;
        }

        public static BasicRagdoll SpawnRagdoll(RoleTypeId ragdollRoleType, Vector3 position, Vector3 scale, Quaternion rotation, bool spawn = true, ExPlayer owner = null, DamageHandlerBase damageHandler = null)
            => SpawnRagdoll(ragdollRoleType, position, scale, Vector3.zero, rotation, spawn, owner, damageHandler);

        public static BasicRagdoll SpawnRagdoll(RoleTypeId ragdollRoleType, Vector3 position, Vector3 scale, Vector3 velocity, Quaternion rotation, bool spawn = true, ExPlayer owner = null, DamageHandlerBase damageHandler = null)
        {
            if (!ragdollRoleType.TryGetPrefab(out var role))
                throw new Exception($"Failed to find role prefab for role {ragdollRoleType}");

            if (role is not IRagdollRole ragdollRole)
                throw new Exception($"Role {ragdollRoleType} does not have a ragdoll.");

            damageHandler ??= new UniversalDamageHandler(-1f, DeathTranslations.Crushed);

            var ragdoll = UnityEngine.Object.Instantiate(ragdollRole.Ragdoll);

            ragdoll.NetworkInfo = new RagdollData((owner ?? ExPlayer.Host).Hub, damageHandler, ragdoll.transform.localPosition, ragdoll.transform.localRotation);

            ragdoll.transform.position = position;
            ragdoll.transform.rotation = rotation;

            ragdoll.transform.localScale = scale;

            if (ragdoll.TryGetComponent<Rigidbody>(out var rigidbody))
                rigidbody.velocity = velocity;

            if (spawn)
                NetworkServer.Spawn(ragdoll.gameObject);

            _ragdolls.Add(ragdoll);
            return ragdoll;
        }

        public static void FlickerLights(float duration, params FacilityZone[] zones)
        {
            foreach (var light in RoomLightController.Instances)
            {
                var room = RoomIdentifier.AllRoomIdentifiers.FirstOrDefault(r => r != null && r.LightControllers.Contains(light));

                if (room is null)
                    continue;

                if (zones.Length < 1 || zones.Contains(room.Zone))
                    light.ServerFlickerLights(duration);
            }
        }

        public static void SetLightColor(Color color)
            => RoomLightController.Instances.ForEach(x => x.NetworkOverrideColor = color);

        public static void ResetLightsColor()
            => RoomLightController.Instances.ForEach(x => x.NetworkOverrideColor = DefaultLightColor);

        public static ItemPickupBase SpawnItem(ItemType type, Vector3 position, Vector3 scale, Quaternion rotation, bool spawn = true)
            => SpawnItem<ItemPickupBase>(type, position, scale, rotation, spawn);

        public static T SpawnItem<T>(ItemType item, Vector3 position, Vector3 scale, Quaternion rotation, bool spawn = true) where T : ItemPickupBase
        {
            if (!item.TryGetItemPrefab(out var prefab))
                return null;

            var pickup = UnityEngine.Object.Instantiate((T)prefab.PickupDropModel, position, rotation);

            pickup.transform.position = position;
            pickup.transform.rotation = rotation;

            pickup.transform.localScale = scale;

            pickup.Info = new PickupSyncInfo(item, prefab.Weight, ItemSerialGenerator.GenerateNext());

            if (spawn)
            {
                NetworkServer.Spawn(pickup.gameObject);
                LabApi.Events.Handlers.ServerEvents.OnItemSpawned(new ItemSpawnedEventArgs(pickup));
            }

            return pickup;
        }

        public static List<T> SpawnItems<T>(ItemType type, int count, Vector3 position, Vector3 scale, Quaternion rotation, bool spawn = true) where T : ItemPickupBase
        {
            var list = new List<T>(count);

            for (int i = 0; i < count; i++)
            {
                var item = SpawnItem<T>(type, position, scale, rotation, spawn);

                if (item is null)
                    continue;

                list.Add(item);
            }

            return list;
        }

        public static ThrownProjectile SpawnProjectile(ItemType item, Vector3 position, Vector3 scale, Vector3 velocity, Quaternion rotation, float force, float fuseTime = 2f, bool spawn = true, bool activate = true)
            => SpawnProjectile<ThrownProjectile>(item, position, scale, velocity, rotation, force, fuseTime, spawn, activate);

        public static T SpawnProjectile<T>(ItemType item, Vector3 position, Vector3 scale, Vector3 velocity, Quaternion rotation, float force, float fuseTime = 2f, bool spawn = true, bool activate = true) where T : ThrownProjectile
            => SpawnProjectile<T>(item, position, scale, Vector3.forward, Vector3.up, rotation, velocity, force, fuseTime, spawn, activate);

        public static T SpawnProjectile<T>(ItemType item, Vector3 position, Vector3 scale, Vector3 forward, Vector3 up, Quaternion rotation, Vector3 velocity, float force, float fuseTime = 2f, bool spawn = true, bool activate = true, ushort? serial = null) where T : ThrownProjectile
        {
            if (!item.TryGetItemPrefab<ThrowableItem>(out var throwableItem))
                return null;

            var projectile = UnityEngine.Object.Instantiate((T)throwableItem.Projectile, position, rotation);
            var settings = throwableItem.FullThrowSettings;

            projectile.transform.localScale = scale;
            projectile.Info = new PickupSyncInfo(item, throwableItem.Weight, serial.HasValue ? serial.Value : ItemSerialGenerator.GenerateNext());

            settings.StartVelocity = force;
            settings.StartTorque = velocity;

            (projectile as TimeGrenade)!._fuseTime = fuseTime;

            if (spawn)
            {
                NetworkServer.Spawn(projectile.gameObject);
                LabApi.Events.Handlers.ServerEvents.OnItemSpawned(new ItemSpawnedEventArgs(projectile));
            }

            if (spawn && activate)
            {
                if (projectile.TryGetRigidbody(out var rigidbody))
                {
                    var num = 1f - Mathf.Abs(Vector3.Dot(forward, Vector3.up));
                    var vector = up * throwableItem.FullThrowSettings.UpwardsFactor;
                    var vector2 = forward + vector * num;

                    rigidbody.centerOfMass = Vector3.zero;
                    rigidbody.angularVelocity = settings.StartTorque;
                    rigidbody.velocity = velocity + vector2 * force;
                }
                else
                {
                    projectile.Position = position;
                    projectile.Rotation = rotation;
                }

                projectile.ServerActivate();

                new ThrowableNetworkHandler.ThrowableItemAudioMessage(projectile.Info.Serial, ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce).SendToAuthenticated();
            }

            return projectile;
        }

        #region Toys
        public static IEnumerable<T> GetToys<T>(Predicate<T> predicate) where T : AdminToy
    => _toys.Where<T>(toy => predicate(toy));

        public static IEnumerable<AdminToy> GetToys(Predicate<AdminToy> predicate)
            => _toys.Where(toy => predicate(toy));

        public static T GetToy<T>(AdminToyBase adminToyBase) where T : AdminToy
        {
            if (adminToyBase is null)
                return null;

            if (_toys.TryGetFirst(x => x.Base == adminToyBase, out var toy))
                return (T)toy;

            toy = AdminToy.Create(adminToyBase);

            _toys.Add(toy);
            return (T)toy;
        }

        public static AdminToy GetToy(AdminToyBase adminToyBase)
        {
            if (adminToyBase is null)
                return null;

            if (_toys.TryGetFirst(x => x.Base == adminToyBase, out var toy))
                return toy;

            toy = AdminToy.Create(adminToyBase);

            _toys.Add(toy);
            return toy;
        }
        #endregion

        #region Cameras
        public static IEnumerable<Camera> GetCameras(RoomName room)
            => GetCameras(x => x.RoomName == room);

        public static IEnumerable<Camera> GetCameras(FacilityZone zone)
            => GetCameras(x => x.Zone == zone);

        public static IEnumerable<Camera> GetCameras(Predicate<Camera> predicate)
            => Cameras.Where(x => predicate(x));

        public static Camera GetCamera(Enums.CameraType type)
            => GetCamera(x => x.Type == type);

        public static Camera GetCamera(ushort id)
            => GetCamera(x => x.Id == id);

        public static Camera GetCamera(string name)
            => GetCamera(x => x.Name == name);

        public static Camera GetCamera(Scp079Camera camera)
            => GetCamera(x => x.Base == camera);

        public static Camera GetCamera(Predicate<Camera> predicate)
            => Cameras.FirstOrDefault(x => predicate(x));
        #endregion

        #region Elevators
        public static IEnumerable<Elevator> GetElevators(Predicate<Elevator> predicate)
            => _elevators.Where(x => predicate(x));

        public static Elevator GetElevator(Vector3 position)
            => GetElevator(x => x.Contains(position));

        public static Elevator GetElevator(Predicate<Elevator> predicate)
            => _elevators.FirstOrDefault(x => predicate(x));

        public static Elevator GetElevator(ElevatorGroup group)
            => _elevators.FirstOrDefault(p => p.Group == group);

        public static Elevator GetElevator(ElevatorChamber chamber)
        {
            if (chamber is null)
                return null;

            if (!_elevators.TryGetFirst(x => x.Base == chamber, out var wrapper))
            {
                wrapper = new Elevator(chamber);

                _elevators.Add(wrapper);
                return wrapper;
            }

            return wrapper;
        }
        #endregion

        #region Doors
        public static IEnumerable<Door> GetDoors(RoomName room)
            => GetDoors(x => x.Rooms.Any(y => y.Name == room));

        public static IEnumerable<Door> GetDoors(FacilityZone zone)
            => GetDoors(x => x.Rooms.Any(y => y.Zone == zone));

        public static IEnumerable<Door> GetDoors(DoorType type)
            => GetDoors(x => x.Type == type);

        public static IEnumerable<Door> GetDoors(Predicate<Door> predicate)
            => _doors.Where(x => predicate(x));

        public static Door GetDoor(Predicate<Door> predicate)
            => _doors.FirstOrDefault(x => predicate(x));

        public static Door GetDoor(Collider collider)
        {
            if (collider.TryGetComponent<InteractableCollider>(out var interactableCollider)
                && interactableCollider.Target != null && interactableCollider.Target is DoorVariant door)
                return GetDoor(door);

            if (collider.TryGetComponent(out door))
                return GetDoor(door);

            if (collider.TryGetComponent<IInteractable>(out var interactable)
                && (door = (interactable as DoorVariant)) != null)
                return GetDoor(door);

            return null;
        }

        public static Door GetDoor(DoorType type)
        {
            if (type is DoorType.UnknownDoor || type is DoorType.UnknownElevator || type is DoorType.UnknownGate)
                return null;

            return Doors.FirstOrDefault(d => d.Type == type);
        }

        public static Door GetDoor(DoorVariant door)
        {
            if (door is null)
                return null;

            if (!_doors.TryGetFirst(x => x.Base == door, out var wrapper))
            {
                wrapper = new Door(door, Door.GetDoorType(door));

                _doors.Add(wrapper);
                return wrapper;
            }

            return wrapper;
        }

        private static Door CreateDoor(DoorVariant door)
        {
            if (door is CheckpointDoor checkpointDoor)
                return new Checkpoint(checkpointDoor, Door.GetDoorType(door));

            return new Door(door, Door.GetDoorType(door));
        }
        #endregion

        private static void OnRoundWaiting()
        {
            try
            {
                AmbientSoundPlayer = ExPlayer.Host.GameObject.GetComponent<AmbientSoundPlayer>();

                _generators.Clear();
                _elevators.Clear();
                _ragdolls.Clear();
                _airlocks.Clear();
                _pickups.Clear();
                _lockers.Clear();
                _doors.Clear();
                _gates.Clear();
                _toys.Clear();
                _cams.Clear();

                foreach (var gate in TeslaGate.AllGates)
                    _gates.Add(new ExTeslaGate(gate));

                foreach (var door in DoorVariant.AllDoors)
                    _doors.Add(new Door(door, Door.GetDoorType(door)));

                foreach (var airlock in UnityEngine.Object.FindObjectsOfType<AirlockController>())
                    _airlocks.Add(new Airlock(airlock));

                foreach (var elevator in ElevatorChamber.AllChambers)
                    _elevators.Add(new Elevator(elevator));

                foreach (var locker in UnityEngine.Object.FindObjectsOfType<Locker>())
                    _lockers.Add(locker);

                foreach (var toy in UnityEngine.Object.FindObjectsOfType<AdminToyBase>())
                    _toys.Add(AdminToy.Create(toy));

                foreach (var interactable in Scp079InteractableBase.AllInstances)
                {
                    if (interactable is null || interactable is not Scp079Camera cam)
                        continue;

                    _cams.Add(new Camera(cam));
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Map API", $"Map generation failed!\n{ex.ToColoredString()}");
            }
        }

        // this is so stupid
        private static void OnIdentityDestroyed(NetworkIdentity identity)
        {
            try
            {
                if (identity is null)
                    return;
                
                _generators.RemoveWhere(x => x.NetId == identity.netId);
                _airlocks.RemoveWhere(x => x.NetId == identity.netId);
                _ragdolls.RemoveWhere(x => x.netId == identity.netId);
                _pickups.RemoveWhere(x => x.netId == identity.netId);
                _lockers.RemoveWhere(x => x.netId == identity.netId);
                _gates.RemoveWhere(x => x.NetId == identity.netId);
                _doors.RemoveWhere(x => x.NetId == identity.netId);
                _toys.RemoveWhere(x => x.NetId == identity.netId);

                foreach (var player in ExPlayer.AllPlayers)
                {
                    if (!player)
                        continue;
                    
                    if (player.Inventory is null || player.Inventory._droppedItems is null)
                        continue;
                    
                    player.Inventory._droppedItems.RemoveWhere(x => x.netId == identity.netId);
                }

                if (identity.TryGetComponent<IInteractable>(out var interactable))
                    InteractableCollider.AllInstances.Remove(interactable);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Map API", ex);
            }
        }

        private static void OnRagdollSpawned(BasicRagdoll ragdoll)
        {
            if (ragdoll is null || !ragdoll)
                return;

            _ragdolls.Add(ragdoll);
        }

        private static void OnRagdollRemoved(BasicRagdoll ragdoll)
        {
            if (ragdoll is null || !ragdoll)
                return;

            _ragdolls.Remove(ragdoll);
        }

        [LoaderInitialize(1)]
        private static void Init()
        {
            RagdollManager.OnRagdollSpawned += OnRagdollSpawned;
            RagdollManager.OnRagdollRemoved += OnRagdollRemoved;

            MirrorEvents.OnDestroy += OnIdentityDestroyed;

            InternalEvents.OnRoundWaiting += OnRoundWaiting;
        }
    }
}