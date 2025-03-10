using LabExtended.API;
using LabExtended.Core.Events;

using PlayerRoles.Ragdolls;

namespace LabExtended.Events.Scp0492
{
    public class Scp0492ConsumingRagdollArgs : BoolCancellableEvent
    {
        public ExPlayer Scp { get; }
        public ExPlayer Target { get; }

        public BasicRagdoll Ragdoll { get; }

        public byte Code { get; set; } = 0;

        internal Scp0492ConsumingRagdollArgs(ExPlayer? scp, ExPlayer target, BasicRagdoll ragdoll)
            => (Scp, Target, Ragdoll) = (scp, target, ragdoll);
    }
}