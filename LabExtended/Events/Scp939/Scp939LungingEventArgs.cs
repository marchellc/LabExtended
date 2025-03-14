using LabExtended.API;

using PlayerRoles.PlayableScps.Scp939;

namespace LabExtended.Events.Scp939
{
    /// <summary>
    /// Gets called before SCP-939 uses it's Lunge ability.
    /// </summary>
    public class Scp939LungingEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player playing as SCP-939.
        /// </summary>
        public ExPlayer Scp { get; }
        
        /// <summary>
        /// The primary lunge target.
        /// </summary>
        public ExPlayer? Target { get; set; }

        /// <summary>
        /// The SCP-939 role instance.
        /// </summary>
        public Scp939Role Role { get; }
        
        /// <summary>
        /// The <see cref="Scp939LungeAbility"/> subroutine instance.
        /// </summary>
        public Scp939LungeAbility Ability { get; }

        /// <summary>
        /// Gets or sets the damage dealt to the primary target-
        /// </summary>
        public float LungeTargetDamage { get; set; } = 120f;
        
        /// <summary>
        /// Gets or sets the damage dealt to secondary targets.
        /// </summary>
        public float LungeSecondaryDamage { get; set; } = 30f;

        /// <summary>
        /// Creates a new <see cref="Scp939LungingEventArgs"/> instance.
        /// </summary>
        /// <param name="scp">SCP-939 player.</param>
        /// <param name="target">Primary target.</param>
        /// <param name="role">Role instance.</param>
        /// <param name="ability">Ability instance.</param>
        public Scp939LungingEventArgs(ExPlayer scp, ExPlayer? target, Scp939Role role, Scp939LungeAbility ability)
            => (Scp, Target, Role, Ability) = (scp, target, role, ability);
    }
}