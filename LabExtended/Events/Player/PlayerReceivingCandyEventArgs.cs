using InventorySystem.Items.Usables.Scp330;

using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when the player picks up a candy from the candy bowl.
    /// </summary>
    public class PlayerReceivingCandyEventArgs : BoolCancellableEvent
    {
        /// <summary>
        /// The player that is receiving a candy.
        /// </summary>
        public ExPlayer? Player { get; }

        /// <summary>
        /// Whether or not the candy can be added.
        /// </summary>
        public bool CanAdd { get; }

        /// <summary>
        /// Gets or sets the type of candy to be added.
        /// </summary>
        public CandyKindID CandyType { get; set; }

        /// <summary>
        /// Creates a new PlayerAddingCandyArgs instance.
        /// </summary>
        /// <param name="player">The player receiving a candy.</param>
        /// <param name="type">The type of candy being added.</param>
        /// <param name="canAdd">Whether or not the candy can be added.</param>
        public PlayerReceivingCandyEventArgs(ExPlayer? player, CandyKindID type, bool canAdd)
            => (Player, CandyType, CanAdd) = (player, type, canAdd);
    }
}