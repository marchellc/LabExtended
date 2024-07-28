using Decals;

using Hazards;

using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;

using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Pickups;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Enums;
using LabExtended.API.Npcs.Navigation;
using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities;

using LightContainmentZoneDecontamination;

using MapGeneration;
using MapGeneration.Distributors;

using Mirror;

using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Cameras;

using RelativePositioning;

using UnityEngine;

using Utils;
using Utils.Networking;

namespace LabExtended.API
{
    public static class ExMap
    {
        static ExMap()
            => NetworkDestroy.OnIdentityDestroyed += OnIdentityDestroyed;

        internal static readonly LockedHashSet<ItemPickupBase> _pickups = new LockedHashSet<ItemPickupBase>();
        internal static readonly LockedHashSet<Locker> _lockers = new LockedHashSet<Locker>();

        public static IEnumerable<ExTeslaGate> TeslaGates => ExTeslaGate._wrappers.Values;
        public static IEnumerable<Elevator> Elevators => Elevator._wrappers.Values;
        public static IEnumerable<Airlock> Airlocks => Airlock._wrappers.Values;
        public static IEnumerable<Camera> Cameras => Camera._wrappers.Values;
        public static IEnumerable<Door> Doors => Door._wrappers.Values;

        public static IReadOnlyList<ItemPickupBase> Pickups => _pickups;
        public static IReadOnlyList<Locker> Lockers => _lockers;

        public static IEnumerable<LockerChamber> LockerChambers => _lockers.SelectMany(x => x.Chambers);

        public static IEnumerable<Elevator> MovingElevators => GetElevators(x => x.IsMoving);
        public static IEnumerable<Elevator> ReadyElevators => GetElevators(x => x.Base.IsReady);

        public static IEnumerable<Door> OpenDoors => GetDoors(x => x.IsOpened);
        public static IEnumerable<Door> ClosedDoors => GetDoors(x => !x.IsOpened);
        public static IEnumerable<Door> MovingDoors => GetDoors(x => x.IsMoving);

        public static IEnumerable<Camera> ActiveCameras => GetCameras(x => x.IsUsed);
        public static IEnumerable<Camera> InactiveCameras => GetCameras(x => !x.IsUsed);

        public static AmbientSoundPlayer AmbientSoundPlayer { get; private set; }

        public static Color DefaultLightColor { get; } = Color.clear;

        public static int Seed
        {
            get => SeedSynchronizer.Seed;
            set
            {
                if (SeedSynchronizer.MapGenerated)
                    return;

                SeedSynchronizer._singleton.Network_syncSeed = value;
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
                return;

            AmbientSoundPlayer.RpcPlaySound(AmbientSoundPlayer.clips[id].index);
        }

        public static TantrumEnvironmentalHazard PlaceTantrum(Vector3 position, bool setActive = true)
        {
            var instance = UnityEngine.Object.Instantiate(Prefabs.TantrumPrefab);

            if (!setActive)
                instance.SynchronizedPosition = new RelativePosition(position);
            else
                instance.SynchronizedPosition = new RelativePosition(position + (Vector3.up * 0.25f));

            instance._destroyed = !setActive;

            NetworkServer.Spawn(instance.gameObject);

            return instance;
        }

        public static void PlaceDecal(Vector3 position, Vector3 direction, DecalPoolType type = DecalPoolType.Blood)
            => new GunDecalMessage(position, direction, type).SendToAuthenticated();

        public static void Explode(Vector3 position, ExPlayer attacker = null)
            => ExplosionUtils.ServerExplode(position, (attacker ?? ExPlayer.Host).Footprint);

        public static void SpawnExplosion(Vector3 position, ItemType type = ItemType.GrenadeHE)
            => ExplosionUtils.ServerSpawnEffect(position, type);

        public static void FlickerLights(float duration, params FacilityZone[] zones)
        {
            foreach (var light in RoomLightController.Instances)
            {
                var room = RoomIdentifier.AllRoomIdentifiers.FirstOrDefault(r => r != null && r.ApiRoom != null && r.ApiRoom.Lights != null && r.ApiRoom.Lights._lightController == light);

                if (room is null)
                    continue;

                if (zones.Length < 1 || zones.Contains(room.Zone))
                    light.ServerFlickerLights(duration);
            }
        }

        public static void SetLightColor(Color color)
        {
            PrimitiveUtils.FixColor(ref color);

            foreach (var light in RoomLightController.Instances)
                light.NetworkOverrideColor = color;
        }

        public static void ResetLightsColor()
        {
            foreach (var light in RoomLightController.Instances)
                light.NetworkOverrideColor = DefaultLightColor;
        }

        #region Cameras
        public static IEnumerable<Camera> GetNearCameras(Vector3 position, float toleration = 15f)
            => GetCameras(x => (position - x.Position).sqrMagnitude <= toleration * toleration);

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
            => Elevator._wrappers.Values.Where(x => predicate(x));

        public static Elevator GetElevator(Vector3 position)
            => GetElevator(x => x.Contains(position));

        public static Elevator GetElevator(Predicate<Elevator> predicate)
            => Elevator._wrappers.Values.FirstOrDefault(x => predicate(x));

        public static Elevator GetElevator(ElevatorManager.ElevatorGroup group)
            => Elevator._wrappers.Values.FirstOrDefault(p => p.Group == group);

        public static Elevator GetElevator(ElevatorChamber chamber)
        {
            if (chamber is null)
                return null;

            if (!Elevator._wrappers.TryGetValue(chamber, out var elevator))
                return Elevator._wrappers[chamber] = new Elevator(chamber);

            return elevator;
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
            => Door._wrappers.Values.Where(x => predicate(x));

        public static Door GetDoor(Predicate<Door> predicate)
            => Door._wrappers.Values.FirstOrDefault(x => predicate(x));

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

            if (!Door._wrappers.TryGetValue(door, out var wrapper))
                return Door._wrappers[door] = CreateDoor(door);

            return wrapper;
        }

        private static Door CreateDoor(DoorVariant door)
        {
            if (door is CheckpointDoor checkpointDoor)
                return new Checkpoint(checkpointDoor, Door.GetDoorType(door));

            return new Door(door, Door.GetDoorType(door));
        }
        #endregion

        internal static void GenerateMap()
        {
            try
            {
                AmbientSoundPlayer = ReferenceHub.HostHub.GetComponent<AmbientSoundPlayer>();

                ExTeslaGate._wrappers.Clear();
                Elevator._wrappers.Clear();
                Airlock._wrappers.Clear();
                Camera._wrappers.Clear();
                Door._wrappers.Clear();

                _pickups.Clear();
                _lockers.Clear();

                NavigationMesh.Prepare();

                if (TeslaGateController.Singleton != null)
                {
                    foreach (var gate in TeslaGateController.Singleton.TeslaGates)
                    {
                        ExTeslaGate._wrappers[gate] = new ExTeslaGate(gate);
                    }
                }
                else
                {
                    ExLoader.Warn("Map API", $"Attempted to reload Tesla Gates while the singleton is still null!");
                }

                foreach (var elevator in ElevatorManager.SpawnedChambers.Values)
                    Elevator._wrappers[elevator] = new Elevator(elevator);

                foreach (var door in DoorVariant.AllDoors)
                    Door._wrappers[door] = CreateDoor(door);

                foreach (var airlock in UnityEngine.Object.FindObjectsOfType<AirlockController>())
                    Airlock._wrappers[airlock] = new Airlock(airlock);

                foreach (var locker in UnityEngine.Object.FindObjectsOfType<Locker>())
                    _lockers.Add(locker);

                foreach (var interactable in Scp079InteractableBase.AllInstances)
                {
                    if (interactable is null || interactable is not Scp079Camera cam)
                        continue;

                    Camera._wrappers[cam] = new Camera(cam);
                }

                ExLoader.Debug("Map API", $"Finished populating objects, cache state:\n" +
                    $"Tesla {ExTeslaGate._wrappers.Count}\n" +
                    $"Elevator {Elevator._wrappers.Count}\n" +
                    $"Airlock {Airlock._wrappers.Count}\n" +
                    $"Camera {Camera._wrappers.Count}\n" +
                    $"Door {Door._wrappers.Count}\n" +
                    $"Lockers {_lockers.Count}");
            }
            catch (Exception ex)
            {
                ExLoader.Error("Map API", $"Map generation failed!\n{ex.ToColoredString()}");
            }
        }

        private static void OnIdentityDestroyed(NetworkIdentity identity)
        {
            try
            {
                var teslaRemoved = false;
                var doorRemoved = false;
                var airlockRemoved = false;

                var removedPickups = _pickups.RemoveWhere(x => x.netId == identity.netId);
                var removedLockers = _lockers.RemoveWhere(x => x.netId == identity.netId);

                if (ExTeslaGate._wrappers.TryGetFirst(x => x.Key.netId == identity.netId, out var tesla))
                    teslaRemoved = ExTeslaGate._wrappers.Remove(tesla.Key);

                if (Door._wrappers.TryGetFirst(x => x.Key.netId == identity.netId, out var door))
                    doorRemoved = Door._wrappers.Remove(door.Key);

                if (Airlock._wrappers.TryGetFirst(x => x.Key.netId == identity.netId, out var airlock))
                    airlockRemoved = Airlock._wrappers.Remove(airlock.Key);

                foreach (var player in ExPlayer.Players)
                    removedPickups += player._droppedItems.RemoveWhere(x => x.netId == identity.netId);

                ExLoader.Debug("Map API", $"Identity destroyed: {identity.netId} teslaRemoved={teslaRemoved} doorRemoved={doorRemoved} airlockRemoved={airlockRemoved} removedPickups={removedPickups} removedLockers={removedLockers}");
            }
            catch (Exception ex)
            {
                ExLoader.Error("Map API", ex);
            }
        }
    }
}