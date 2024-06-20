using LabExtended.API;

using PlayerStatsSystem;

namespace LabExtended.Interfaces
{
    /// <summary>
    /// An interface for objects that can damage players.
    /// </summary>
    public interface IDamageObject
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not this object can damage players.
        /// </summary>
        bool IsDamageDisabled { get; set; }

        /// <summary>
        /// Deals damage to the specified player.
        /// </summary>
        /// <param name="player">The player to deal damage to.</param>
        /// <param name="amount">The amount of damage to deal.</param>
        void Damage(ExPlayer player, float amount);

        /// <summary>
        /// Kills the specified player.
        /// </summary>
        /// <param name="player">The player to kill.</param>
        void Kill(ExPlayer player);

        /// <summary>
        /// Gets a new <see cref="DamageHandlerBase"/> used by the object.
        /// </summary>
        /// <param name="damageAmount">The amount of damage to set.</param>
        /// <returns>The <see cref="DamageHandlerBase"/> instance.</returns>
        DamageHandlerBase GetDamageHandler(float damageAmount);
    }
}