namespace LabExtended.API
{
    /// <summary>
    /// A class that holds custom player switches.
    /// </summary>
    public class PlayerSwitches
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
        /// Gets or sets a value indicating whether or not this player can block SCP-173's movement.
        /// </summary>
        public bool CanBlockScp173 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can drop items.
        /// </summary>
        public bool CanDropItems { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can pick up items.
        /// </summary>
        public bool CanPickUpItems { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player is able to use their inventory.
        /// </summary>
        public bool CanUseInventory { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not this player can trigger tesla gates.
        /// </summary>
        public bool CanTriggerTesla { get; set; } = true;
    }
}