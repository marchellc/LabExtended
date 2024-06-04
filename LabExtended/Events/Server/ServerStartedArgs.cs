using LabExtended.Core.Events;

namespace LabExtended.Events.Server
{
    /// <summary>
    /// Occurs when the server finishes it's startup.
    /// </summary>
    public class ServerStartedArgs : HookEvent
    {
        /// <summary>
        /// The date of the server starting.
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// How long it took for the server to start.
        /// </summary>
        public TimeSpan Time { get; }

        internal ServerStartedArgs(DateTime startTime, TimeSpan time)
        {
            StartTime = startTime;
            Time = time;
        }
    }
}