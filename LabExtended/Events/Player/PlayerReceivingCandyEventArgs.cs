using InventorySystem.Items.Usables.Scp330;

using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when the player picks up a candy from the candy bowl.
    /// </summary>
    public class PlayerReceivingCandyEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player that is receiving a candy.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets or sets the type of candy to be added.
        /// </summary>
        public CandyKindID CandyType { get; set; }

        /// <summary>
        /// Creates a new PlayerReceivingCandyEventArgs instance.
        /// </summary>
        /// <param name="player">The player receiving a candy.</param>
        /// <param name="type">The type of candy being added.</param>
        public PlayerReceivingCandyEventArgs(ExPlayer player, CandyKindID type)
            => (Player, CandyType) = (player, type);
    }
}