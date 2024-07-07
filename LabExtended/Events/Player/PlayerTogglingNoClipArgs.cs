using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player toggles their no-clip state.
    /// </summary>
    public class PlayerTogglingNoClipArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// The player toggling their noclip state.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the current noclip state.
        /// </summary>
        public bool CurrentState { get; }

        /// <summary>
        /// Gets or sets the new noclip state.
        /// </summary>
        public bool NewState { get; set; }

        internal PlayerTogglingNoClipArgs(ExPlayer player, bool curState)
        {
            Player = player;

            CurrentState = curState;
            NewState = !curState;
        }
    }
}