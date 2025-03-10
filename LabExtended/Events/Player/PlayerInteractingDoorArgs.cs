using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    public class PlayerInteractingDoorArgs : BoolCancellableEvent
    {
        public ExPlayer Player { get; }

        public Door Door { get; }

        public bool CanOpen { get; set; }

        internal PlayerInteractingDoorArgs(ExPlayer? player, Door door, bool canOpen)
            => (Player, Door, CanOpen) = (player, door, canOpen);
    }
}