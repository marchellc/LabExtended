using LabExtended.API;
using PlayerRoles.PlayableScps.Scp049.Zombies;
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
        /// Error that prevents the player from consuming.
        /// </summary>
        public ZombieConsumeAbility.ConsumeError Error { get; set; }

        /// <summary>
        /// Creates a new <see cref="Scp0492ConsumingRagdollEventArgs"/>.
        /// </summary>
        /// <param name="scp">SCP-049-2 player.</param>
        /// <param name="target">Ragdoll owner.</param>
        /// <param name="ragdoll">Target ragdoll.</param>
        /// <param name="error">The consume error.</param>
        public Scp0492ConsumingRagdollEventArgs(ExPlayer scp, ExPlayer? target, BasicRagdoll ragdoll, ZombieConsumeAbility.ConsumeError error)
            => (Scp, Target, Ragdoll, Error) = (scp, target, ragdoll, error);
    }
}