using LabExtended.API;

namespace LabExtended.Events.Scp049
{
    /// <summary>
    /// Gets called after SCP-049 cancels it's Resurrection ability.
    /// </summary>
    public class Scp049CancelledResurrectionEventArgs : EventArgs
    {
        /// <summary>
        /// The player playing as SCP-049.
        /// </summary>
        public ExPlayer Scp { get; }

        /// <summary>
        /// Owner of the targeted ragdoll.
        /// </summary>
        public ExPlayer? Target { get; }

        /// <summary>
        /// Creates a new <see cref="Scp049CancelledResurrectionEventArgs"/> instance.
        /// </summary>
        /// <param name="scp">SCP-049 player.</param>
        /// <param name="target">Target ragdoll owner.</param>
        public Scp049CancelledResurrectionEventArgs(ExPlayer scp, ExPlayer? target)
            => (Scp, Target) = (scp, target);
    }
}