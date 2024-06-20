namespace LabExtended.API.Voice
{
    /// <summary>
    /// Flags for custom voice chat.
    /// </summary>
    [Flags]
    public enum VoiceFlags : byte
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// The receiver can hear himself.
        /// </summary>
        CanHearSelf = 2
    }
}