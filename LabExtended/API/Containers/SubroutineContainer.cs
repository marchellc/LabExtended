using PlayerRoles.Subroutines;

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
using PlayerRoles.PlayableScps.Scp939.Mimicry;

namespace LabExtended.API.Containers
{
    public class SubroutineContainer
    {
        public RoleContainer Role { get; }

        public SubroutineContainer(RoleContainer role)
            => Role = role;

        public SubroutineManagerModule SubroutineManager => Role.SubroutineManager;

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

        public EnvironmentalMimicry EnvironmentalMimicry => GetRoutine<EnvironmentalMimicry>();

        public MimicPointController MimicPointController => GetRoutine<MimicPointController>();
        public MimicryTransmitter MimicryTransmitter => GetRoutine<MimicryTransmitter>();
        public MimicryRecorder MimicryRecorder => GetRoutine<MimicryRecorder>();
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

            return SubroutineManager.TryGetSubroutine(out routine);
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