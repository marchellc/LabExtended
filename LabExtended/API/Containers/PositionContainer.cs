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
        /// Gets the default gravity value.
        /// </summary>
        public static Vector3 DefaultGravity => FpcGravityController.DefaultGravity;
        
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
        public FakeValue<Vector3> FakedList { get; } = new();

        /// <summary>
        /// Gets the player's current room.
        /// </summary>
        public RoomIdentifier Room => RoomIdUtils.RoomAtPosition(Position);

        /// <summary>
        /// Gets the elevator this player is currently in.
        /// </summary>
        public Elevator? CurrentElevator => Elevator.Get(x => x.Contains(Player));

        /// <summary>
        /// Gets the closest elevator.
        /// </summary>
        public Elevator? ClosestElevator
        {
            get
            {
                var closestLift = default(Elevator);
                var closestDistance = 0f;
                
                foreach (var pair in Elevator.Lookup)
                {
                    if (closestLift is null)
                    {
                        closestLift = pair.Value;
                        closestDistance = Vector3.Distance(pair.Value.Position, Position);

                        continue;
                    }

                    var distance = Vector3.Distance(pair.Value.Position, Position);

                    if (distance > closestDistance)
                        continue;

                    closestLift = pair.Value;
                    closestDistance = distance;
                }

                return closestLift;
            }
        }

        /// <summary>
        /// Gets the closest door.
        /// </summary>
        public Door? ClosestDoor
        {
            get
            {
                var closestDoor = default(Door);
                var closestDistance = 0f;
                
                foreach (var pair in Door.Lookup)
                {
                    if (closestDoor is null)
                    {
                        closestDoor = pair.Value;
                        closestDistance = Vector3.Distance(pair.Value.Position, Position);

                        continue;
                    }

                    var distance = Vector3.Distance(pair.Value.Position, Position);

                    if (distance > closestDistance)
                        continue;

                    closestDoor = pair.Value;
                    closestDistance = distance;
                }

                return closestDoor;
            }
        }

        /// <summary>
        /// Gets the closest SCP-079 camera.
        /// </summary>
        public Camera? ClosestCamera
        {
            get
            {
                var closestCamera = default(Camera);
                var closestDistance = 0f;

                foreach (var pair in Camera.Lookup)
                {
                    if (closestCamera is null)
                    {
                        closestCamera = pair.Value;
                        closestDistance = Vector3.Distance(pair.Value.Position, Position);

                        continue;
                    }

                    var distance = Vector3.Distance(pair.Value.Position, Position);

                    if (distance > closestDistance)
                        continue;

                    closestCamera = pair.Value;
                    closestDistance = distance;
                }

                return closestCamera;
            }
        }

        /// <summary>
        /// Gets the closest player.
        /// </summary>
        public ExPlayer? ClosestPlayer
        {
            get
            {
                var closestPlayer = default(ExPlayer);
                var closestDistance = 0f;

                ExPlayer.AllPlayers.ForEach(player =>
                {
                    if (!player.Role.IsAlive) return;
                    if (player == Player) return;

                    if (closestPlayer is null)
                    {
                        closestPlayer = player;
                        closestDistance = Vector3.Distance(player.Position, Player.Position);

                        return;
                    }

                    var distance = Vector3.Distance(player.Position, Player.Position);

                    if (distance > closestDistance) 
                        return;
                    
                    closestPlayer = player;
                    closestDistance = distance;
                });

                return closestPlayer;
            }
        }

        /// <summary>
        /// Gets the closest SCP player.
        /// </summary>
        public ExPlayer? ClosestScp
        {
            get
            {
                var closestPlayer = default(ExPlayer);
                var closestDistance = 0f;

                ExPlayer.AllPlayers.ForEach(player =>
                {
                    if (!player.Role.IsScp) return;
                    if (player == Player) return;

                    if (closestPlayer is null)
                    {
                        closestPlayer = player;
                        closestDistance = Vector3.Distance(player.Position, Player.Position);

                        return;
                    }

                    var distance = Vector3.Distance(player.Position, Player.Position);

                    if (distance > closestDistance)
                        return;

                    closestPlayer = player;
                    closestDistance = distance;
                });

                return closestPlayer;
            }
        }

        /// <summary>
        /// Gets the closest waypoint.
        /// </summary>
        public WaypointBase? WayPoint
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
        /// Gets or sets the player's current position.
        /// </summary>
        public Vector3 Position
        {
            get => Player.Transform.position;
            set => Set(value);
        }

        /// <summary>
        /// Gets or sets the player's gravity.
        /// </summary>
        public Vector3 Gravity
        {
            get => Player.Role.GravityController?.Gravity ?? Vector3.zero;
            set => Player.Role.GravityController!.Gravity = value;
        }

        /// <summary>
        /// Gets or sets the player's current relative position.
        /// </summary>
        public RelativePosition Relative
        {
            get => Player.Role.Motor?.ReceivedPosition ?? new(Position);
            set => Set(value.Position);
        }

        /// <summary>
        /// Checks if player is grounded.
        /// </summary>
        public bool IsGrounded => Player.RoleBase is IFpcRole fpcRole && fpcRole.FpcModule.IsGrounded;

        /// <summary>
        /// Gets the player's current ground position, where player is actually standing on.<br />
        /// Returns <see langword="null" /> when Player isn't grounded.
        /// </summary>
        public Vector3? GroundPosition 
        {
            get 
            {
                if (PhysicsUtils.TryGetGroundPosition(Player, out var groundPosition))
                    return groundPosition;
                
                return null;
            }
        }

        /// <summary>
        /// Sets the player's position.
        /// </summary>
        /// <param name="position">The position to set.</param>
        public void Set(Vector3 position)
            => Player.ReferenceHub.TryOverridePosition(position);

        /// <summary>
        /// Gets a list of players in a specified range.
        /// </summary>
        /// <param name="range">The maximum range.</param>
        /// <returns>A list of players that are in range.</returns>
        public IEnumerable<ExPlayer> GetPlayersInRange(float range)
            => ExPlayer.AllPlayers.Where(p => p.NetworkId != Player.NetworkId && p.Role.IsAlive && p.Position.DistanceTo(Player) <= range);

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
            => player is null ? 0f : Vector3.Distance(player.Position.Position, Position);

        /// <summary>
        /// Gets the distance to a specified transform.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <returns>The distance.</returns>
        public float DistanceTo(Transform transform)
            => transform is null ? 0f : Vector3.Distance(transform.position, Position);

        /// <summary>
        /// Gets the distance to a specified gameObject.
        /// </summary>
        /// <param name="gameObject">The gameObject.</param>
        /// <returns>The distance.</returns>
        public float DistanceTo(GameObject gameObject)
            => gameObject is null ? 0f : Vector3.Distance(gameObject.transform.position, Position);

        /// <summary>
        /// Resets gravity to it's default for all players.
        /// <param name="predicate">The predicate to filter players by.</param>
        /// </summary>
        public static void ResetGravity(Func<ExPlayer, bool> predicate = null)
            => SetGravity(DefaultGravity, predicate);
        
        /// <summary>
        /// Sets gravity for all players.
        /// </summary>
        /// <param name="gravity">The gravity to set.</param>
        /// <param name="predicate">The predicate to filter players by.</param>
        public static void SetGravity(Vector3 gravity, Func<ExPlayer, bool> predicate = null)
        {
            ExPlayer.Players.ForEach(ply =>
            {
                if (!ply) return;
                if (predicate != null && !predicate(ply)) return;
                
                ply.Position.Gravity = gravity;
            });
        }

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