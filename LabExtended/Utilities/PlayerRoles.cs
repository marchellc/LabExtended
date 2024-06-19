using PlayerRoles;
using PlayerRoles.Voice;
using PlayerRoles.Spectating;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps;
using PlayerRoles.Subroutines;

using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.PlayableScps.Scp173;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using PlayerRoles.PlayableScps.Scp079.Pinging;
using PlayerRoles.PlayableScps.Scp079.Rewards;

using UnityEngine;

namespace LabExtended.Utilities
{
    public class PlayerRoles
    {
        internal PlayerRoles(PlayerRoleManager manager)
            => Manager = manager;

        public PlayerRoleManager Manager { get; }

        public PlayerRoleBase Role => Manager.CurrentRole;
        public Type Class => Role.GetType();

        public TimeSpan ActiveTime => Role._activeTime.Elapsed;
        public DateTime ActiveSince => DateTime.Now - Role._activeTime.Elapsed;

        public Color RoleColor => Role.RoleColor;

        public RoleChangeReason ChangeReason => Role.ServerSpawnReason;
        public RoleSpawnFlags SpawnFlags => Role.ServerSpawnFlags;

        public RoleTypeId Type
        {
            get => Role.RoleTypeId;
            set => Manager.ServerSetRole(value, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.All);
        }

        public Team Team => Type.GetTeam();
        public Faction Faction => Team.GetFaction();

        public string Name => string.IsNullOrWhiteSpace(Role.RoleName) ? Role.ToString() : Role.RoleName;
        public string ColoredName => Role.GetColoredName();

        public bool IsScp => Team == Team.SCPs;
        public bool IsScpButNotZombie => Team == Team.SCPs && Type != RoleTypeId.Scp0492;

        public bool IsNtf => Team == Team.FoundationForces && Type != RoleTypeId.FacilityGuard;
        public bool IsNtfOrFacilityGuard => Team == Team.FoundationForces;

        public bool IsChaos => Team == Team.ChaosInsurgency;
        public bool IsClassD => Team == Team.ClassD;
        public bool IsChaosOrClassD => Team == Team.ClassD || Team == Team.ChaosInsurgency;

        public bool IsDead => Team == Team.Dead;
        public bool IsAlive => Team != Team.Dead;

        public bool IsFacilityGuard => Type is RoleTypeId.FacilityGuard;
        public bool IsScientist => Type is RoleTypeId.Scientist;
        public bool IsOverwatch => Type is RoleTypeId.Overwatch;
        public bool IsSpectator => Type is RoleTypeId.Spectator;
        public bool IsTutorial => Type is RoleTypeId.Tutorial;
        public bool IsNone => Type is RoleTypeId.None;

        public IFpcRole FpcRole => Role as IFpcRole;
        public IVoiceRole VoiceRole => Role as IVoiceRole;
        public ISubroutinedRole SubroutinedRole => Role as ISubroutinedRole;
        public IHumeShieldedRole HumeShieldedRole => Role as IHumeShieldedRole;

        public VoiceModuleBase VoiceModule => VoiceRole?.VoiceModule;
        public HumeShieldModuleBase HumeShieldManager => HumeShieldedRole?.HumeShieldModule;
        public SubroutineManagerModule SubroutineManager => SubroutinedRole?.SubroutineModule;

        #region Fpc Stuff
        public FirstPersonMovementModule MovementModule => FpcRole?.FpcModule;

        public FpcStateProcessor StateProcessor => MovementModule?.StateProcessor;
        public FpcMouseLook MouseLook => MovementModule?.MouseLook;
        public FpcNoclip NoClip => MovementModule?.Noclip;
        public FpcMotor Motor => MovementModule?.Motor;
        #endregion

        #region Other Roles
        public NoneRole NoneRole => Role as NoneRole;
        public HumanRole HumanRole => Role as HumanRole;
        public SpectatorRole SpectatorRole => Role as SpectatorRole;
        public OverwatchRole OverwatchRole => Role as OverwatchRole;
        public FpcStandardScp ScpRole => Role as FpcStandardScp;
        #endregion

        #region Scp Roles
        public Scp049Role Scp049 => Role as Scp049Role;
        public Scp079Role Scp079 => Role as Scp079Role;
        public Scp096Role Scp096 => Role as Scp096Role;
        public Scp106Role Scp106 => Role as Scp106Role;
        public Scp173Role Scp173 => Role as Scp173Role;
        public Scp939Role Scp939 => Role as Scp939Role;
        public Scp3114Role Scp3114 => Role as Scp3114Role;

        public ZombieRole ZombieRole => Role as ZombieRole;
        #endregion

        #region Scp049 Abilities
        public Scp049CallAbility Scp049CallAbility => GetRoutine<Scp049CallAbility>();
        public Scp049SenseAbility Scp049SenseAbility => GetRoutine<Scp049SenseAbility>();
        public Scp049ResurrectAbility Scp049ResurrectAbility => GetRoutine<Scp049ResurrectAbility>();
        #endregion

        #region Zombie Abilities
        public ZombieBloodlustAbility ZombieBloodlustAbility => GetRoutine<ZombieBloodlustAbility>();
        public ZombieConsumeAbility ZombieConsumeAbility => GetRoutine<ZombieConsumeAbility>();
        public ZombieAttackAbility ZombieAttackAbility => GetRoutine<ZombieAttackAbility>();
        public ZombieAudioPlayer ZombieAudioPlayer => GetRoutine<ZombieAudioPlayer>();
        #endregion

        #region Scp079 Abilities
        public Scp079BlackoutRoomAbility Scp079BlackoutRoomAbility => GetRoutine<Scp079BlackoutRoomAbility>();
        public Scp079BlackoutZoneAbility Scp079BlackoutZoneAbility => GetRoutine<Scp079BlackoutZoneAbility>();
        public Scp079ElevatorStateChanger Scp079ElevatorStateChanger => GetRoutine<Scp079ElevatorStateChanger>();
        public Scp079LockdownRoomAbility Scp079LockdownRoomAbility => GetRoutine<Scp079LockdownRoomAbility>();
        public Scp079LostSignalHandler Scp079LostSignalHandler => GetRoutine<Scp079LostSignalHandler>();
        public Scp079DoorLockReleaser Scp079DoorLockReleaser => GetRoutine<Scp079DoorLockReleaser>();
        public Scp079DoorStateChanger Scp079DoorStateChanger => GetRoutine<Scp079DoorStateChanger>();
        public Scp079DoorLockChanger Scp079DoorLockChanger => GetRoutine<Scp079DoorLockChanger>();
        public Scp079SpeakerAbility Scp079SpeakerAbility => GetRoutine<Scp079SpeakerAbility>();
        public Scp079RewardManager Scp079RewardManager => GetRoutine<Scp079RewardManager>();
        public Scp079TeslaAbility Scp079TeslaAbility => GetRoutine<Scp079TeslaAbility>();
        public Scp079PingAbility Scp079PingAbility => GetRoutine<Scp079PingAbility>();
        public Scp079AuxManager Scp079AuxManager => GetRoutine<Scp079AuxManager>();


        public Scp079CameraRotationSync Scp079CameraRotationSync => GetRoutine<Scp079CameraRotationSync>();
        public Scp079CurrentCameraSync Scp079CurrentCameraSync => GetRoutine<Scp079CurrentCameraSync>();
        #endregion

        #region Scp096 Abilities
        public Scp096TryNotToCryAbility Scp096TryNotToCryAbility => GetRoutine<Scp096TryNotToCryAbility>();
        public Scp096RageCycleAbility Scp096RageCycleAbility => GetRoutine<Scp096RageCycleAbility>();
        public Scp096StateController Scp096StateController => GetRoutine<Scp096StateController>();
        public Scp096PrygateAbility Scp096PrygateAbility => GetRoutine<Scp096PrygateAbility>();
        public Scp096TargetsTracker Scp096TargetsTracker => GetRoutine<Scp096TargetsTracker>();
        public Scp096AttackAbility Scp096AttackAbility => GetRoutine<Scp096AttackAbility>();
        public Scp096ChargeAbility Scp096ChargeAbility => GetRoutine<Scp096ChargeAbility>();
        public Scp096AudioPlayer Scp096AudioPlayer => GetRoutine<Scp096AudioPlayer>();
        public Scp096RageManager Scp096RageManager => GetRoutine<Scp096RageManager>();
        #endregion

        #region Scp106 Abilities
        public Scp106HuntersAtlasAbility Scp106HuntersAtlasAbility => GetRoutine<Scp106HuntersAtlasAbility>();
        public Scp106SinkholeController Scp106SinkholeController => GetRoutine<Scp106SinkholeController>();
        public Scp106StalkAbility Scp106StalkAbility => GetRoutine<Scp106StalkAbility>();
        #endregion

        #region Scp173 Abilities
        public Scp173BreakneckSpeedsAbility Scp173BreakneckSpeedsAbility => GetRoutine<Scp173BreakneckSpeedsAbility>();
        public Scp173ObserversTracker Scp173ObserversTracker => GetRoutine<Scp173ObserversTracker>();
        public Scp173TeleportAbility Scp173TeleportAbility => GetRoutine<Scp173TeleportAbility>();
        public Scp173TantrumAbility Scp173TantrumAbility => GetRoutine<Scp173TantrumAbility>();
        public Scp173SnapAbility Scp173SnapAbility => GetRoutine<Scp173SnapAbility>();
        public Scp173AudioPlayer Scp173AudioPlayer => GetRoutine<Scp173AudioPlayer>();
        #endregion

        #region Scp939 Abilities
        public Scp939AmnesticCloudAbility Scp939AmnesticCloudAbility => GetRoutine<Scp939AmnesticCloudAbility>();
        public Scp939BreathController Scp939BreathController => GetRoutine<Scp939BreathController>();
        public Scp939FocusAbility Scp939FocusAbility => GetRoutine<Scp939FocusAbility>();
        public Scp939LungeAbility Scp939LungeAbility => GetRoutine<Scp939LungeAbility>();
        public Scp939ClawAbility Scp939ClawAbility => GetRoutine<Scp939ClawAbility>();
        #endregion

        #region Scp3114 Abilities
        public Scp3114RagdollToBonesConverter Scp3114RagdollToBonesConverter => GetRoutine<Scp3114RagdollToBonesConverter>();
        public Scp3114Disguise Scp3114DisguiseAbility => GetRoutine<Scp3114Disguise>();
        public Scp3114Strangle Scp3114StrangleAbility => GetRoutine<Scp3114Strangle>();
        public Scp3114VoiceLines Scp3114VoiceLines => GetRoutine<Scp3114VoiceLines>();
        public Scp3114Reveal Scp3114RevealAbility => GetRoutine<Scp3114Reveal>();
        public Scp3114Identity Scp3114Identity => GetRoutine<Scp3114Identity>();
        public Scp3114Dance Scp3114DanceAbility => GetRoutine<Scp3114Dance>();
        public Scp3114Slap Scp3114SlapAbility => GetRoutine<Scp3114Slap>();
        #endregion

        public IEnumerable<SubroutineBase> AllSubroutines => SubroutineManager?.AllSubroutines ?? Array.Empty<SubroutineBase>();

        public void Set(RoleTypeId newRole, RoleChangeReason changeReason = RoleChangeReason.RemoteAdmin, RoleSpawnFlags spawnFlags = RoleSpawnFlags.All)
            => Manager.ServerSetRole(newRole, changeReason, spawnFlags);

        public bool Is<T>() where T : PlayerRoleBase
            => Role is T;

        public bool Is<T>(out T role) where T : PlayerRoleBase
            => (role = Role as T) != null;

        public bool IfRole<T>(Action<T> action) where T : PlayerRoleBase
        {
            if (Role is T tRole)
            {
                action(tRole);
                return true;
            }

            return false;
        }

        public T GetRoutine<T>() where T : SubroutineBase
        {
            if (SubroutineManager is null)
                return default;

            if (SubroutineManager.TryGetSubroutine<T>(out var subroutine))
                return subroutine;

            return default;
        }

        public bool TryGetRoutine<T>(out T routine) where T : SubroutineBase
        {
            if (SubroutineManager is null)
            {
                routine = default;
                return false;
            }

            return SubroutineManager.TryGetSubroutine<T>(out routine);
        }

        public bool IfRoutine<T>(Action<T> action) where T : SubroutineBase
        {
            if (SubroutineManager is null)
                return false;

            if (!SubroutineManager.TryGetSubroutine<T>(out var subroutine))
                return false;

            action(subroutine);
            return true;
        }
    }
}