using LabExtended.API;
using LabExtended.Core.Events;

using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.PlayableScps.Scp939.Mimicry;

using RelativePositioning;

namespace LabExtended.Events.Scp939
{
    public class Scp939RemovingMimicryArgs : BoolCancellableEvent
    {
        public ExPlayer Scp { get; }

        public Scp939Role Role { get; }
        public MimicPointController Controller { get; }

        public RelativePosition Position { get; set; }

        public MimicPointController.RpcStateMsg Reason { get; }

        internal Scp939RemovingMimicryArgs(ExPlayer scp, RelativePosition position, MimicPointController.RpcStateMsg reason)
            => (Scp, Role, Controller, Position, Reason) = (scp, scp.Role.Scp939, scp.Subroutines.MimicPointController, position, reason);
    }
}
