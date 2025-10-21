using Interactables.Interobjects;

using InventorySystem.Items.Pickups;

using LabExtended.API.Interfaces;
using LabExtended.API.Wrappers;

using MapGeneration;

using PlayerRoles;

using UnityEngine;

using Interactables.Interobjects.DoorUtils;

using LabApi.Features.Wrappers;
using LabExtended.Attributes;
using LabExtended.Events;
using ElevatorDoor = Interactables.Interobjects.ElevatorDoor;

namespace LabExtended.API
{
    /// <summary>
    /// Represents an in-game elevator.
    /// </summary>
    public class Elevator : Wrapper<ElevatorChamber>,
        IMapObject,

        INetworkedPosition,
        INetworkedRotation
    {
        /// <summary>
        /// Gets all spawned elevators.
        /// </summary>
        public static Dictionary<ElevatorChamber, Elevator> Lookup { get; } = new();

        /// <summary>
        /// Tries to get a wrapper by it's base object.
        /// </summary>
        /// <param name="chamber">The base object.</param>
        /// <param name="elevator">The found wrapper instance.</param>
        /// <returns>true if the wrapper instance was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet(ElevatorChamber chamber, out Elevator elevator)
        {
            if (chamber is null)
                throw new ArgumentNullException(nameof(chamber));
            
            return Lookup.TryGetValue(chamber, out elevator);
        }

        /// <summary>
        /// Tries to get a wrapper by a predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter by.</param>
        /// <param name="elevator">The found wrapper instance.</param>
        /// <returns>true if the wrapper instance was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet(Func<Elevator, bool> predicate, out Elevator? elevator)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            foreach (var pair in Lookup)
            {
                if (!predicate(pair.Value))
                    continue;
                
                elevator = pair.Value;
                return true;
            }
            
            elevator = null;
            return false;
        }

        /// <summary>
        /// Gets a wrapper instance by it's base object.
        /// </summary>
        /// <param name="chamber">The base object.</param>
        /// <returns>The found wrapper instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static Elevator Get(ElevatorChamber chamber)
        {
            if (chamber is null)
                throw new ArgumentNullException(nameof(chamber));
            
            if (!Lookup.TryGetValue(chamber, out var elevator))
                throw new KeyNotFoundException($"Could not find elevator chamber {chamber.AssignedGroup}");
            
            return elevator;
        }

        /// <summary>
        /// Gets a wrapper instance by a predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter by.</param>
        /// <returns>Wrapper instance if found, otherwise null.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Elevator? Get(Func<Elevator, bool> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            
            return TryGet(predicate, out var elevator) ? elevator : null;
        }
        
        internal Elevator(ElevatorChamber baseValue) : base(baseValue) { }

        /// <summary>
        /// Gets the elevator's group.
        /// </summary>
        public ElevatorGroup Group => Base.AssignedGroup;

        /// <summary>
        /// Gets the elevator's game object.
        /// </summary>
        public GameObject GameObject => Base.gameObject;
        
        /// <summary>
        /// Gets the elevator's transform.
        /// </summary>
        public Transform Transform => Base.transform;

        /// <summary>
        /// Whether or not the elevator can be operated.
        /// </summary>
        public bool IsOperative => Base.IsReady;
        
        /// <summary>
        /// Whether or not the elevator is locked.
        /// </summary>
        public bool IsLocked => Base.ActiveLocksAnyDoors != DoorLockReason.None;

        /// <summary>
        /// Whether or not the elevator is moving.
        /// </summary>
        public bool IsMoving => Sequence is ElevatorChamber.ElevatorSequence.MovingAway || Sequence is ElevatorChamber.ElevatorSequence.Arriving;

        /// <summary>
        /// How long it takes to rotate the elevator chamber.
        /// </summary>
        public float RotationTime => Base._rotationTime;

        /// <summary>
        /// How long it takes for doors to open.
        /// </summary>
        public float DoorOpenTime => Base._doorOpenTime;
        
        /// <summary>
        /// How long it takes for doors to close.
        /// </summary>
        public float DoorCloseTime => Base._doorCloseTime;

        /// <summary>
        /// Total traversal duration.
        /// </summary>
        public float MoveTime => AnimationTime + RotationTime + DoorOpenTime + DoorCloseTime;

        /// <summary>
        /// Whether or not the elevator is located in Light Containment Zone.
        /// </summary>
        public bool IsInLcz => Base.DestinationDoor?.Rooms?.FirstOrDefault()?.Zone is FacilityZone.LightContainment;
        
        /// <summary>
        /// Whether or not the elevator is located in Heavy Containment Zone.
        /// </summary>
        public bool IsInHcz => Base.DestinationDoor?.Rooms?.FirstOrDefault()?.Zone is FacilityZone.HeavyContainment;
        
        /// <summary>
        /// Whether or not the elevator is located in Entrance Zone.
        /// </summary>
        public bool IsInEz => Base.DestinationDoor?.Rooms?.FirstOrDefault()?.Zone is FacilityZone.Entrance;
        
        /// <summary>
        /// Whether or not the elevator is located in Surface Zone.
        /// </summary>
        public bool IsInSurface => Base.DestinationDoor?.Rooms?.FirstOrDefault()?.Zone is FacilityZone.Surface;

        /// <summary>
        /// Whether or not the elevator is currently on it's lower destination.
        /// </summary>
        public bool IsDown => !IsUp;

        /// <summary>
        /// Gets the elevator chamber bounds.
        /// </summary>
        public Bounds Bounds => Base.WorldspaceBounds;

        /// <summary>
        /// Gets or sets the elevator's current sequence.
        /// </summary>
        public ElevatorChamber.ElevatorSequence Sequence
        {
            get => Base.CurSequence;
            set => Base.CurSequence = value;
        }

        /// <summary>
        /// Gets or sets the elevator's animation time.
        /// </summary>
        public float AnimationTime
        {
            get => Base._animationTime;
            set => Base._animationTime = value;
        }

        /// <summary>
        /// Gets or sets the elevator's current level.
        /// </summary>
        public int Level
        {
            get => Base.Network_syncDestinationLevel;
            set => Move(value, true);
        }

        /// <summary>
        /// Whether or not the elevator is currently in Alpha Warhead.
        /// </summary>
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

        /// <summary>
        /// Whether or not the elevator is currently in SCP-049.
        /// </summary>
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

        /// <summary>
        /// Whether or not the elevator is currently in it's higher destination.
        /// </summary>
        public bool IsUp
        {
            get
            {
                if (Group is ElevatorGroup.GateA01
                    or ElevatorGroup.GateA02
                    or ElevatorGroup.GateB)
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

        /// <summary>
        /// Gets or sets the elevator's position.
        /// </summary>
        public Vector3 Position
        {
            get => Base.transform.position;
            set => Base.transform.position = value;
        }

        /// <summary>
        /// Gets or sets the elevator's rotation.
        /// </summary>
        public Quaternion Rotation
        {
            get => Base.transform.rotation;
            set => Base.transform.rotation = value;
        }

        /// <summary>
        /// Gets or sets the elevator's destination door.
        /// </summary>
        public Door Destination
        {
            get => Door.Get(Base.DestinationDoor);
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

        /// <summary>
        /// Moves the elevator to the specified level.
        /// </summary>
        /// <param name="newLevel">The level to move to.</param>
        /// <param name="forced">Whether or not to force the move.</param>
        public void Move(int newLevel, bool forced = false)
        {
            if (newLevel == Level && !forced)
                return;

            Base.ServerSetDestination(newLevel, !forced);
        }

        /// <summary>
        /// Moves the elevator to the next level.
        /// </summary>
        /// <param name="forced">Whether or not to force the move.</param>
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

        /// <summary>
        /// Sends the elevator to it's lower destination.
        /// </summary>
        /// <param name="forced">Whether or not to force the move.</param>
        public void SendDown(bool forced = false)
            => Move(Level - 1, forced);

        /// <summary>
        /// Sends the elevator to it's higher destination.
        /// </summary>
        /// <param name="forced">Whether or not to force the move.</param>
        public void SendUp(bool forced = false)
            => Move(Level + 1, forced);

        /// <summary>
        /// Gets players located in this elevator.
        /// </summary>
        /// <returns>List of players.</returns>
        public IEnumerable<ExPlayer> GetPlayers()
            => ExPlayer.Players.Where(Contains);

        /// <summary>
        /// Gets pickups located in this elevator.
        /// </summary>
        /// <returns>List of pickups.</returns>
        public IEnumerable<ItemPickupBase> GetPickups()
            => ExMap.Pickups.Where(Contains);

        /// <summary>
        /// Gets the elevator's doors.
        /// </summary>
        /// <returns>A list of elevator doors.</returns>
        public IEnumerable<ElevatorDoor> GetBaseDoors()
            => ElevatorDoor.AllElevatorDoors.TryGetValue(Group, out var doors) ? doors : Array.Empty<ElevatorDoor>();

        /// <summary>
        /// Gets the elevator's doors.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Door> GetDoors()
            => GetBaseDoors().Select(Door.Get);

        /// <summary>
        /// Whether or not the elevator's bounds encapsulate a specific position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns></returns>
        public bool Contains(Vector3 pos)
            => Bounds.Contains(pos);

        /// <summary>
        /// Whether or not the elevator's bounds encapsulate a specific position.
        /// </summary>
        /// <param name="transform">The position.</param>
        /// <returns></returns>
        public bool Contains(Transform transform)
            => transform != null && Bounds.Contains(transform.position);

        /// <summary>
        /// Whether or not the elevator's bounds encapsulate a specific position.
        /// </summary>
        /// <param name="gameObject">The position.</param>
        /// <returns></returns>
        public bool Contains(GameObject gameObject)
            => gameObject != null && Bounds.Contains(gameObject.transform.position);

        /// <summary>
        /// Whether or not the elevator's bounds encapsulate a specific position.
        /// </summary>
        /// <param name="behaviour">The position.</param>
        /// <returns></returns>
        public bool Contains(MonoBehaviour behaviour)
            => behaviour != null && Bounds.Contains(behaviour.transform.position);

        /// <summary>
        /// Whether or not the elevator's bounds encapsulate a specific position.
        /// </summary>
        /// <param name="player">The position.</param>
        /// <returns></returns>
        public bool Contains(ExPlayer player)
            => player != null && player.Role.IsAlive && Bounds.Contains(player.Position);

        /// <summary>
        /// Whether or not the elevator's bounds encapsulate a specific position.
        /// </summary>
        /// <param name="player">The position.</param>
        /// <returns></returns>
        public bool Contains(Player player)
            => player != null && player.IsAlive && Bounds.Contains(player.Position);

        /// <summary>
        /// Whether or not the elevator's bounds encapsulate a specific position.
        /// </summary>
        /// <param name="hub">The position.</param>
        /// <returns></returns>
        public bool Contains(ReferenceHub hub)
            => hub != null && hub.IsAlive() && Bounds.Contains(hub.transform.position);

        /// <summary>
        /// Whether or not the elevator's bounds encapsulate a specific position.
        /// </summary>
        /// <param name="item">The position.</param>
        /// <returns></returns>
        public bool Contains(ItemPickupBase item)
            => item != null && Bounds.Contains(item.Position);

        /// <summary>
        /// Sets the lock on all doors.
        /// </summary>
        /// <param name="reason">The lock's reason.</param>
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

        private static void OnElevatorSpawned(ElevatorChamber chamber)
            => Lookup.Add(chamber, new(chamber));

        private static void OnElevatorDestroyed(ElevatorChamber chamber)
            => Lookup.Remove(chamber);

        internal static void Internal_Init()
        {
            ElevatorChamber.OnElevatorSpawned += OnElevatorSpawned;
            ElevatorChamber.OnElevatorRemoved += OnElevatorDestroyed;

            InternalEvents.OnRoundRestart += Lookup.Clear;
        }
    }
}