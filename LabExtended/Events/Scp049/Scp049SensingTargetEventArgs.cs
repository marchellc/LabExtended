using LabExtended.API;

using PlayerRoles.PlayableScps.Scp049;

namespace LabExtended.Events.Scp049
{
    /// <summary>
    /// Called before SCP-049 uses it's Sense ability.
    /// </summary>
    public class Scp049SensingTargetEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player playing as SCP-049.
        /// </summary>
        public ExPlayer Scp { get; }
        
        /// <summary>
        /// Sense target.
        /// </summary>
        public ExPlayer Target { get; set; }

        /// <summary>
        /// SCP-049-2 role instance.
        /// </summary>
        public Scp049Role Role { get; }
        
        /// <summary>
        /// SCP-049-2 sense ability subroutine instance.
        /// </summary>
        public Scp049SenseAbility Ability { get; }

        /// <summary>
        /// Gets or sets the ability cooldown after this usage.
        /// </summary>
        public double Cooldown { get; set; } = 2.5;
        
        /// <summary>
        /// Gets or sets the ability duration.
        /// </summary>
        public double Duration { get; set; } = 20;

        /// <summary>
        /// Creates a new <see cref="Scp049SensingTargetEventArgs"/> instance.
        /// </summary>
        /// <param name="scp">SCP-049 player.</param>
        /// <param name="target">Sense target.</param>
        /// <param name="role">Role instance.</param>
        /// <param name="ability">Subroutine instance.</param>
        public Scp049SensingTargetEventArgs(ExPlayer scp, ExPlayer target, Scp049Role role, Scp049SenseAbility ability)
            => (Scp, Target, Role, Ability) = (scp, target, role, ability);
    }
}