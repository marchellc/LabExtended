using LabExtended.Core;
using LabExtended.Extensions;

using YamlDotNet.Serialization;

namespace LabExtended.API.Containers
{
    /// <summary>
    /// A class that holds custom player switches.
    /// </summary>
    public class SwitchContainer
    {
        /// <summary>
        /// Default switches for real players.
        /// </summary>
        public static SwitchContainer DefaultPlayerSwitches { get; } = new();

        /// <summary>
        /// Default switches for dummy players.
        /// </summary>
        public static SwitchContainer DefaultNpcSwitches { get; } = new()
        {
            IsVisibleInRemoteAdmin = true,
            IsVisibleInSpectatorList = false,
            
            CanBeRespawned = false,
            CanBlockRoundEnd = false,
            CanTriggerScp096 = false,
            CanBlockScp173 = false,
            CanCountAs079ExpTarget = false,
            CanBeResurrectedBy049 = false,
            CanBePocketDimensionItemTarget = false,
            
            PreventsRecontaining079 = false,
            
            ShouldReceivePositions = false,
        };

        /// <summary>
        /// Switches for real players from the config file.
        /// </summary>
        public static SwitchContainer PlayerConfig => ApiLoader.ApiConfig?.OtherSection?.PlayerToggles ?? DefaultPlayerSwitches;
        
        /// <summary>
        /// Switches for dummy players from the config file.
        /// </summary>
        public static SwitchContainer NpcConfig => ApiLoader.ApiConfig?.OtherSection?.NpcToggles ?? DefaultNpcSwitches;

        /// <summary>
        /// Gets a new instance of real player switches.
        /// </summary>
        /// <param name="useConfigValues">Whether or not to copy values from config.</param>
        /// <returns>The created instance.</returns>
        public static SwitchContainer GetNewPlayerToggles(bool useConfigValues = true)
        {
            var toggles = new SwitchContainer();
            
            toggles.ResetToPlayer(useConfigValues);
            return toggles;
        }

        /// <summary>
        /// Gets a new instance of dummy player switches.
        /// </summary>
        /// <param name="useConfigValues">Whether or not to copy values from config.</param>
        /// <returns>The created instance.</returns>
        public static SwitchContainer GetNewNpcToggles(bool useConfigValues = true)
        {
            var toggles = new SwitchContainer();
            
            toggles.ResetToNpc(useConfigValues);
            return toggles;
        }

        /// <summary>
        /// Gets a list of ignored effect types.
        /// </summary>
        [YamlIgnore]
        public HashSet<Type> IgnoredEffects { get; } = new();

        #region Visibility Switches
        /// <summary>
        /// Gets or sets a value indicating whether or not this player is visible in the Remote Admin player list.
        /// </summary>
        public bool IsVisibleInRemoteAdmin { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player is visible in the Spectator List.
        /// </summary>
        public bool IsVisibleInSpectatorList { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating if this player is always visible to SCP-939.
        /// </summary>
        public bool IsVisibleToScp939 { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can see every other player when playing as SCP-939.
        /// </summary>
        public bool CanSeeEveryoneAs939 { get; set; } = false;
        #endregion

        #region Round Switches
        /// <summary>
        /// Gets or sets a value indicating whether or not this player should count in the next respawn wave.
        /// </summary>
        public bool CanBeRespawned { get; set; } = true; // BROKEN

        /// <summary>
        /// Gets or sets a value indicating whether or not this player prevents the round from ending.
        /// </summary>
        public bool CanBlockRoundEnd { get; set; } = true;
        #endregion

        #region Scp Switches
        /// <summary>
        /// Gets or sets a value indicating whether or not this player can become a target of SCP-049's Sense ability.
        /// </summary>
        public bool CanBeScp049Target { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can become a target of a random item drop from the Pocket Dimension.
        /// </summary>
        public bool CanBePocketDimensionItemTarget { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can trigger SCP-096.
        /// </summary>
        public bool CanTriggerScp096 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can block SCP-173's movement.
        /// </summary>
        public bool CanBlockScp173 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can be teleported to the Pocket Dimension by SCP-106.
        /// </summary>
        public bool CanBeCapturedBy106 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can be strangled by SCP-3114.
        /// </summary>
        public bool CanBeStrangledBy3114 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can hear SCP-939's Amnestic Cloud.
        /// </summary>
        public bool CanHearAmnesticCloudSpawn { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can become a target for SCP-079's EXP rewards.
        /// </summary>
        public bool CanCountAs079ExpTarget { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can be resurrected by SCP-049.
        /// </summary>
        public bool CanBeResurrectedBy049 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player's ragdoll can be consumed by Zombies.
        /// </summary>
        public bool CanBeConsumedByZombies { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not SCP-049 can use it's Sense ability.
        /// </summary>
        public bool CanUseSenseAs049 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not SCP-049 can use it's Resurrect ability.
        /// </summary>
        public bool CanUseResurrectAs049 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can strangle other players when playing as SCP-3114.
        /// </summary>
        public bool CanStrangleAs3114 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can capture other players when playing as SCP-106.
        /// </summary>
        public bool CanCaptureAs106 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can consume ragdolls when playing as SCP-049-2.
        /// </summary>
        public bool CanConsumeRagdollsAsZombie { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player (when playing as SCP-096) can be triggered by other players.
        /// </summary>
        public bool CanBeTriggeredAs096 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player (when playing as SCP-173) can be blocked by other players.
        /// </summary>
        public bool CanBeBlockedAs173 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player (when playing as SCP-939) can use the Lunge ability.
        /// </summary>
        public bool CanLungeAs939 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can gain experience when playing as SCP-079.
        /// </summary>
        public bool CanGainExpAs079 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player (when playing as SCP-939) can use the Mimicry ability.
        /// </summary>
        public bool CanUseMimicryAs939 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player (when playing as SCP-079) can be recontained.
        /// </summary>
        public bool CanBeRecontainedAs079 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player (when playing as SCP, except SCP-079) can prevent recontaining of SCP-079.
        /// </summary>
        public bool PreventsRecontaining079 { get; set; } = true;
        #endregion

        #region Item Switches
        /// <summary>
        /// Gets or sets a value indicating whether or not this player can drop items.
        /// </summary>
        public bool CanDropItems { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can throw items.
        /// </summary>
        public bool CanThrowItems { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can pick up items.
        /// </summary>
        public bool CanPickUpItems { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can pick up ammo.
        /// </summary>
        public bool CanPickUpAmmo { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can switch their currently held item.
        /// </summary>
        public bool CanSwitchItems { get; set; } = true;
        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can receive effects.
        /// </summary>
        public bool CanReceiveEffects { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can trigger tesla gates.
        /// </summary>
        public bool CanTriggerTesla { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can change their role.
        /// <para><b>This includes the Spectator role, so this would give the player godmode as well.</b></para>
        /// </summary>
        public bool CanChangeRoles { get; set; } = true;

        #region Voice Switches
        /// <summary>
        /// Gets or sets a value indicating whether or not this player can be heard by anyone.
        /// </summary>
        public bool CanBeHeard { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can be heard by staff.
        /// <para>Overrides <see cref="CanBeHeard"/>.</para>
        /// </summary>
        public bool CanBeHeardByStaff { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can by heard by roles other than theirs.
        /// </summary>
        public bool CanBeHeardByOtherRoles { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can be heard by people spectating them.
        /// <para>Overrides <see cref="CanBeHeard"/>.</para>
        /// </summary>
        public bool CanBeHeardBySpectators { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether the player can hear themselves speaking.
        /// </summary>
        public bool CanHearSelf { get; set; }
        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether or not any damage dealt to other players will result in instant death.
        /// </summary>
        public bool HasInstantKill { get; set; } = false; // BROKEN

        /// <summary>
        /// Gets or sets a value indicating whether or not this player consumes ammo while shooting.
        /// </summary>
        public bool HasUnlimitedAmmo { get; set; } = false; // BROKEN

        /// <summary>
        /// Whether or not this player should receive position messages.
        /// </summary>
        public bool ShouldReceivePositions { get; set; } = true;

        /// <summary>
        /// Whether or not this player should send position messages to other players.
        /// </summary>
        public bool ShouldSendPosition { get; set; } = true;

        /// <summary>
        /// Whether or not to send this player's own position to the player.
        /// </summary>
        public bool ShouldReceiveOwnPosition { get; set; } = false;

        /// <summary>
        /// Resets the values of these switches to real players.
        /// </summary>
        /// <param name="fromConfig">Whether or not to reset to values from the config.</param>
        public void ResetToPlayer(bool fromConfig = true) 
            => Copy(fromConfig ? PlayerConfig : DefaultPlayerSwitches);
        
        /// <summary>
        /// Resets the values of these switches to dummy players.
        /// </summary>
        /// <param name="fromConfig">Whether or not to reset to values from the config.</param>
        public void ResetToNpc(bool fromConfig = true) 
            => Copy(fromConfig ? NpcConfig : DefaultNpcSwitches);

        /// <summary>
        /// Copies switches from another container.
        /// </summary>
        /// <param name="other">The other container.</param>
        public void Copy(SwitchContainer other)
        {
            if (other is null)
                return;

            IgnoredEffects.Clear();
            
            other.IgnoredEffects.ForEach(x => IgnoredEffects.Add(x));

            IsVisibleInRemoteAdmin = other.IsVisibleInRemoteAdmin;
            IsVisibleInSpectatorList = other.IsVisibleInSpectatorList;
            IsVisibleToScp939 = other.IsVisibleToScp939;

            CanSeeEveryoneAs939 = other.CanSeeEveryoneAs939;
            CanBeRespawned = other.CanBeRespawned;
            CanBlockRoundEnd = other.CanBlockRoundEnd;
            CanBeScp049Target = other.CanBeScp049Target;
            CanBePocketDimensionItemTarget = other.CanBePocketDimensionItemTarget;
            CanTriggerScp096 = other.CanTriggerScp096;
            CanBlockScp173 = other.CanBlockScp173;
            CanBeCapturedBy106 = other.CanBeCapturedBy106;
            CanBeStrangledBy3114 = other.CanBeStrangledBy3114;
            CanHearAmnesticCloudSpawn = other.CanHearAmnesticCloudSpawn;
            CanCountAs079ExpTarget = other.CanCountAs079ExpTarget;
            CanBeResurrectedBy049 = other.CanBeResurrectedBy049;
            CanBeConsumedByZombies = other.CanBeConsumedByZombies;
            CanUseSenseAs049 = other.CanUseSenseAs049;
            CanUseResurrectAs049 = other.CanUseResurrectAs049;
            CanStrangleAs3114 = other.CanStrangleAs3114;
            CanCaptureAs106 = other.CanCaptureAs106;
            CanConsumeRagdollsAsZombie = other.CanConsumeRagdollsAsZombie;
            CanBeTriggeredAs096 = other.CanBeTriggeredAs096;
            CanBeBlockedAs173 = other.CanBeBlockedAs173;
            CanLungeAs939 = other.CanLungeAs939;
            CanGainExpAs079 = other.CanGainExpAs079;
            CanUseMimicryAs939 = other.CanUseMimicryAs939;
            CanBeRecontainedAs079 = other.CanBeRecontainedAs079;

            PreventsRecontaining079 = other.PreventsRecontaining079;

            CanDropItems = other.CanDropItems;
            CanThrowItems = other.CanThrowItems;
            CanPickUpItems = other.CanPickUpItems;
            CanPickUpAmmo = other.CanPickUpAmmo;
            CanSwitchItems = other.CanSwitchItems;
            CanReceiveEffects = other.CanReceiveEffects;
            CanTriggerTesla = other.CanTriggerTesla;
            CanChangeRoles = other.CanChangeRoles;

            CanBeHeard = other.CanBeHeard;
            CanBeHeardByStaff = other.CanBeHeardByStaff;
            CanBeHeardByOtherRoles = other.CanBeHeardByOtherRoles;
            CanBeHeardBySpectators = other.CanBeHeardBySpectators;
            CanHearSelf = other.CanHearSelf;

            HasInstantKill = other.HasInstantKill;
            HasUnlimitedAmmo = other.HasUnlimitedAmmo;

            ShouldReceiveOwnPosition = other.ShouldReceiveOwnPosition;
            ShouldReceivePositions = other.ShouldReceivePositions;
            ShouldSendPosition = other.ShouldSendPosition;
        }
    }
}