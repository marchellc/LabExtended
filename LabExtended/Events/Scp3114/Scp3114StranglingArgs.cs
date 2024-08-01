using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Scp3114
{
    public class Scp3114StranglingArgs : HookBooleanCancellableEventBase
    {
        public ExPlayer Scp { get; }
        public ExPlayer Target { get; set; }

        internal Scp3114StranglingArgs(ExPlayer scp, ExPlayer target)
            => (Scp, Target) = (scp, target);
    }
}