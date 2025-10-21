using LabExtended.API;

namespace LabExtended.Events.Scp049
{
    /// <summary>
    /// Gets called before SCP-049 cancels it's Resurrection ability.
    /// </summary>
    public class Scp049CancellingResurrectionEventArgs : BooleanEventArgs
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
        /// Error code to send in case this event is disallowed.
        /// </summary>
        public byte ErrorCode { get; set; }

        /// <summary>
        /// Creates a new <see cref="Scp049CancellingResurrectionEventArgs"/> instance.
        /// </summary>
        /// <param name="scp">SCP-049 player.</param>
        /// <param name="target">Target ragdoll owner.</param>
        /// <param name="code">Error code.</param>
        public Scp049CancellingResurrectionEventArgs(ExPlayer scp, ExPlayer? target, byte code) 
            => (Scp, Target, ErrorCode) = (scp, target, code);
    }
}