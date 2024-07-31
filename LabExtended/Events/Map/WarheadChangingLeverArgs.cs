using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a player interacts with the Warhead lever.
    /// </summary>
    public class WarheadChangingLeverArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// The lever's current state.
        /// </summary>
        public bool CurrentState { get; }

        /// <summary>
        /// The lever's new state.
        /// </summary>
        public bool NewState { get; set; }

        /// <summary>
        /// The player who's interacting with the lever.
        /// </summary>
        public ExPlayer Player { get; }

        internal WarheadChangingLeverArgs(bool curState, ExPlayer player)
        {
            CurrentState = curState;
            NewState = !curState;
            Player = player;
        }
    }
}