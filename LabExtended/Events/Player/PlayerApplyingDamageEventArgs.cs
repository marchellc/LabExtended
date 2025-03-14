using LabExtended.API;

using PlayerStatsSystem;

namespace LabExtended.Events.Other
{
    /// <summary>
    /// Gets called before <see cref="StandardDamageHandler.ApplyDamage(ReferenceHub)"/> gets called.
    /// </summary>
    public class PlayerApplyingDamageEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The damage being applied.
        /// </summary>
        public DamageInfo Info { get; }

        /// <summary>
        /// The damage target.
        /// </summary>
        public ExPlayer? Target { get; set; }

        /// <summary>
        /// Creates a new <see cref="PlayerApplyingDamageEventArgs"/> instance.
        /// </summary>
        /// <param name="damageInfo">Information about the damage being done.</param>
        /// <param name="target">The target.</param>
        public PlayerApplyingDamageEventArgs(DamageInfo damageInfo, ExPlayer? target)
        {
            Info = damageInfo;
            Target = target;
        }
    }
}