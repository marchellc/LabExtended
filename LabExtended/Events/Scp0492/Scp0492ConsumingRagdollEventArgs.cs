using LabExtended.API;

using PlayerRoles.Ragdolls;

namespace LabExtended.Events.Scp0492
{
    /// <summary>
    /// Gets called before SCP-049-2 uses it's Consume ability.
    /// </summary>
    public class Scp0492ConsumingRagdollEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player playing as SCP-049-2.
        /// </summary>
        public ExPlayer Scp { get; }
        
        /// <summary>
        /// The owner of the targeted ragdoll.
        /// </summary>
        public ExPlayer? Target { get; }

        /// <summary>
        /// The targeted ragdoll.
        /// </summary>
        public BasicRagdoll Ragdoll { get; }

        /// <summary>
        /// Response code used in case this event is disallowed.
        /// </summary>
        public byte Code { get; set; } = 0;

        /// <summary>
        /// Creates a new <see cref="Scp0492ConsumingRagdollEventArgs"/>.
        /// </summary>
        /// <param name="scp">SCP-049-2 player.</param>
        /// <param name="target">Ragdoll owner.</param>
        /// <param name="ragdoll">Target ragdoll.</param>
        public Scp0492ConsumingRagdollEventArgs(ExPlayer scp, ExPlayer? target, BasicRagdoll ragdoll)
            => (Scp, Target, Ragdoll) = (scp, target, ragdoll);
    }
}