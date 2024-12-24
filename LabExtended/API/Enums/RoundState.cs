namespace LabExtended.API.Enums
{
    /// <summary>
    /// Used to specify the round's current state.
    /// </summary>
    public enum RoundState
    {
        /// <summary>
        /// The round is waiting for players.
        /// </summary>
        WaitingForPlayers,

        /// <summary>
        /// The round is in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// The round has ended.
        /// </summary>
        Ended,

        /// <summary>
        /// The round is restarting.
        /// </summary>
        Restarting
    }
}