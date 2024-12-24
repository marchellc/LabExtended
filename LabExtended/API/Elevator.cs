using Interactables.Interobjects;

using InventorySystem.Items.Pickups;

using LabExtended.API.Interfaces;
using LabExtended.API.Wrappers;

using MapGeneration;

using PlayerRoles;

using UnityEngine;

using Interactables.Interobjects.DoorUtils;

using LabApi.Features.Wrappers;

namespace LabExtended.API
{
    public class Elevator : Wrapper<ElevatorChamber>,
        IMapObject,

        INetworkedPosition,
        INetworkedRotation
    {
        public Elevator(ElevatorChamber baseValue) : base(baseValue) { }

        public ElevatorGroup Group => Base.AssignedGroup;

        public GameObject GameObject => Base.gameObject;
        public Transform Transform => Base.transform;

        public bool IsOperative => Base.IsReady;
        public bool IsLocked => Base.ActiveLocksAnyDoors != DoorLockReason.None;

        public bool IsMoving => Sequence is ElevatorChamber.ElevatorSequence.MovingAway || Sequence is ElevatorChamber.ElevatorSequence.Arriving;

        public float RotationTime => Base._rotationTime;

        public float DoorOpenTime => Base._doorOpenTime;
        public float DoorCloseTime => Base._doorCloseTime;

        public float MoveTime => AnimationTime + RotationTime + DoorOpenTime + DoorCloseTime;

        public bool IsInLcz => Base.DestinationDoor?.Rooms?.FirstOrDefault()?.Zone is FacilityZone.LightContainment;
        public bool IsInHcz => Base.DestinationDoor?.Rooms?.FirstOrDefault()?.Zone is FacilityZone.HeavyContainment;
        public bool IsInEz => Base.DestinationDoor?.Rooms?.FirstOrDefault()?.Zone is FacilityZone.Entrance;
        public bool IsInSurface => Base.DestinationDoor?.Rooms?.FirstOrDefault()?.Zone is FacilityZone.Surface;

        public bool IsDown => !IsUp;

        public Bounds Bounds => Base.WorldspaceBounds;

        public ElevatorChamber.ElevatorSequence Sequence
        {
            get => Base.CurSequence;
            set => Base.CurSequence = value;
        }

        public float AnimationTime
        {
            get => Base._animationTime;
            set => Base._animationTime = value;
        }

        public int Level
        {
            get => Base.Network_syncDestinationLevel;
            set => Move(value, true);
        }

        public bool IsInNuke
        {
            get
            {
                var room = Base.DestinationDoor?.Rooms?.FirstOrDefault();

                if (room is null)
                    return false;

                return room.Name is RoomName.HczWarhead && room.Shape is RoomShape.Curve;
            }
        }

        public bool IsIn049
        {
            get
            {
                var room = Base.DestinationDoor?.Rooms?.FirstOrDefault();

                if (room is null)
                    return false;

                return room.Name is RoomName.Hcz049 && room.Shape is RoomShape.Curve;
            }
        }

        public bool IsUp
        {
            get
            {
                if (Group is ElevatorGroup.GateA || Group is ElevatorGroup.GateB)
                    return IsInSurface;

                if (Group is ElevatorGroup.LczA01 || Group is ElevatorGroup.LczA02 || Group is ElevatorGroup.LczB01 || Group is ElevatorGroup.LczB02)
                    return IsInHcz;

                if (Group is ElevatorGroup.Nuke01 || Group is ElevatorGroup.Nuke02)
                    return IsInNuke;

                if (Group is ElevatorGroup.Scp049)
                    return IsIn049;

                return false;
            }
        }

        public Vector3 Position
        {
            get => Base.transform.position;
            set => Base.transform.position = value;
        }

        public Quaternion Rotation
        {
            get => Base.transform.rotation;
            set => Base.transform.rotation = value;
        }

        public Door Destination
        {
            get => ExMap.GetDoor(Base.DestinationDoor);
            set
            {
                if (value is null)
                    return;

                if (!ElevatorDoor.AllElevatorDoors.TryGetValue(Group, out var doors))
                    return;

                if (!doors.Contains(value.Base))
                    return;

                var index = doors.FindIndex(d => d != null && d == value.Base);

                if (index < 0)
                    return;

                Level = index;
            }
        }

        public void Move(int newLevel, bool forced = false)
        {
            if (newLevel == Level && !forced)
                return;

            Base.ServerSetDestination(newLevel, !forced);
        }

        public void Move(bool forced = false)
        {
            var doors = GetBaseDoors();
            var count = doors.Count();

            if (count < 1)
                return;

            var level = Level + 1;

            if (level >= count)
                level = 0;

            Move(level, forced);
        }

        public void SendDown(bool forced = false)
            => Move(Level - 1, forced);

        public void SendUp(bool forced = false)
            => Move(Level + 1, forced);

        public IEnumerable<ExPlayer> GetPlayers()
            => ExPlayer.Players.Where(Contains);

        public IEnumerable<ItemPickupBase> GetPickups()
            => ExMap.Pickups.Where(Contains);

        public IEnumerable<ElevatorDoor> GetBaseDoors()
            => ElevatorDoor.AllElevatorDoors.TryGetValue(Group, out var doors) ? doors : Array.Empty<ElevatorDoor>();

        public IEnumerable<Door> GetDoors()
            => GetBaseDoors().Select(ExMap.GetDoor);

        public bool Contains(Vector3 pos)
            => Bounds.Contains(pos);

        public bool Contains(Transform transform)
            => transform != null && Bounds.Contains(transform.position);

        public bool Contains(GameObject gameObject)
            => gameObject != null && Bounds.Contains(gameObject.transform.position);

        public bool Contains(MonoBehaviour behaviour)
            => behaviour != null && Bounds.Contains(behaviour.transform.position);

        public bool Contains(ExPlayer player)
            => player != null && player.Role.IsAlive && Bounds.Contains(player.Position);

        public bool Contains(Player player)
            => player != null && player.IsAlive && Bounds.Contains(player.Position);

        public bool Contains(ReferenceHub hub)
            => hub != null && hub.IsAlive() && Bounds.Contains(hub.transform.position);

        public bool Contains(ItemPickupBase item)
            => item != null && Bounds.Contains(item.Position);

        public void ChangeLock(DoorLockReason reason)
        {
            foreach (var door in GetBaseDoors())
            {
                if (reason is DoorLockReason.None)
                    door.NetworkActiveLocks = 0;
                else
                {
                    door.ServerChangeLock(reason, true);

                    if (Level != 1)
                        Move(1, true);
                }
            }
        }
    }
}