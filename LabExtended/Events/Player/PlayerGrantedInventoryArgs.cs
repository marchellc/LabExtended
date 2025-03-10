using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Called when a player's starting inventory is granted.
    /// </summary>
    public class PlayerGrantedInventoryArgs
    {
        /// <summary>
        /// The player who's starting inventory has been granted.
        /// </summary>
        public ExPlayer? Player { get; }

        /// <summary>
        /// Whether or not the player has escaped.
        /// </summary>
        public bool HasEscaped { get; }

        /// <summary>
        /// A list of all granted items.
        /// </summary>
        public List<ItemBase> GrantedItems { get; }

        /// <summary>
        /// A list of all dropped items.
        /// </summary>
        public List<ItemPickupBase> KeptItems { get; }

        /// <summary>
        /// A dictionary of added ammo.
        /// </summary>
        public Dictionary<ItemType, ushort>? Ammo { get; }

        internal PlayerGrantedInventoryArgs(ExPlayer? player, bool hasEscaped, List<ItemBase> items, List<ItemPickupBase> keptItems, Dictionary<ItemType, ushort>? ammo)
        {
            Player = player;

            HasEscaped = hasEscaped;

            GrantedItems = items;
            KeptItems = keptItems;

            Ammo = ammo;
        }
    }
}
