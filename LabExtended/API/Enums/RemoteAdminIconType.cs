namespace LabExtended.API.Enums
{
    /// <summary>
    /// An enum for specifying a player's icon type.
    /// </summary>
    [Flags]
    public enum RemoteAdminIconType : byte
    {
        /// <summary>
        /// No icons.
        /// </summary>
        None = 0,

        /// <summary>
        /// The muted icon.
        /// </summary>
        MutedIcon = 2,

        /// <summary>
        /// The Overwatch icon.
        /// </summary>
        OverwatchIcon = 4,
        
        /// <summary>
        /// The Dummy player icon.
        /// </summary>
        DummyIcon = 8,
    }
}