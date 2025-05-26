using LabExtended.Extensions;
using LabExtended.Utilities.Values;

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

using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers.Wearables;
using UnityEngine;

namespace LabExtended.API.Containers
{
    public class RoleContainer
    {
        internal RoleContainer(PlayerRoleManager manager)
            => Manager = manager;

        public PlayerRoleManager Manager { get; }

        public FakeValue<RoleTypeId> FakedList { get; } = new();
        
        public PlayerRoleBase Role => Manager.CurrentRole;
        public Type Class => Role.GetType();

        public TimeSpan ActiveTime => Role._activeTime.Elapsed;
        public DateTime ActiveSince => DateTime.Now - Role._activeTime.Elapsed;

        public Color Color => Role.RoleColor;

        public RoleChangeReason ChangeReason => Role.ServerSpawnReason;
        public RoleSpawnFlags SpawnFlags => Role.ServerSpawnFlags;

        /// <summary>
        /// Gets the player's first spawned role (when the round started, will be None if the player joined afterwards).
        /// </summary>
        public RoleTypeId RoundStartRole { get; internal set; } = RoleTypeId.None;

        public RoleTypeId Type
        {
            get => Role.RoleTypeId;
            set => Manager.ServerSetRole(value, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.All);
        }

        public EmotionPresetType Emotion
        {
            get => EmotionSync.GetEmotionPreset(Manager._hub);
            set => EmotionSync.ServerSetEmotionPreset(Manager._hub, value);
        }

        public WearableElements WearableElements
        {
            get => WearableSync.GetFlags(Manager._hub);
            set => WearableSync.OverrideWearables(Manager._hub, value);
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

        public bool IsWearingScp268
        {
            get => WearableElements.Any(WearableElements.Scp268Hat);
            set
            {
                if (value)
                {
                    if (IsWearingScp268)
                        return;

                    WearableElements |= WearableElements.Scp268Hat;
                    return;
                }

                if (!IsWearingScp268)
                    return;

                WearableElements &= ~WearableElements.Scp268Hat;
            }
        }
        
        public bool IsWearingScp1344
        {
            get => WearableElements.Any(WearableElements.Scp1344Goggles);
            set
            {
                if (value)
                {
                    if (IsWearingScp268)
                        return;

                    WearableElements |= WearableElements.Scp1344Goggles;
                    return;
                }

                if (!IsWearingScp268)
                    return;

                WearableElements &= ~WearableElements.Scp1344Goggles;
            }
        }

        public IFpcRole FpcRole => Role as IFpcRole;
        public IVoiceRole VoiceRole => Role as IVoiceRole;
        public ISubroutinedRole SubroutinedRole => Role as ISubroutinedRole;
        public IHumeShieldedRole HumeShieldedRole => Role as IHumeShieldedRole;

        public VoiceModuleBase VoiceModule => VoiceRole?.VoiceModule;
        public HumeShieldModuleBase HumeShieldManager => HumeShieldedRole?.HumeShieldModule;
        public SubroutineManagerModule SubroutineManager => SubroutinedRole?.SubroutineModule;

        #region Fpc Stuff
        public FirstPersonMovementModule MovementModule => FpcRole?.FpcModule;

        public FpcGravityController GravityController => Motor?.GravityController;
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

        public void Set(RoleTypeId newRole, RoleChangeReason changeReason = RoleChangeReason.RemoteAdmin, RoleSpawnFlags spawnFlags = RoleSpawnFlags.All)
            => Manager.ServerSetRole(newRole, changeReason, spawnFlags);

        public bool Is(RoleTypeId type)
            => Type == type;

        public bool Is<T>()
            => Role is T;

        public bool Is<T>(out T role)
        {
            if (Role is null || Role is not T castRole)
            {
                role = default;
                return false;
            }

            role = castRole;
            return true;
        }

        public bool IfRole<T>(Action<T> action)
        {
            if (Role is T tRole)
            {
                action(tRole);
                return true;
            }

            return false;
        }

        public override string ToString()
            => Name;

        public static implicit operator PlayerRoleBase(RoleContainer container)
            => container?.Role;

        public static implicit operator RoleTypeId(RoleContainer container)
            => container?.Type ?? RoleTypeId.None;

        public static implicit operator bool(RoleContainer container)
            => container?.Role != null;

        public static implicit operator string(RoleContainer container)
            => container?.Name ?? string.Empty;
    }
}