using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    public class PlayerTogglingRoundLockArgs : HookBooleanCancellableEventBase
    {
        public ExPlayer Player { get; }

        public bool CurrentState { get; }
        public bool NewState { get; set; }

        internal PlayerTogglingRoundLockArgs(ExPlayer player, bool curState)
        {
            Player = player;
            CurrentState = curState;
            NewState = !curState;
        }
    }
}