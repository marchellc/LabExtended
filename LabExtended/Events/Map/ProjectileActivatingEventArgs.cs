using UnityEngine;

using Footprinting;

using InventorySystem.Items.ThrowableProjectiles;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when an inactive projectile is activated by another explosion.
    /// </summary>
    public class ProjectileActivatingEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the instance of the projectile which will be activated.
        /// </summary>
        public TimedGrenadePickup Pickup { get; }

        /// <summary>
        /// Gets the footprint of the attacker who threw the grenade which caused the explosion.
        /// </summary>
        public Footprint Attacker { get; }

        /// <summary>
        /// Gets the position of the explosion.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Initializes a new instance of the ProjectileActivatingEventArgs class with the specified projectile,
        /// attacker, and activation position.
        /// </summary>
        /// <param name="projectile">The TimedGrenadePickup instance representing the projectile being activated. Cannot be null.</param>
        /// <param name="attacker">The Footprint representing the entity that activated the projectile. May be null if the activation was not
        /// caused by an entity.</param>
        /// <param name="position">The position in world coordinates where the projectile was activated.</param>
        public ProjectileActivatingEventArgs(TimedGrenadePickup projectile, Footprint attacker, Vector3 position)
        {
            Pickup = projectile;
            Attacker = attacker;
            Position = position;
        }
    }
}