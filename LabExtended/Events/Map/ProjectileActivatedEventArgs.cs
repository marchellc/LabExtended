using InventorySystem.Items.ThrowableProjectiles;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called after an inactive projectile gets activated by another explosion.
    /// </summary>
    /// <remarks>This event is called <b>before</b> the spawned projectile is activated by calling <see cref="ThrownProjectile.ServerActivate"/></remarks>
    public class ProjectileActivatedEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the instance of the projectile which was activated.
        /// </summary>
        public TimedGrenadePickup Pickup { get; }

        /// <summary>
        /// Gets the projectile which was spawned.
        /// </summary>
        public ThrownProjectile Projectile { get; }

        /// <summary>
        /// Initializes a new instance of the ProjectileActivatedEventArgs class with the specified pickup and
        /// projectile information.
        /// </summary>
        /// <param name="pickup">The TimedGrenadePickup instance representing the pickup that triggered the projectile activation. Cannot be
        /// null.</param>
        /// <param name="projectile">The ThrownProjectile instance representing the activated projectile. Cannot be null.</param>
        public ProjectileActivatedEventArgs(TimedGrenadePickup pickup, ThrownProjectile projectile)
        {
            Pickup = pickup;
            Projectile = projectile;
        }
    }
}