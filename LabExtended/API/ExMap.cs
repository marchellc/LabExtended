using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;

using InventorySystem.Items.Pickups;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Enums;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities;

using MapGeneration;
using MapGeneration.Distributors;

using Mirror;

using UnityEngine;

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
        public static IEnumerable<Door> Doors => Door._wrappers.Values;

        public static IReadOnlyList<ItemPickupBase> Pickups => _pickups;
        public static IReadOnlyList<Locker> Lockers => _lockers;

        public static IEnumerable<LockerChamber> LockerChambers => _lockers.SelectMany(x => x.Chambers);

        public static IEnumerable<Elevator> MovingElevators => GetElevators(x => x.IsMoving);
        public static IEnumerable<Elevator> ReadyElevators => GetElevators(x => x.Base.IsReady);

        public static IEnumerable<Door> OpenDoors => GetDoors(x => x.IsOpened);
        public static IEnumerable<Door> ClosedDoors => GetDoors(x => !x.IsOpened);
        public static IEnumerable<Door> MovingDoors => GetDoors(x => x.IsMoving);

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
                ExTeslaGate._wrappers.Clear();
                Elevator._wrappers.Clear();
                Airlock._wrappers.Clear();
                Door._wrappers.Clear();

                _pickups.Clear();
                _lockers.Clear();

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