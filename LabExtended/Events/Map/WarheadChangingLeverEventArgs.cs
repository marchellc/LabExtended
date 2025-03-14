using LabExtended.API;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a player interacts with the Warhead lever.
    /// </summary>
    public class WarheadChangingLeverEventArgs : BooleanEventArgs
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
        public ExPlayer? Player { get; }

        /// <summary>
        /// Creates a new <see cref="WarheadChangingLeverEventArgs"/> instance.
        /// </summary>
        /// <param name="curState">The current state of the lever.</param>
        /// <param name="player">The player interacting with the lever.</param>
        public WarheadChangingLeverEventArgs(bool curState, ExPlayer? player)
        {
            CurrentState = curState;
            NewState = !curState;
            Player = player;
        }
    }
}