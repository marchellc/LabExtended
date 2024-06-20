namespace LabExtended.API.Voice.Processing
{
    /// <summary>
    /// An interface for custom voice chat data processing.
    /// </summary>
    public interface IVoiceProcessor
    {
        /// <summary>
        /// Whether or not this voice processor is globally active.
        /// </summary>
        bool IsGloballyActive { get; }

        /// <summary>
        /// Gets called when a player sends a voice packet.
        /// </summary>
        /// <param name="speaker">The speaking player.</param>
        /// <param name="data">The data received.</param>
        /// <param name="dataLength">The length of the received data.</param>
        /// <returns><see langword="true"/> if the data was modified, otherwise <see langword="false"/>.</returns>
        bool Process(ExPlayer speaker, ref byte[] data, ref int dataLength);

        /// <summary>
        /// Gets a value indicating whether this processor is active for a specific player.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns><see langword="true"/> if it's active, otherwise <see langword="false"/>.</returns>
        bool IsActive(ExPlayer player);

        /// <summary>
        /// Sets this processor's state for the specified player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="active">The status.</param>
        void SetActive(ExPlayer player, bool active);
    }
}