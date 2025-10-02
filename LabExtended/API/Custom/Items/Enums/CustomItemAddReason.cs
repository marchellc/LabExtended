namespace LabExtended.API.Custom.Items.Enums
{
    /// <summary>
    /// Describes the reason why a custom item was added to a player's inventory.
    /// </summary>
    public enum CustomItemAddReason
    {
        /// <summary>
        /// The player received up a previously spawned custom item either by picking it up or by receiving it via the <see cref="CustomItem.GiveItem(InventorySystem.Items.Pickups.ItemPickupBase, ExPlayer, object?, bool)"/> method.
        /// </summary>
        PickedUp,

        /// <summary>
        /// The player received the custom item through a command or plugin using the <see cref="CustomItem.AddItem(ExPlayer, object?)"/>.
        /// </summary>
        Added,

        /// <summary>
        /// The item's owner was changed using the <see cref="CustomItem.TransferItem(InventorySystem.Items.ItemBase, ExPlayer)"/> method.
        /// </summary>
        Transferred
    }
}