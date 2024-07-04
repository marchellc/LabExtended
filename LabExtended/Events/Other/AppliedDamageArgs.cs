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
        public DamageInfo Info { get; }

        public DamageHandlerBase.HandlerOutput Output { get; set; }

        public ExPlayer Target { get; set; }

        internal AppliedDamageArgs(DamageInfo damageInfo, DamageHandlerBase.HandlerOutput output, ExPlayer target)
        {
            Info = damageInfo;
            Output = output;
            Target = target;
        }
    }
}