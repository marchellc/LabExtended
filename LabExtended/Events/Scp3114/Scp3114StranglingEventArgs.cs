using LabExtended.API;

namespace LabExtended.Events.Scp3114
{
    /// <summary>
    /// Gets called before SCP-3114 starts strangling a player.
    /// </summary>
    public class Scp3114StranglingEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player who is playing as SCP-3114.
        /// </summary>
        public ExPlayer Scp { get; }
        
        /// <summary>
        /// The player who is going to be strangled.
        /// </summary>
        public ExPlayer Target { get; set; }

        /// <summary>
        /// Creates a new <see cref="Scp3114StranglingEventArgs"/> instance.
        /// </summary>
        /// <param name="scp">SCP-3114 player.</param>
        /// <param name="target">Target player.</param>
        public Scp3114StranglingEventArgs(ExPlayer scp, ExPlayer target)
            => (Scp, Target) = (scp, target);
    }
}