using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player is about to receive their starting inventory.
    /// </summary>
    public class PlayerGrantingInventoryArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// The player receiving the inventory.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Whether or not to remove previous items.
        /// </summary>
        public bool ShouldResetInventory { get; set; }

        /// <summary>
        /// Whether or not to grant the starting inventory.
        /// </summary>
        public bool ShouldGrantInventory { get; set; }

        /// <summary>
        /// Whether or not the player's role change reason is escaping.
        /// </summary>
        public bool HasEscaped { get; }

        /// <summary>
        /// Whether or not to drop previous items.
        /// </summary>
        public bool DropPreviousItems { get; set; }

        /// <summary>
        /// A list of items to add.
        /// </summary>
        public List<ItemType> Items { get; }

        /// <summary>
        /// A dictionary of ammo type and their amount to add.
        /// </summary>
        public Dictionary<ItemType, ushort> Ammo { get; }

        internal PlayerGrantingInventoryArgs(ExPlayer player, bool shouldGrantInventory, bool shouldResetInventory, bool hasEscaped, bool dropPreviousItems, List<ItemType> items, Dictionary<ItemType, ushort> ammo)
        {
            Player = player;

            ShouldResetInventory = shouldResetInventory;
            ShouldGrantInventory = shouldGrantInventory;

            HasEscaped = hasEscaped;

            DropPreviousItems = dropPreviousItems;

            Items = items;
            Ammo = ammo;
        }
    }
}