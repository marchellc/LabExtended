using LabExtended.API;

using PlayerStatsSystem;

namespace LabExtended.Events.Other
{
    /// <summary>
    /// Gets called when after <see cref="StandardDamageHandler.ApplyDamage(ReferenceHub)"/> gets called.
    /// </summary>
    public class AppliedDamageArgs
    {
        /// <summary>
        /// The damage being applied.
        /// </summary>
        public DamageInfo Info { get; }

        /// <summary>
        /// The result of the damage application.
        /// </summary>
        public DamageHandlerBase.HandlerOutput Output { get; set; }

        /// <summary>
        /// The target that the damage is being applied on.
        /// </summary>
        public ExPlayer? Target { get; set; }

        internal AppliedDamageArgs(DamageInfo damageInfo, DamageHandlerBase.HandlerOutput output, ExPlayer? target)
        {
            Info = damageInfo;
            Output = output;
            Target = target;
        }
    }
}