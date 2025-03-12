using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;

using LabExtended.API.Enums;
using LabExtended.API.Wrappers;
using LabExtended.Attributes;
using LabExtended.Events;
using LabExtended.Extensions;

using MapGeneration;

using UnityEngine;

namespace LabExtended.API
{
    /// <summary>
    /// Represents an in-game door.
    /// </summary>
    public class Door : NetworkWrapper<DoorVariant>
    {
        /// <summary>
        /// Exact open state of SCP-173's gate.
        /// </summary>
        public const float Scp173TimedGateOpenState = 0.5845918f;

        /// <summary>
        /// A list of all doors.
        /// </summary>
        public static Dictionary<DoorVariant, Door> Lookup { get; } = new();
        
        /// <summary>
        /// Tries to get a wrapper by it's base object.
        /// </summary>
        /// <param name="door">The base object.</param>
        /// <param name="wrapper">The found wrapper instance.</param>
        /// <returns>true if the wrapper instance was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet(DoorVariant door, out Door wrapper)
        {
            if (door is null)
                throw new ArgumentNullException(nameof(door));
            
            return Lookup.TryGetValue(door, out wrapper);
        }

        /// <summary>
        /// Tries to get a wrapper by a predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter by.</param>
        /// <param name="door">The found wrapper instance.</param>
        /// <returns>true if the wrapper instance was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet(Func<Door, bool> predicate, out Door? door)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            foreach (var pair in Lookup)
            {
                if (!predicate(pair.Value))
                    continue;
                
                door = pair.Value;
                return true;
            }
            
            door = null;
            return false;
        }

        /// <summary>
        /// Gets a wrapper instance by it's base object.
        /// </summary>
        /// <param name="door">The base object.</param>
        /// <returns>The found wrapper instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static Door Get(DoorVariant door)
        {
            if (door is null)
                throw new ArgumentNullException(nameof(door));
            
            if (!Lookup.TryGetValue(door, out var wrapper))
                throw new KeyNotFoundException($"Could not find door {door.netId}");
            
            return wrapper;
        }

        /// <summary>
        /// Gets a wrapper instance by a predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter by.</param>
        /// <returns>Wrapper instance if found, otherwise null.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Door? Get(Func<Door, bool> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            
            return TryGet(predicate, out var wrapper) ? wrapper : null;
        }

        internal Door(DoorVariant baseValue, DoorType type) : base(baseValue)
            => Type = type;

        /// <summary>
        /// Gets the door's type.
        /// </summary>
        public DoorType Type { get; }

        /// <summary>
        /// Gets the rooms this door is connecting.
        /// </summary>
        public IEnumerable<RoomIdentifier> Rooms => Base.Rooms ?? Array.Empty<RoomIdentifier>();
        
        /// <summary>
        /// Gets the door's colliders.
        /// </summary>
        public IEnumerable<Collider> Scp106Colliders => Base is BasicDoor basicDoor ? basicDoor.Scp106Colliders : Array.Empty<Collider>();

        /// <summary>
        /// Gets the door's game object.
        /// </summary>
        public GameObject GameObject => Base.gameObject;
        
        /// <summary>
        /// Gets the door's transform.
        /// </summary>
        public Transform Transform => Base.transform;

        /// <summary>
        /// Gets the door's name tag.
        /// </summary>
        public DoorNametagExtension NameTag => Base.GetComponent<DoorNametagExtension>();

        /// <summary>
        /// Gets the associated elevator chamber.
        /// </summary>
        public ElevatorChamber? ElevatorChamber => (Base is ElevatorDoor elevatorDoor) ? elevatorDoor.Chamber : null;
        
        /// <summary>
        /// Gets the associated elevator.
        /// </summary>
        public Elevator? Elevator => (Base is ElevatorDoor elevatorDoor) ? Elevator.Get(elevatorDoor.Chamber) : null;

        /// <summary>
        /// Whether the door is fully closed.
        /// </summary>
        public bool IsFullyClosed => State is 0f && (!IsGate || (Base as PryableDoor)._remainingPryCooldown <= 0f);
        
        /// <summary>
        /// Whether the door is fully opened.
        /// </summary>
        public bool IsFullyOpen => State is 1f || (IsTimedGate && (Base as Timed173PryableDoor).GetExactState() is Scp173TimedGateOpenState);

        /// <summary>
        /// Whether the door is considered to be open.
        /// </summary>
        public bool IsConsideredOpen => Base.IsConsideredOpen();

        /// <summary>
        /// Whether the door is currently moving.
        /// </summary>
        public bool IsMoving => (State > 0f && State < 1f || (IsGate && (Base as PryableDoor)._remainingPryCooldown > 0f));

        /// <summary>
        /// Whether the door is a gate.
        /// </summary>
        public bool IsGate => Base is PryableDoor;
        
        /// <summary>
        /// Whether the door belongs to an elevator chamber.
        /// </summary>
        public bool IsElevator => Base is ElevatorDoor;
        
        /// <summary>
        /// Whether the door is breakable.
        /// </summary>
        public bool IsBreakable => Base is BreakableDoor;
        
        /// <summary>
        /// Whether the door belongs to a checkpoint.
        /// </summary>
        public bool IsCheckpoint => Base is CheckpointDoor;
        
        /// <summary>
        /// Whether the door is a timed gate (SCP-173 gate).
        /// </summary>
        public bool IsTimedGate => Base is Timed173PryableDoor;
        
        /// <summary>
        /// Whether the door cannot be interacted with (safe zone gate).
        /// </summary>
        public bool IsNonInteractable => Base is BasicNonInteractableDoor;

        /// <summary>
        /// Whether SCP-106 can pass through this door.
        /// </summary>
        public bool IsScp106PassableDoor => Base is IScp106PassableDoor;

        /// <summary>
        /// Whether this door requires a keycard.
        /// </summary>
        public bool IsKeycardDoor => Permissions != KeycardPermissions.None;

        /// <summary>
        /// Whether this door has a name tag.
        /// </summary>
        public bool HasName => NameTag != null;

        /// <summary>
        /// Gets the door's exact state.
        /// </summary>
        public float State => Base.GetExactState();

        /// <summary>
        /// Gets the game object instance ID.
        /// </summary>
        public int InstanceId => Base.GetInstanceID();
        
        /// <summary>
        /// Gets the door's network ID.
        /// </summary>
        public uint NetworkId => Base.netId;

        /// <summary>
        /// Whether or not the door is opened.
        /// </summary>
        public bool IsOpened
        {
            get => Base.NetworkTargetState;
            set => Base.NetworkTargetState = value;
        }

        /// <summary>
        /// Whether or not the door is destroyed.
        /// </summary>
        public bool IsDestroyed
        {
            get => IsBreakable && ((BreakableDoor)Base).Network_destroyed;
            set
            {
                if (Base is not BreakableDoor breakableDoor)
                    return;

                if (value)
                    breakableDoor.Network_destroyed = true;
                else
                    breakableDoor.ServerRepair();
            }
        }

        /// <summary>
        /// Whether or not this door currently allows SCP-106 passage.
        /// </summary>
        public bool AllowsScp106
        {
            get => IsScp106PassableDoor && ((IScp106PassableDoor)Base).IsScp106Passable;
            set => (Base as IScp106PassableDoor)!.IsScp106Passable = value;
        }

        /// <summary>
        /// Whether or not this door ignores lockdowns.
        /// </summary>
        public bool IgnoresLockdowns
        {
            get
            {
                if (Base is BreakableDoor breakableDoor)
                    return breakableDoor.IgnoreLockdowns;

                if (Base is BasicNonInteractableDoor nonInteractableDoor)
                    return nonInteractableDoor.IgnoreLockdowns;

                return false;
            }
            set
            {
                if (Base is BreakableDoor breakableDoor)
                    breakableDoor._nonInteractable = value;

                if (Base is BasicNonInteractableDoor nonInteractableDoor)
                    nonInteractableDoor._ignoreLockdowns = value;
            }
        }

        /// <summary>
        /// Whether or not this door ignores Remote Admin commands.
        /// </summary>
        public bool IgnoresRemoteAdmin
        {
            get
            {
                if (Base is BreakableDoor breakableDoor)
                    return breakableDoor.IgnoreRemoteAdmin;

                if (Base is BasicNonInteractableDoor nonInteractableDoor)
                    return nonInteractableDoor.IgnoreRemoteAdmin;

                return false;
            }
            set
            {
                if (Base is BreakableDoor breakableDoor)
                    breakableDoor._nonInteractable = value;

                if (Base is BasicNonInteractableDoor nonInteractableDoor)
                    nonInteractableDoor._ignoreRemoteAdmin = value;
            }
        }

        /// <summary>
        /// Gets or sets the door's name.
        /// </summary>
        public string Name
        {
            get => NameTag?.GetName.RemoveBracketsOnEndOfName() ?? Base.name.GetBefore(' ');
            set => (NameTag ?? Base.gameObject.AddComponent<DoorNametagExtension>()).UpdateName(value);
        }

        /// <summary>
        /// Gets or sets the door's cooldown.
        /// </summary>
        public float Cooldown
        {
            get => Base is BasicDoor basicDoor ? basicDoor._cooldownDuration : 0f;
            set => (Base as BasicDoor)!._cooldownDuration = value;
        }

        /// <summary>
        /// Gets or sets the door's remaining cooldown.
        /// </summary>
        public float RemainingCooldown
        {
            get => Base is BasicDoor basicDoor ? basicDoor._remainingAnimCooldown : 0f;
            set => (Base as BasicDoor)!._remainingAnimCooldown = value;
        }

        /// <summary>
        /// Gets or sets the door's remaining pry cooldown.
        /// </summary>
        public float RemainingPryCooldown
        {
            get => Base is PryableDoor pryableDoor ? pryableDoor._remainingPryCooldown : 0f;
            set => (Base as PryableDoor)!._remainingPryCooldown = value;
        }

        /// <summary>
        /// Gets or sets the door's health.
        /// </summary>
        public float Health
        {
            get => IsBreakable ? ((BreakableDoor)Base).RemainingHealth : 0f;
            set => SetHealth(value);
        }

        /// <summary>
        /// Gets or sets the door's maximum health.
        /// </summary>
        public float MaxHealth
        {
            get => IsBreakable ? ((BreakableDoor)Base).MaxHealth : 0f;
            set => (Base as BreakableDoor)!.MaxHealth = value;
        }

        /// <summary>
        /// Gets or sets damage sources ignored by this door.
        /// </summary>
        public DoorDamageType IgnoredDamage
        {
            get => IsBreakable ? ((BreakableDoor)Base).IgnoredDamageSources : DoorDamageType.None;
            set => (Base as BreakableDoor)!.IgnoredDamageSources = value;
        }

        /// <summary>
        /// Gets or sets the door's active locks.
        /// </summary>
        public DoorLockReason ActiveLocks
        {
            get => (DoorLockReason)Base.NetworkActiveLocks;
            set => Base.NetworkActiveLocks = (ushort)value;
        }

        /// <summary>
        /// Gets or sets the door's blocking mask for prying.
        /// </summary>
        public DoorLockReason PryingMaskBlock
        {
            get => Base is PryableDoor pryableDoor ? pryableDoor._blockPryingMask : DoorLockReason.None;
            set => (Base as PryableDoor)!._blockPryingMask = value;
        }

        /// <summary>
        /// Gets or sets the door's required keycard permissions.
        /// </summary>
        public KeycardPermissions Permissions
        {
            get => Base.RequiredPermissions?.RequiredPermissions ?? KeycardPermissions.None;
            set => Base.RequiredPermissions!.RequiredPermissions = value;
        }

        /// <summary>
        /// Whether or not the specific lock is active.
        /// </summary>
        /// <param name="reason">The lock to check.</param>
        /// <returns>true if the lock is active.</returns>
        public bool GetLock(DoorLockReason reason)
            => ActiveLocks.HasFlagFast(reason);

        /// <summary>
        /// Sets the status of a specific lock.
        /// </summary>
        /// <param name="reason">The lock to set.</param>
        /// <param name="status">The lock's status.</param>
        public void SetLock(DoorLockReason reason, bool status)
            => Base.ServerChangeLock(reason, status);

        /// <summary>
        /// Enables a specific lock.
        /// </summary>
        /// <param name="reason">The lock to enable.</param>
        public void EnableLock(DoorLockReason reason = DoorLockReason.AdminCommand)
            => SetLock(reason, true);

        /// <summary>
        /// Disables a specific lock.
        /// </summary>
        /// <param name="reason">The lock to disable.</param>
        public void DisableLock(DoorLockReason reason = DoorLockReason.AdminCommand)
            => SetLock(reason, false);

        /// <summary>
        /// Disables a specific lock after a specific amount of time.
        /// </summary>
        /// <param name="time">The delay before disabling the lock.</param>
        /// <param name="reason">The lock to disable.</param>
        public void DisableLock(float time, DoorLockReason reason = DoorLockReason.AdminCommand)
            => DoorScheduledUnlocker.UnlockLater(Base, time, reason);

        /// <summary>
        /// Toggles a specific lock.
        /// </summary>
        /// <param name="reason">The lock.</param>
        public void ToggleLock(DoorLockReason reason = DoorLockReason.AdminCommand)
            => SetLock(reason, !GetLock(reason));

        /// <summary>
        /// Disables all active locks.
        /// </summary>
        public void DisableLocks()
            => Base.NetworkActiveLocks = 0;

        /// <summary>
        /// Locks this door for a specific amount of time.
        /// </summary>
        /// <param name="time">The amount of time to lock this door for.</param>
        /// <param name="reason">The lock reason.</param>
        public void Lock(float time, DoorLockReason reason = DoorLockReason.AdminCommand)
        {
            EnableLock(reason);
            DisableLock(time, reason);
        }

        /// <summary>
        /// Whether or not a player can interact with this door.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>true if the player can interact,</returns>
        public bool CanInteract(ExPlayer player = null)
            => Base.AllowInteracting(player?.ReferenceHub, 0);

        /// <summary>
        /// Tries to pry this door.
        /// </summary>
        /// <param name="player">The player to pry this door with.</param>
        /// <returns>true if the door was pried.</returns>
        public bool TryPry(ExPlayer player = null)
            => Base is PryableDoor pryableDoor && pryableDoor.TryPryGate(player?.ReferenceHub);

        /// <summary>
        /// Sets the door's health.
        /// </summary>
        /// <param name="health">The health to set.</param>
        public void SetHealth(float health)
        {
            if (Base is not BreakableDoor breakableDoor)
                return;

            if (breakableDoor.Network_destroyed && health > 0f)
            {
                breakableDoor.Network_destroyed = false;
                breakableDoor.RemainingHealth = health;

                return;
            }

            if (health <= 0f)
            {
                breakableDoor.Network_destroyed = true;
                return;
            }

            breakableDoor.RemainingHealth = health;
        }

        /// <summary>
        /// Heals the door to it's maximum health.
        /// </summary>
        public void SetMaxHealth()
        {
            if (Base is not BreakableDoor breakableDoor)
                return;

            SetHealth(breakableDoor.MaxHealth);
        }

        /// <summary>
        /// Damages the door.
        /// </summary>
        /// <param name="amount">The amount of damage.</param>
        /// <param name="type">The damage type.</param>
        public void Damage(float amount, DoorDamageType type = DoorDamageType.ServerCommand)
            => (Base as BreakableDoor)?.ServerDamage(amount, type);

        /// <summary>
        /// Destroys this door.
        /// </summary>
        /// <param name="type">Damage type.</param>
        public void Destroy(DoorDamageType type = DoorDamageType.ServerCommand)
            => (Base as BreakableDoor)?.ServerDamage(float.MaxValue, type);

        /// <summary>
        /// Repairs this door.
        /// </summary>
        /// <returns>true if the door was repaired.</returns>
        public bool Repair()
            => (Base as BreakableDoor)?.ServerRepair() ?? false;

        /// <summary>
        /// Plays the denied sound effect (missing keycard permissions).
        /// </summary>
        /// <param name="setButtons">Whether or not to show red buttons.</param>
        public void PlayDeniedSound(bool setButtons = true)
            => (Base as BasicDoor)?.RpcPlayBeepSound(setButtons);

        /// <summary>
        /// Gets the type of a door.
        /// </summary>
        /// <param name="door">The door.</param>
        /// <returns>The door's type.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static DoorType GetDoorType(DoorVariant door)
        {
            if (door is null)
                throw new ArgumentNullException(nameof(door));

            var nameTag = door.GetComponent<DoorNametagExtension>();
            var room = door.Rooms?.FirstOrDefault() ?? null;

            if (nameTag is null)
            {
                var doorName = door.name.GetBefore(' ');

                return doorName switch
                {
                    "LCZ" => room?.Name switch
                    {
                        RoomName.LczAirlock => door.GetComponentInParent<AirlockController>() != null
                                            ? DoorType.Airlock
                                            : DoorType.LightContainmentDoor,

                        _ => DoorType.LightContainmentDoor,
                    },

                    "Intercom" => room?.Name switch
                    {
                        RoomName.HczCheckpointA => DoorType.CheckpointArmoryA,
                        RoomName.HczCheckpointB => DoorType.CheckpointArmoryB,

                        _ => DoorType.UnknownDoor,
                    },

                    "Unsecured" => room?.Name switch
                    {
                        RoomName.HczCheckpointToEntranceZone => DoorType.CheckpointGate,

                        RoomName.Hcz049 => door.transform.position.y < -805
                                        ? DoorType.Scp049Gate
                                        : DoorType.Scp173NewGate,

                        _ => DoorType.UnknownGate,
                    },

                    "Elevator" => (door as ElevatorDoor)?.Group switch
                    {
                        ElevatorGroup.Nuke01 => DoorType.ElevatorNuke1,
                        ElevatorGroup.Nuke02 => DoorType.ElevatorNuke2,

                        ElevatorGroup.Scp049 => DoorType.ElevatorScp049,

                        ElevatorGroup.GateB => DoorType.ElevatorGateB,
                        ElevatorGroup.GateA => DoorType.ElevatorGateA,

                        ElevatorGroup.LczA01 or ElevatorGroup.LczA02 => DoorType.ElevatorLczA,
                        ElevatorGroup.LczB01 or ElevatorGroup.LczB02 => DoorType.ElevatorLczB,

                        _ => DoorType.UnknownElevator,
                    },

                    "HCZ" => DoorType.HeavyContainmentDoor,
                    "Prison" => DoorType.PrisonDoor,
                    "EZ" => DoorType.EntranceDoor,
                    "914" => DoorType.Scp914Door,

                    _ => DoorType.UnknownDoor,
                };
            }

            return nameTag.GetName.RemoveBracketsOnEndOfName() switch
            {
                "CHECKPOINT_LCZ_A" => DoorType.CheckpointLczA,
                "CHECKPOINT_LCZ_B" => DoorType.CheckpointLczB,

                "CHECKPOINT_EZ_HCZ_A" => DoorType.CheckpointEzHczA,
                "CHECKPOINT_EZ_HCZ_B" => DoorType.CheckpointEzHczB,

                "106_PRIMARY" => DoorType.Scp106Primary,
                "106_SECONDARY" => DoorType.Scp106Secondary,

                "ESCAPE_PRIMARY" => DoorType.EscapePrimary,
                "ESCAPE_SECONDARY" => DoorType.EscapeSecondary,

                "INTERCOM" => DoorType.Intercom,

                "NUKE_ARMORY" => DoorType.NukeArmory,
                "LCZ_ARMORY" => DoorType.LczArmory,

                "HID" => DoorType.HID,
                "HID_RIGHT" => DoorType.HIDRight,
                "HID_LEFT" => DoorType.HIDLeft,

                "HCZ_ARMORY" => DoorType.HczArmory,

                "096" => DoorType.Scp096,

                "049_ARMORY" => DoorType.Scp049Armory,

                "079_ARMORY" => DoorType.Scp079Armory,
                "079_FIRST" => DoorType.Scp079First,
                "079_SECOND" => DoorType.Scp079Second,

                "914" => DoorType.Scp914Gate,

                "GATE_A" => DoorType.GateA,
                "GATE_B" => DoorType.GateB,

                "173_CONNECTOR" => DoorType.Scp173Connector,
                "173_ARMORY" => DoorType.Scp173Armory,
                "173_GATE" => DoorType.Scp173Gate,
                "173_BOTTOM" => DoorType.Scp173Bottom,

                "939_CRYO" => DoorType.Scp939Cryo,

                "330" => DoorType.Scp330,
                "330_CHAMBER" => DoorType.Scp330Chamber,

                "LCZ_WC" => DoorType.LczWc,
                "LCZ_CAFE" => DoorType.LczCafe,

                "GR18" => DoorType.GR18Gate,
                "GR18_INNER" => DoorType.GR18Inner,

                "SURFACE_GATE" => DoorType.SurfaceGate,
                "SURFACE_NUKE" => DoorType.NukeSurface,

                "SERVERS_BOTTOM" => DoorType.ServersBottom,

                "LightContainmentDoor" => DoorType.LightContainmentDoor,
                "EntrDoor" => DoorType.EntranceDoor,

                _ => DoorType.UnknownDoor,
            };
        }

        private static void OnDoorSpawned(DoorVariant door)
            => Lookup.Add(door, new(door, GetDoorType(door)));

        private static void OnDoorDestroyed(DoorVariant door)
            => Lookup.Remove(door);

        [LoaderInitialize(1)]
        private static void OnInit()
        {
            DoorVariant.OnInstanceCreated += OnDoorSpawned;
            DoorVariant.OnInstanceRemoved += OnDoorDestroyed;

            InternalEvents.OnRoundRestart += Lookup.Clear;
        }
    }
}