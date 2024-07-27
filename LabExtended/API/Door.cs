using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Interfaces;
using LabExtended.API.Enums;

using LabExtended.Extensions;

using MapGeneration;

using Mirror;

using UnityEngine;

namespace LabExtended.API
{
    public class Door : Wrapper<DoorVariant>,
        IMapObject,

        INetworkedPosition,
        INetworkedRotation
    {
        internal static readonly LockedDictionary<DoorVariant, Door> _wrappers = new LockedDictionary<DoorVariant, Door>();

        public const float Scp173TimedGateOpenState = 0.5845918f;

        public Door(DoorVariant baseValue, DoorType type) : base(baseValue)
            => Type = type;

        public DoorType Type { get; }

        public ElevatorManager.ElevatorGroup? ElevatorGroup => Base is ElevatorDoor elevatorDoor ? elevatorDoor.Group : null;

        public IEnumerable<RoomIdentifier> Rooms => Base.Rooms ?? Array.Empty<RoomIdentifier>();
        public IEnumerable<Collider> Scp106Colliders => Base is BasicDoor basicDoor ? basicDoor.Scp106Colliders : Array.Empty<Collider>();

        public GameObject GameObject => Base.gameObject;
        public Transform Transform => Base.transform;

        public DoorNametagExtension NameTag => Base.GetComponent<DoorNametagExtension>();

        public ElevatorPanel ElevatorPanel => (Base is ElevatorDoor elevatorDoor) ? elevatorDoor.TargetPanel : null;
        public Elevator Elevator => (Base is ElevatorDoor elevatorDoor) ? ExMap.GetElevator(elevatorDoor.Group) : null;

        public bool IsFullyClosed => State is 0f && (!IsGate || (Base as PryableDoor)._remainingPryCooldown <= 0f);
        public bool IsFullyOpen => State is 1f || (IsTimedGate && (Base as Timed173PryableDoor).GetExactState() is Scp173TimedGateOpenState);

        public bool IsConsideredOpen => Base.IsConsideredOpen();

        public bool IsMoving => (State > 0f && State < 1f || (IsGate && (Base as PryableDoor)._remainingPryCooldown > 0f);

        public bool IsGate => Base is PryableDoor;
        public bool IsElevator => Base is ElevatorDoor;
        public bool IsBreakable => Base is BreakableDoor;
        public bool IsCheckpoint => Base is CheckpointDoor;
        public bool IsTimedGate => Base is Timed173PryableDoor;
        public bool IsNonInteractable => Base is BasicNonInteractableDoor;

        public bool IsScp106PassableDoor => Base is IScp106PassableDoor;

        public bool IsKeycardDoor => Permissions != KeycardPermissions.None;

        public bool HasName => NameTag != null;

        public float State => Base.GetExactState();

        public int InstanceId => Base.GetInstanceID();
        public uint NetworkId => Base.netId;

        public bool IsOpened
        {
            get => Base.NetworkTargetState;
            set => Base.NetworkTargetState = value;
        }

        public bool IsDestroyed
        {
            get => IsBreakable && ((BreakableDoor)Base).Network_destroyed;
            set => (Base as BreakableDoor)!.Network_destroyed = value;
        }

        public bool AllowsScp106
        {
            get => IsScp106PassableDoor && ((IScp106PassableDoor)Base).IsScp106Passable;
            set => (Base as IScp106PassableDoor)!.IsScp106Passable = value;
        }

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

        public string Name
        {
            get => NameTag?.GetName.RemoveBracketsOnEndOfName() ?? Base.name.GetBefore(' ');
            set => (NameTag ?? Base.gameObject.AddComponent<DoorNametagExtension>()).UpdateName(value);
        }

        public float Cooldown
        {
            get => Base is BasicDoor basicDoor ? basicDoor._cooldownDuration : 0f;
            set => (Base as BasicDoor)!._cooldownDuration = value;
        }

        public float RemainingCooldown
        {
            get => Base is BasicDoor basicDoor ? basicDoor._remainingAnimCooldown : 0f;
            set => (Base as BasicDoor)!._remainingAnimCooldown = value;
        }

        public float RemainingPryCooldown
        {
            get => Base is PryableDoor pryableDoor ? pryableDoor._remainingPryCooldown : 0f;
            set => (Base as PryableDoor)!._remainingPryCooldown = value;
        }

        public float Health
        {
            get => IsBreakable ? ((BreakableDoor)Base).RemainingHealth : 0f;
            set => SetHealth(value);
        }

        public float MaxHealth
        {
            get => IsBreakable ? ((BreakableDoor)Base).MaxHealth : 0f;
            set => (Base as BreakableDoor)!.MaxHealth = value;
        }

        public DoorDamageType IgnoredDamage
        {
            get => IsBreakable ? ((BreakableDoor)Base).IgnoredDamageSources : DoorDamageType.None;
            set => (Base as BreakableDoor)!.IgnoredDamageSources = value;
        }

        public DoorLockReason ActiveLocks
        {
            get => (DoorLockReason)Base.NetworkActiveLocks;
            set => Base.NetworkActiveLocks = (ushort)value;
        }

        public DoorLockReason PryingMaskBlock
        {
            get => Base is PryableDoor pryableDoor ? pryableDoor._blockPryingMask : DoorLockReason.None;
            set => (Base as PryableDoor)!._blockPryingMask = value;
        }

        public KeycardPermissions Permissions
        {
            get => Base.RequiredPermissions?.RequiredPermissions ?? KeycardPermissions.None;
            set => Base.RequiredPermissions!.RequiredPermissions = value;
        }

        public Vector3 Position
        {
            get => Base.transform.position;
            set
            {
                NetworkServer.UnSpawn(GameObject);

                Base.transform.position = value;

                NetworkServer.Spawn(GameObject);
            }
        }

        public Vector3 Scale
        {
            get => Base.transform.localScale;
            set
            {
                NetworkServer.UnSpawn(GameObject);

                Base.transform.localScale = value;

                NetworkServer.Spawn(GameObject);
            }
        }

        public Quaternion Rotation
        {
            get => Base.transform.rotation;
            set
            {
                NetworkServer.UnSpawn(GameObject);

                Base.transform.rotation = value;

                NetworkServer.Spawn(GameObject);
            }
        }

        public bool GetLock(DoorLockReason reason)
            => ActiveLocks.HasFlagFast(reason);

        public void SetLock(DoorLockReason reason, bool status)
            => Base.ServerChangeLock(reason, status);

        public void EnableLock(DoorLockReason reason = DoorLockReason.AdminCommand)
            => SetLock(reason, true);

        public void DisableLock(DoorLockReason reason = DoorLockReason.AdminCommand)
            => SetLock(reason, false);

        public void DisableLock(float time, DoorLockReason reason = DoorLockReason.AdminCommand)
            => DoorScheduledUnlocker.UnlockLater(Base, time, reason);

        public void ToggleLock(DoorLockReason reason)
            => SetLock(reason, !GetLock(reason));

        public void DisableLocks()
            => Base.NetworkActiveLocks = 0;

        public void Lock(float time, DoorLockReason reason)
        {
            EnableLock(reason);
            DisableLock(time, reason);
        }

        public bool CanInteract(ExPlayer player = null)
            => Base.AllowInteracting(player?.Hub, 0);

        public bool TryPry(ExPlayer player = null)
            => Base is PryableDoor pryableDoor && pryableDoor.TryPryGate(player?.Hub);

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, Vector3? scale = null)
        {
            NetworkServer.UnSpawn(GameObject);

            Base.transform.position = position;
            Base.transform.rotation = rotation;

            if (scale.HasValue)
                Base.transform.localScale = scale.Value;

            NetworkServer.Spawn(GameObject);
        }

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

        public void SetMaxHealth()
        {
            if (Base is not BreakableDoor breakableDoor)
                return;

            SetHealth(breakableDoor.MaxHealth);
        }

        public void Damage(float amount, DoorDamageType type = DoorDamageType.ServerCommand)
            => (Base as BreakableDoor)?.ServerDamage(amount, type);

        public void Destroy(DoorDamageType type = DoorDamageType.ServerCommand)
            => (Base as BreakableDoor)?.ServerDamage(float.MaxValue, type);

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
                        ElevatorManager.ElevatorGroup.Nuke => DoorType.ElevatorNuke,

                        ElevatorManager.ElevatorGroup.Scp049 => DoorType.ElevatorScp049,

                        ElevatorManager.ElevatorGroup.GateB => DoorType.ElevatorGateB,
                        ElevatorManager.ElevatorGroup.GateA => DoorType.ElevatorGateA,

                        ElevatorManager.ElevatorGroup.LczA01 or ElevatorManager.ElevatorGroup.LczA02 => DoorType.ElevatorLczA,
                        ElevatorManager.ElevatorGroup.LczB01 or ElevatorManager.ElevatorGroup.LczB02 => DoorType.ElevatorLczB,

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
    }
}