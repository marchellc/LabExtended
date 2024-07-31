namespace LabExtended.Events
{
    /// <summary>
    /// A class used for round event delegates. These delegates get called before any other event handlers.
    /// </summary>
    public static class RoundEvents
    {
        /// <summary>
        /// Gets called when the round ends.
        /// </summary>
        public static event Action OnRoundEnded;

        /// <summary>
        /// Gets called when the round starts.
        /// </summary>
        public static event Action OnRoundStarted;

        /// <summary>
        /// Gets called when the round starts restarting.
        /// </summary>
        public static event Action OnRoundRestarted;

        /// <summary>
        /// Gets called when the round starts waiting for players.
        /// </summary>
        public static event Action OnWaitingForPlayers;

        internal static void InvokeWaiting()
            => OnWaitingForPlayers?.Invoke();

        internal static void InvokeStarted()
            => OnRoundStarted?.Invoke();

        internal static void InvokeEnded()
            => OnRoundEnded?.Invoke();

        internal static void InvokeRestarted()
            => OnRoundRestarted?.Invoke();
    }
}