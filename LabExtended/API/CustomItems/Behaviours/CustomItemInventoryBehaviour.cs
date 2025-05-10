using InventorySystem.Items;

using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.Events.Player;

using PlayerDroppingItemEventArgs = LabExtended.Events.Player.PlayerDroppingItemEventArgs;

namespace LabExtended.API.CustomItems.Behaviours;

/// <summary>
/// Represents the behaviour of an inventory Custom Item.
/// </summary>
public class CustomItemInventoryBehaviour : CustomItemBehaviour
{
    /// <summary>
    /// Gets the inventory item.
    /// </summary>
    public ItemBase? Item { get; internal set; }
    
    /// <summary>
    /// Gets the player that owns the inventory item.
    /// </summary>
    public ExPlayer? Player { get; internal set; }
    
    /// <summary>
    /// Whether or not the item is currently selected.
    /// </summary>
    public bool IsSelected { get; internal set; }

    /// <summary>
    /// Selects this item.
    /// </summary>
    /// <returns>true if the item was selected</returns>
    public bool Select()
    {
        if (IsSelected)
            return false;

        if (Item == null)
            return false;

        if (Player is null)
            return false;
        
        Player.ReferenceHub.inventory.ServerSelectItem(Item.ItemSerial);
        return true;
    }
    
    /// <summary>
    /// Gets called once the item is added to a player's inventory.
    /// <param name="pickup">The behaviour of the pickup which this item was picked up, null if given by code.</param>
    /// </summary>
    public virtual void OnAdded(CustomItemPickupBehaviour? pickup = null) { }
    
    /// <summary>
    /// Gets called once the player starts dropping this item.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnDropping(PlayerDroppingItemEventArgs args) { }
    
    /// <summary>
    /// Gets called once the player drops this item.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    /// <param name="pickup">The pickup that was spawned.</param>
    public virtual void OnDropped(PlayerDroppedItemEventArgs args, CustomItemPickupBehaviour pickup) { }
    
    /// <summary>
    /// Gets called once this item is being selected.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnSelecting(PlayerSelectingItemEventArgs args) { }
    
    /// <summary>
    /// Gets called once this item is selected.
    /// </summary>
    /// <param name="args"></param>
    public virtual void OnSelected(PlayerSelectedItemEventArgs args) { }
    
    /// <summary>
    /// Gets called once the player starts selecting a different item.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnUnselecting(PlayerSelectingItemEventArgs args) { }
    
    /// <summary>
    /// Gets called once the player selects a different item.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnUnselected(PlayerSelectedItemEventArgs args) { }
}