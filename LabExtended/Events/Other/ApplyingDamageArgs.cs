using LabExtended.API;
using LabExtended.Core.Events;

using PlayerStatsSystem;

namespace LabExtended.Events.Other
{
    /// <summary>
    /// Gets called when before <see cref="StandardDamageHandler.ApplyDamage(ReferenceHub)"/> gets called.
    /// </summary>
    public class ApplyingDamageArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// The damage being applied.
        /// </summary>
        public DamageInfo Info { get; }

        /// <summary>
        /// The damage target.
        /// </summary>
        public ExPlayer Target { get; set; }

        internal ApplyingDamageArgs(DamageInfo damageInfo, ExPlayer target)
        {
            Info = damageInfo;
            Target = target;
        }
    }
}