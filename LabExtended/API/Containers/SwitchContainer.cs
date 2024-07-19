namespace LabExtended.API.Containers
{
    /// <summary>
    /// A class that holds custom player switches.
    /// </summary>
    public class SwitchContainer
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not this player is visible in the Remote Admin player list.
        /// </summary>
        public bool IsVisibleInRemoteAdmin { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player is visible in the Spectator List.
        /// </summary>
        public bool IsVisibleInSpectatorList { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player should count in the next respawn wave.
        /// </summary>
        public bool CanRespawn { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player prevents the round from ending.
        /// </summary>
        public bool CanBlockRoundEnd { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can trigger SCP-096.
        /// </summary>
        public bool CanTriggerScp096 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player (when playing as SCP-096) can be triggered by other players.
        /// </summary>
        public bool CanBeTriggered { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can block SCP-173's movement.
        /// </summary>
        public bool CanBlockScp173 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player (when playing as SCP-173) can be blocked by other players.
        /// </summary>
        public bool CanBeBlocked { get; set; } = true;

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

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can trigger tesla gates.
        /// </summary>
        public bool CanTriggerTesla { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can change their role.
        /// <para><b>This includes the Spectator role, so this would give the player godmode as well.</b></para>
        /// </summary>
        public bool CanChangeRoles { get; set; } = true;

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
    }
}