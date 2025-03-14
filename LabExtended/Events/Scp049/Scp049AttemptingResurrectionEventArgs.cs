using LabExtended.API;

using PlayerRoles.PlayableScps.Scp049;

namespace LabExtended.Events.Scp049
{
    /// <summary>
    /// Called before SCP-049 resurrects a ragdoll.
    /// </summary>
    public class Scp049AttemptingResurrectionEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player playing as SCP-049.
        /// </summary>
        public ExPlayer Scp { get; }
        
        /// <summary>
        /// The targeted ragdoll's owner.
        /// </summary>
        public ExPlayer Target { get; }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public Scp049ResurrectAbility.ResurrectError Error { get; set; }

        /// <summary>
        /// Creates a new <see cref="Scp049AttemptingResurrectionEventArgs"/> instance.
        /// </summary>
        /// <param name="scp">SCP-049 player.</param>
        /// <param name="target">Target ragdoll owner.</param>
        /// <param name="error">Error code.</param>
        public Scp049AttemptingResurrectionEventArgs(ExPlayer scp, ExPlayer target, Scp049ResurrectAbility.ResurrectError error) 
            => (Scp, Target, Error) = (scp, target, error);
    }
}