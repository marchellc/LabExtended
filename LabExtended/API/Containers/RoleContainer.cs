using LabExtended.API.Custom.Roles;
using LabExtended.API.CustomTeams;

using LabExtended.Extensions;

using LabExtended.Utilities;
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

namespace LabExtended.API.Containers;

/// <summary>
/// Provides properties and methods for easier role management.
/// </summary>
public class RoleContainer
{
    internal RoleContainer(ExPlayer player, PlayerRoleManager manager)
    {
        Player = player;
        Manager = manager;
    }

    internal object? customRoleData;

    /// <summary>
    /// Gets the player this wrapper belongs to.
    /// </summary>
    public ExPlayer Player { get; }

    /// <summary>
    /// Gets the player's role manager component.
    /// </summary>
    public PlayerRoleManager Manager { get; }

    /// <summary>
    /// Gets the player's role faking list.
    /// </summary>
    public FakeValue<RoleTypeId> FakedList { get; } = new();

    /// <summary>
    /// Gets the player's active role instance.
    /// </summary>
    public PlayerRoleBase Role => Manager.CurrentRole;
    
    /// <summary>
    /// Gets the type of the player's active role instance.
    /// </summary>
    public Type Class => Role.GetType();

    /// <summary>
    /// Gets the amount of time the player's role has been active for.
    /// </summary>
    public TimeSpan ActiveTime => Role._activeTime.Elapsed;
    
    /// <summary>
    /// Gets the time at which the player spawned.
    /// </summary>
    public DateTime ActiveSince => DateTime.Now - Role._activeTime.Elapsed;

    /// <summary>
    /// Gets the color of the player's role.
    /// </summary>
    public Color Color => Role.RoleColor;

    /// <summary>
    /// Gets the role's spawn reason.
    /// </summary>
    public RoleChangeReason ChangeReason => Role.ServerSpawnReason;
    
    /// <summary>
    /// Gets the role's spawn flags.
    /// </summary>
    public RoleSpawnFlags SpawnFlags => Role.ServerSpawnFlags;

    /// <summary>
    /// Gets the role assigned to the player by the late join function (will be <see cref="RoleTypeId.None"/> if the player joined after the function's timer).
    /// </summary>
    public RoleTypeId LateJoinRole { get; internal set; } = RoleTypeId.None;

    /// <summary>
    /// Gets the player's first spawned role (when the round started, will be <see cref="RoleTypeId.None"/> if the player joined afterwards).
    /// </summary>
    public RoleTypeId RoundStartRole { get; internal set; } = RoleTypeId.None;

    /// <summary>
    /// Gets or sets the player's current role type.
    /// </summary>
    public RoleTypeId Type
    {
        get => Role.RoleTypeId;
        set => Manager.ServerSetRole(value, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.All);
    }

    /// <summary>
    /// Gets or sets the player's current emotion.
    /// </summary>
    public EmotionPresetType Emotion
    {
        get => EmotionSync.GetEmotionPreset(Manager._hub);
        set => EmotionSync.ServerSetEmotionPreset(Manager._hub, value);
    }

    /// <summary>
    /// Gets or sets the player's current wearable elements.
    /// </summary>
    public WearableElements WearableElements
    {
        get => WearableSync.GetFlags(Manager._hub);
        set => WearableSync.OverrideWearables(Manager._hub, value);
    }

    /// <summary>
    /// Gets the team of the player's role.
    /// </summary>
    public Team Team => Role.Team;

    /// <summary>
    /// Gets the faction of the player's role.
    /// </summary>
    public Faction Faction => Team.GetFaction();

    /// <summary>
    /// Gets the name of the player's role.
    /// </summary>
    public string Name => string.IsNullOrWhiteSpace(Role.RoleName) 
        ? Role.ToString().SpaceByUpperCase() 
        : Role.RoleName;

    /// <summary>
    /// Gets the name of the player's role, prefixed with a color tag with the role's color and postfixed with a color closing tag.
    /// </summary>
    public string ColoredName => $"<color={Color.ToHex()}>{Name}</color>";

    /// <summary>
    /// Whether or not the player is an SCP (including SCP-049-2).
    /// </summary>
    public bool IsScp => Team == Team.SCPs;
    
    /// <summary>
    /// Whether or not the player is an SCP (excluding SCP-049-2).
    /// </summary>
    public bool IsScpButNotZombie => Team == Team.SCPs && Type != RoleTypeId.Scp0492;

    /// <summary>
    /// Whether or not the player is a member of a Nine-Tailed Fox squad.
    /// </summary>
    public bool IsNtf => Team == Team.FoundationForces && Type != RoleTypeId.FacilityGuard;
    
    /// <summary>
    /// Whether or not the player is a member of a Nine-Tailed Fox squad (including facility guards).
    /// </summary>
    public bool IsNtfOrFacilityGuard => Team == Team.FoundationForces;

    /// <summary>
    /// Whether or not the player is a member of a Chaos Insurgency squad.
    /// </summary>
    public bool IsChaos => Team == Team.ChaosInsurgency;
    
    /// <summary>
    /// Whether or not the player is a Class-D Personnel.
    /// </summary>
    public bool IsClassD => Team == Team.ClassD;
    
    /// <summary>
    /// Whether or not the player is a member of a Chaos Insurgency squad (including Class-D personnel).
    /// </summary>
    public bool IsChaosOrClassD => Team == Team.ClassD || Team == Team.ChaosInsurgency;

    /// <summary>
    /// Whether or not the player is dead.
    /// </summary>
    public bool IsDead => Team == Team.Dead;
    
    /// <summary>
    /// Whether or not the player is alive.
    /// </summary>
    public bool IsAlive => Team != Team.Dead;

    /// <summary>
    /// Whether or not the player is a facility guard.
    /// </summary>
    public bool IsFacilityGuard => Type is RoleTypeId.FacilityGuard;
    
    /// <summary>
    /// Whether or not the player is a scientist.
    /// </summary>
    public bool IsScientist => Type is RoleTypeId.Scientist;
    
    /// <summary>
    /// Whether or not the player is in Overwatch.
    /// </summary>
    public bool IsOverwatch => Type is RoleTypeId.Overwatch;
    
    /// <summary>
    /// Whether or not the player is spectating.
    /// </summary>
    public bool IsSpectator => Type is RoleTypeId.Spectator;
    
    /// <summary>
    /// Whether or not the player is playing as the Tutorial role.
    /// </summary>
    public bool IsTutorial => Type is RoleTypeId.Tutorial;
    
    /// <summary>
    /// Whether or not the player has a role assigned (true while waiting for players).
    /// </summary>
    public bool IsNone => Type is RoleTypeId.None;

    /// <summary>
    /// Whether or not the player is wearing the SCP-268 hat.
    /// </summary>
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

    /// <summary>
    /// Whether or not the player is wearing the SCP-1344 goggles.
    /// </summary>
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

    /// <summary>
    /// Gets the player's active custom team (if part of any, otherwise null).
    /// </summary>
    public CustomTeamInstance? CustomTeam { get; internal set; }
    
    /// <summary>
    /// Gets the player's active custom role.
    /// </summary>
    public CustomRole? CustomRole { get; internal set; }

    /// <summary>
    /// Gets custom data associated with custom the role.
    /// </summary>
    public object? CustomRoleData => customRoleData;

    /// <summary>
    /// Gets the player's current role cast to <see cref="IFpcRole"/>.
    /// </summary>
    public IFpcRole? FpcRole => Role as IFpcRole;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="IVoiceRole"/>.
    /// </summary>
    public IVoiceRole? VoiceRole => Role as IVoiceRole;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="ISubroutinedRole"/>.
    /// </summary>
    public ISubroutinedRole? SubroutinedRole => Role as ISubroutinedRole;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="IHumeShieldedRole"/>.
    /// </summary>
    public IHumeShieldedRole? HumeShieldedRole => Role as IHumeShieldedRole;
    
    /// <summary>
    /// Gets the player's <see cref="VoiceModuleBase"/> component if the player is playing as a role that inherits from <see cref="IVoiceRole"/>.
    /// </summary>
    public VoiceModuleBase? VoiceModule => VoiceRole?.VoiceModule;
    
    /// <summary>
    /// Gets the player's <see cref="HumeShieldModuleBase"/> component if the player is playing as a role that inherits from <see cref="IHumeShieldedRole"/>.
    /// </summary>
    public HumeShieldModuleBase? HumeShieldManager => HumeShieldedRole?.HumeShieldModule;
    
    /// <summary>
    /// Gets the player's <see cref="SubroutineManagerModule"/> component if the player is playing as a role that inherits from <see cref="ISubroutinedRole"/>.
    /// </summary>
    public SubroutineManagerModule? SubroutineManager => SubroutinedRole?.SubroutineModule;

    #region Fpc Stuff
    /// <summary>
    /// Gets the player's <see cref="FirstPersonMovementModule"/> component.
    /// </summary>
    public FirstPersonMovementModule? MovementModule => FpcRole?.FpcModule;

    /// <summary>
    /// Gets the player's <see cref="FpcGravityController"/> component.
    /// </summary>
    public FpcGravityController? GravityController => Motor?.GravityController;
    
    /// <summary>
    /// Gets the player's <see cref="FpcStateProcessor"/> component.
    /// </summary>
    public FpcStateProcessor? StateProcessor => MovementModule?.StateProcessor;
    
    /// <summary>
    /// Gets the player's <see cref="FpcMouseLook"/> component.
    /// </summary>
    public FpcMouseLook? MouseLook => MovementModule?.MouseLook;
    
    /// <summary>
    /// Gets the player's <see cref="FpcNoclip"/> component.
    /// </summary>
    public FpcNoclip? NoClip => MovementModule?.Noclip;
    
    /// <summary>
    /// Gets the player's <see cref="FpcMotor"/> component.
    /// </summary>
    public FpcMotor? Motor => MovementModule?.Motor;
    #endregion

    #region Other Roles
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.NoneRole"/>.
    /// </summary>
    public NoneRole? NoneRole => Role as NoneRole;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.HumanRole"/>.
    /// </summary>
    public HumanRole? HumanRole => Role as HumanRole;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.Spectating.SpectatorRole"/>.
    /// </summary>
    public SpectatorRole? SpectatorRole => Role as SpectatorRole;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.Spectating.OverwatchRole"/>.
    /// </summary>
    public OverwatchRole? OverwatchRole => Role as OverwatchRole;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.PlayableScps.FpcStandardScp"/>.
    /// </summary>
    public FpcStandardScp? ScpRole => Role as FpcStandardScp;
    #endregion

    #region Scp Roles
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.PlayableScps.Scp049.Scp049Role"/>.
    /// </summary>
    public Scp049Role? Scp049 => Role as Scp049Role;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.PlayableScps.Scp079.Scp079Role"/>.
    /// </summary>
    public Scp079Role? Scp079 => Role as Scp079Role;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.PlayableScps.Scp096.Scp096Role"/>.
    /// </summary>
    public Scp096Role? Scp096 => Role as Scp096Role;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.PlayableScps.Scp106.Scp106Role"/>.
    /// </summary>
    public Scp106Role? Scp106 => Role as Scp106Role;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.PlayableScps.Scp173.Scp173Role"/>.
    /// </summary>
    public Scp173Role? Scp173 => Role as Scp173Role;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.PlayableScps.Scp939.Scp939Role"/>.
    /// </summary>
    public Scp939Role? Scp939 => Role as Scp939Role;
    
    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.PlayableScps.Scp3114.Scp3114Role"/>.
    /// </summary>
    public Scp3114Role? Scp3114 => Role as Scp3114Role;

    /// <summary>
    /// Gets the player's current role cast to <see cref="PlayerRoles.PlayableScps.Scp049.Zombies.ZombieRole"/>.
    /// </summary>
    public ZombieRole? ZombieRole => Role as ZombieRole;
    #endregion

    /// <summary>
    /// Gets the role type that the current player appears to have from the perspective of the specified target player.
    /// </summary>
    /// <param name="target">The player whose perspective is used to determine the appearance. Cannot be null.</param>
    /// <param name="allowOverwatchSpoof">Indicates whether Overwatch role spoofing should be considered when determining the appearance. If <see
    /// langword="true"/>, the returned role may be spoofed for Overwatch purposes; otherwise, the actual role is used.</param>
    /// <returns>The role type identifier representing how the current player appears to the target player.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> is null or does not have a valid reference hub.</exception>
    public RoleTypeId GetAppearance(ExPlayer target, bool allowOverwatchSpoof = true)
    {
        if (target?.ReferenceHub == null)
            throw new ArgumentNullException(nameof(target));

        var appearance = allowOverwatchSpoof
            ? RoleSync.GetSpoofedRoleToSend(Player, target)
            : RoleSync.GetRoleToSend(Player, target);

        if (CustomRole != null)
            appearance = CustomRole.GetAppearance(target, appearance);

        return appearance;
    }

    /// <summary>
    /// Sets the player's current role.
    /// </summary>
    /// <param name="newRole">The new role to set.</param>
    /// <param name="changeReason">The reason for the role change.</param>
    /// <param name="spawnFlags">The role spawn flags.</param>
    public void Set(RoleTypeId newRole, RoleChangeReason changeReason = RoleChangeReason.RemoteAdmin,
        RoleSpawnFlags spawnFlags = RoleSpawnFlags.All)
        => Manager.ServerSetRole(newRole, changeReason, spawnFlags);

    /// <summary>
    /// Whether or not the player's role is a specific one.
    /// </summary>
    /// <param name="type">The type of the role to check.</param>
    /// <returns>true if the player's current role is <paramref name="type"/></returns>
    public bool Is(RoleTypeId type)
        => Type == type;

    /// <summary>
    /// Determines whether the specified identifier matches the custom role identifier.
    /// </summary>
    /// <param name="id">The identifier to compare with the custom role. Cannot be null.</param>
    /// <returns>true if the custom role exists and its identifier equals the specified id; otherwise, false.</returns>
    public bool IsCustom(string id)
        => id != null && CustomRole != null && CustomRole.Id == id;

    /// <summary>
    /// Determines whether the current custom role is of the specified type.
    /// </summary>
    /// <param name="type">The type to compare with the current custom role. Cannot be null.</param>
    /// <returns>true if the current custom role is not null and its type matches the specified type; otherwise, false.</returns>
    public bool IsCustom(Type type)
        => type != null && CustomRole != null && CustomRole.GetType() == type;

    /// <summary>
    /// Determines whether the current custom role is of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to compare with the current custom role.</typeparam>
    /// <returns>true if the custom role is of type T; otherwise, false.</returns>
    public bool IsCustom<T>()
        => CustomRole is T;

    /// <summary>
    /// Retrieves the custom role data as the specified type, or returns the provided default value if the data is not
    /// available or cannot be cast.
    /// </summary>
    /// <typeparam name="T">The type to which the custom role data should be cast.</typeparam>
    /// <param name="defaultValue">The value to return if the custom role data is not present or cannot be cast to type <typeparamref name="T"/>.
    /// The default is <c>default</c> for the type.</param>
    /// <returns>The custom role data cast to type <typeparamref name="T"/>, or <paramref name="defaultValue"/> if the data is
    /// not available or cannot be cast.</returns>
    public T GetDataOrDefault<T>(T? defaultValue = default)
    {
        if (customRoleData is not T castData)
            return defaultValue!;

        return castData;
    }

    /// <summary>
    /// Attempts to retrieve the stored custom role data as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the custom role data should be cast.</typeparam>
    /// <param name="data">When this method returns, contains the custom role data cast to type <typeparamref name="T"/> if the cast
    /// succeeds; otherwise, the default value for type <typeparamref name="T"/>.</param>
    /// <returns>true if the custom role data can be cast to type <typeparamref name="T"/>; otherwise, false.</returns>
    public bool TryGetData<T>(out T data)
    {
        if (customRoleData is not T castData)
        {
            data = default!;
            return false;
        }

        data = castData;
        return true;
    }

    /// <summary>
    /// Checks if the player's current role inherits a type.
    /// </summary>
    /// <typeparam name="T">The type to inherit.</typeparam>
    /// <returns>true if the current role inherits <typeparamref name="T"/></returns>
    public bool Is<T>()
        => Role is T;

    /// <summary>
    /// Checks if the player's current role inherits a type.
    /// </summary>
    /// <param name="role">The player's current role cast to T.</param>
    /// <typeparam name="T">The type to inherit.</typeparam>
    /// <returns>true if the current role inherits <typeparamref name="T"/></returns>
    public bool Is<T>(out T role)
    {
        if (Role is not T castRole)
        {
            role = default!;
            return false;
        }

        role = castRole;
        return true;
    }

    /// <summary>
    /// Invokes a delegate if the player's current role inherits a specific type.
    /// </summary>
    /// <param name="action">The delegate to invoke.</param>
    /// <typeparam name="T">The type to inherit.</typeparam>
    /// <returns>true if the delegate was invoked</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool IfRole<T>(Action<T> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        if (Role is T tRole)
        {
            action(tRole);
            return true;
        }

        return false;
    }

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
        => Name;

    /// <summary>
    /// Converts a role container to it's role base.
    /// </summary>
    public static implicit operator PlayerRoleBase?(RoleContainer container)
        => container?.Role;

    /// <summary>
    /// Converts a role container to it's role type.
    /// </summary>
    public static implicit operator RoleTypeId(RoleContainer container)
        => container?.Type ?? RoleTypeId.None;

    /// <summary>
    /// Checks if a container's role is not null.
    /// </summary>
    public static implicit operator bool(RoleContainer container)
        => container?.Role != null;

    /// <summary>
    /// Converts a role container to it's role name.
    /// </summary>
    public static implicit operator string(RoleContainer container)
        => container?.Name ?? string.Empty;
}