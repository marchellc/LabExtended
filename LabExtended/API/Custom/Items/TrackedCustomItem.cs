using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.Extensions;

namespace LabExtended.API.Custom.Items
{
    /// <summary>
    /// Used to track custom item changes.
    /// </summary>

    public class TrackedCustomItem
    {
        /// <summary>
        /// Gets the custom item that created this tracker.
        /// </summary>
        public CustomItem TargetItem { get; }

        /// <summary>
        /// Gets the serial targeted by this tracker.
        /// </summary>
        public ushort TargetSerial { get; }

        /// <summary>
        /// Gets or sets the player who currently holds the item in their inventory.
        /// </summary>
        public ExPlayer? Owner { get; set; }

        /// <summary>
        /// Gets or sets the active inventory item instance.
        /// </summary>
        public ItemBase? Item { get; set; }

        /// <summary>
        /// Gets or sets the active pickup instance.
        /// </summary>
        public ItemPickupBase? Pickup { get; set; }

        /// <summary>
        /// Gets or sets the custom data.
        /// </summary>
        public object? Data;

        /// <summary>
        /// Whether or not the item is currently selected by the player.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                if (Item == null)
                    return false;

                if (Owner?.ReferenceHub == null)
                    return false;

                if (Owner.Inventory.CurrentItemIdentifier.SerialNumber != Item.ItemSerial)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the TrackedCustomItem class, associating a custom item with its serial number,
        /// owner, related item references, and optional metadata.
        /// </summary>
        /// <param name="targetItem">The custom item to be tracked. Cannot be null.</param>
        /// <param name="targetSerial">The serial number that uniquely identifies the target item.</param>
        /// <param name="owner">The player who owns the tracked item, or null if the item is not owned by any player.</param>
        /// <param name="item">The base item reference associated with the tracked custom item, or null if not applicable.</param>
        /// <param name="pickup">The pickup reference for the tracked item, or null if the item is not currently picked up.</param>
        /// <param name="data">Optional metadata or additional data associated with the tracked item. Can be null.</param>
        public TrackedCustomItem(CustomItem targetItem, ushort targetSerial, ExPlayer? owner, ItemBase? item, ItemPickupBase? pickup, object? data)
        {
            TargetItem = targetItem;
            TargetSerial = targetSerial;
            Owner = owner;
            Item = item;
            Pickup = pickup;
            Data = data;
        }

        /// <summary>
        /// Validates the tracker by ensuring that either the associated item or pickup is set based on the current
        /// serial number.
        /// </summary>
        /// <remarks>If both the item and pickup are unset, this method attempts to locate and assign them
        /// using the target serial number. Subsequent calls will return true unless both remain unset and cannot be
        /// found.</remarks>
        /// <returns>true if the tracker is successfully validated and the item or pickup is found; otherwise, false.</returns>
        public bool ValidateTracker()
        {
            if (Item == null && Pickup == null)
            {
                if (InventoryExtensions.ServerTryGetItemWithSerial(TargetSerial, out var item))
                {
                    Pickup = null;
                    Item = item;

                    if (ExPlayer.TryGet(item.Owner, out var owner))
                    {
                        Owner = owner;

                        if (!owner.Inventory.ownedCustomItems.ContainsKey(TargetSerial))
                            owner.Inventory.ownedCustomItems.Add(TargetSerial, TargetItem);

                        if (owner.Inventory.CurrentCustomItem is null
                            && owner.Inventory.CurrentItemIdentifier.SerialNumber == item.ItemSerial)
                            owner.Inventory.CurrentCustomItem = TargetItem;
                    }

                    return true;
                }
               
                if (ExMap.Pickups.TryGetFirst(x => x != null && x.Info.Serial == TargetSerial, out var pickup))
                {
                    Item = null;
                    Owner = null;

                    Pickup = pickup;
                    return true;
                }

                return false;
            }

            return true;
        }
    }
}