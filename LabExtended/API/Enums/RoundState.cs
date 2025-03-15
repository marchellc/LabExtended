namespace LabExtended.API.Enums
{
    /// <summary>
    /// Used to specify the round's current state.
    /// </summary>
    [Flags]
    public enum RoundState : byte
    {
        /// <summary>
        /// The round is waiting for players.
        /// </summary>
        WaitingForPlayers = 1,

        /// <summary>
        /// The round is in progress.
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// The round is ending (before the end screen shows)
        /// </summary>
        Ending = 4,
        
        /// <summary>
        /// The round has ended (occurs ~1.5 secs after Ended).
        /// </summary>
        Ended = 8,

        /// <summary>
        /// The round is restarting.
        /// </summary>
        Restarting = 16
    }
}