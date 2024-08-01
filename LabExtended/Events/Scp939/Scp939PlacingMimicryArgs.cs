using LabExtended.API;
using LabExtended.Core.Events;

using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.PlayableScps.Scp939.Mimicry;

using RelativePositioning;

namespace LabExtended.Events.Scp939
{
    public class Scp939PlacingMimicryArgs : HookBooleanCancellableEventBase
    {
        public ExPlayer Scp { get; }

        public Scp939Role Role { get; }
        public MimicPointController Controller { get; }

        public RelativePosition Position { get; set; }

        internal Scp939PlacingMimicryArgs(ExPlayer scp, RelativePosition position)
            => (Scp, Role, Controller, Position) = (scp, scp.Role.Scp939, scp.Subroutines.MimicPointController, position);
    }
}
