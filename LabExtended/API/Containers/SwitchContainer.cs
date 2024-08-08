using LabExtended.API.Collections.Locked;
using LabExtended.Extensions;

namespace LabExtended.API.Containers
{
    /// <summary>
    /// A class that holds custom player switches.
    /// </summary>
    public class SwitchContainer
    {
        /// <summary>
        /// Gets a list of ignored effect types.
        /// </summary>
        public LockedHashSet<Type> IgnoredEffects { get; } = new LockedHashSet<Type>();

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
        public bool IsVisibleToScp939 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can see every other player when playing as SCP-939.
        /// </summary>
        public bool CanSeeEveryoneAs939 { get; set; }
        #endregion

        #region Round Switches
        /// <summary>
        /// Gets or sets a value indicating whether or not this player should count in the next respawn wave.
        /// </summary>
        public bool CanBeRespawned { get; set; } = true;

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
        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether or not any damage dealt to other players will result in instant death.
        /// </summary>
        public bool HasInstantKill { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player consumes ammo while shooting.
        /// </summary>
        public bool HasUnlimitedAmmo { get; set; } = false;

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

        public void Copy(SwitchContainer other)
        {
            if (other is null)
                return;

            other.CopyPropertiesTo(this);
        }
    }
}