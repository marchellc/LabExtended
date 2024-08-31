using LabExtended.Utilities;
using LabExtended.Utilities.Values;

using MapGeneration;

using PlayerRoles.FirstPersonControl;

using RelativePositioning;

using UnityEngine;

namespace LabExtended.API.Containers
{
    /// <summary>
    /// A class used to manage player position.
    /// </summary>
    public class PositionContainer
    {
        /// <summary>
        /// Creates a new <see cref="PositionContainer"/> instance.
        /// </summary>
        /// <param name="player">Targeted player.</param>
        public PositionContainer(ExPlayer player)
            => Player = player;

        /// <summary>
        /// Gets the targeted player.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the fake list for desyncing positions.
        /// </summary>
        public FakeValue<Vector3> FakedList { get; } = new FakeValue<Vector3>();

        /// <summary>
        /// Gets the player's current room.
        /// </summary>
        public RoomIdentifier Room => RoomIdUtils.RoomAtPosition(Position);

        /// <summary>
        /// Gets the elevator this player is currently in.
        /// </summary>
        public Elevator Elevator => ExMap.Elevators.FirstOrDefault(elevator => elevator.Contains(Player));

        /// <summary>
        /// Gets the closest door.
        /// </summary>
        public Door ClosestDoor => ExMap.Doors.OrderBy(d => DistanceTo(d.Position)).FirstOrDefault();

        /// <summary>
        /// Gets the closest SCP-079 camera.
        /// </summary>
        public Camera ClosestCamera => ExMap.Cameras.OrderBy(x => Vector3.Distance(x.Position, Position)).First();

        /// <summary>
        /// Gets the closest player.
        /// </summary>
        public ExPlayer ClosestPlayer => ExPlayer.Players.Where(p => p.NetId != Player.NetId && p.Role.IsAlive).OrderBy(DistanceTo).FirstOrDefault();

        /// <summary>
        /// Gets the closest SCP player.
        /// </summary>
        public ExPlayer ClosestScp => ExPlayer.Players.Where(p => p.NetId != Player.NetId && p.Role.IsScp).OrderBy(DistanceTo).FirstOrDefault();

        /// <summary>
        /// Gets the closest waypoint.
        /// </summary>
        public WaypointBase WayPoint
        {
            get
            {
                WaypointBase.GetRelativePosition(Position, out var waypointId, out _);

                if (!WaypointBase.TryGetWaypoint(waypointId, out var waypoint))
                    return null;

                return waypoint;
            }
        }

        /// <summary>
        /// Gets the player's current position.
        /// </summary>
        public Vector3 Position
        {
            get => Player.Transform.position;
            set => Set(value);
        }

        /// <summary>
        /// Gets the player's current relative position.
        /// </summary>
        public RelativePosition Relative
        {
            get => new RelativePosition(Position);
            set => Set(value.Position);
        }

        /// <summary>
        /// Gets the player's current ground position, where player is actually standing on.<br />
        /// Returns <see langword="null" /> when Player isn't grounded.
        /// </summary>
        public Vector3? GroundPosition {
            get {
                if (PhysicsUtils.TryGetGroundPosition(Player, out var groundPosition)) {
                    return groundPosition;
                }
                return null;
            }
        }

        public void Set(Vector3 position, Vector3? rotationDelta = null)
            => Player.Hub.TryOverridePosition(position, rotationDelta.HasValue ? rotationDelta.Value : Vector3.zero);

        /// <summary>
        /// Gets a list of players in a specified range.
        /// </summary>
        /// <param name="range">The maximum range.</param>
        /// <returns>A list of players that are in range.</returns>
        public IEnumerable<ExPlayer> GetPlayersInRange(float range)
            => ExPlayer.Players.Where(p => p.NetId != Player.NetId && p.Role.IsAlive && p.Position.DistanceTo(Player) <= range);

        /// <summary>
        /// Gets the distance to a specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The distance.</returns>
        public float DistanceTo(Vector3 position)
            => Vector3.Distance(Position, position);

        /// <summary>
        /// Gets the distance to a specified player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>The distance.</returns>
        public float DistanceTo(ExPlayer player)
            => Vector3.Distance(player.Position.Position, Position);

        /// <summary>
        /// Gets the distance to a specified transform.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <returns>The distance.</returns>
        public float DistanceTo(Transform transform)
            => Vector3.Distance(transform.position, Position);

        /// <summary>
        /// Gets the distance to a specified gameObject.
        /// </summary>
        /// <param name="gameObject">The gameObject.</param>
        /// <returns>The distance.</returns>
        public float DistanceTo(GameObject gameObject)
            => Vector3.Distance(gameObject.transform.position, Position);

        /// <summary>
        /// Converts the specified <see cref="PositionContainer"/> instance to a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="container">The instance to convert.</param>
        public static implicit operator Vector3(PositionContainer container)
            => container?.Position ?? Vector3.zero;

        /// <summary>
        /// Converts the specified <see cref="PositionContainer"/> instance to a <see cref="RelativePosition"/>.
        /// </summary>
        /// <param name="container">The instance to convert.</param>
        public static implicit operator RelativePosition(PositionContainer container)
            => container?.Relative ?? new RelativePosition(Vector3.zero);
    }
}