using InventorySystem.Items.Usables.Scp330;

using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called before player drops a candy from their inventory.
    /// </summary>
    public class PlayerDroppingCandyEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player dropping a candy.
        /// </summary>
        public ExPlayer Player { get; }
        
        /// <summary>
        /// The bag that contains the candy.
        /// </summary>
        public Scp330Bag Bag { get; }
        
        /// <summary>
        /// Gets or sets the index of the dropped candy.
        /// </summary>
        public int Index { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the dropped candy.
        /// </summary>
        public CandyKindID Type { get; set; }

        /// <summary>
        /// Creates a new <see cref="PlayerDroppingCandyEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player dropping the candy.</param>
        /// <param name="bag">The bag.</param>
        /// <param name="index">The candy index.</param>
        /// <param name="type">The candy type.</param>
        public PlayerDroppingCandyEventArgs(ExPlayer player, Scp330Bag bag, int index, CandyKindID type)
        {
            Player = player;
            Bag = bag;
            Index = index;
            Type = type;
        }
    }
}