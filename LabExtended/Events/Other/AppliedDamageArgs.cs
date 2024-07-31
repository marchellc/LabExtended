using LabExtended.API;
using LabExtended.Core.Hooking.Interfaces;

using PlayerStatsSystem;

namespace LabExtended.Events.Other
{
    /// <summary>
    /// Gets called when after <see cref="StandardDamageHandler.ApplyDamage(ReferenceHub)"/> gets called.
    /// </summary>
    public class AppliedDamageArgs : IHookEvent
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
        public ExPlayer Target { get; set; }

        internal AppliedDamageArgs(DamageInfo damageInfo, DamageHandlerBase.HandlerOutput output, ExPlayer target)
        {
            Info = damageInfo;
            Output = output;
            Target = target;
        }
    }
}