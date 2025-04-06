using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using UnityEngine;

namespace LabExtended.API.CustomItems;

/// <summary>
/// Represents a spawned custom item instance.
/// </summary>
public class CustomItemInstance
{
    /// <summary>
    /// Gets the associated player.
    /// </summary>
    public ExPlayer? Player { get; internal set; }
    
    /// <summary>
    /// Gets the item's custom data.
    /// </summary>
    public CustomItemData? CustomData { get; internal set; }
    
    /// <summary>
    /// Gets the associated item.
    /// </summary>
    public ItemBase? Item { get; internal set; }
    
    /// <summary>
    /// Gets the associated item pickup.
    /// </summary>
    public ItemPickupBase? Pickup { get; internal set; }
    
    /// <summary>
    /// Whether or not this custom item has an owner.
    /// </summary>
    public bool HasPlayer => Player is not null;
    
    /// <summary>
    /// Whether or not this custom item is dropped.
    /// </summary>
    public bool HasPickup => Pickup is not null;
    
    /// <summary>
    /// Whether or not this custom item is in inventory.
    /// </summary>
    public bool HasItem => Item is not null;
    
    /// <summary>
    /// Whether or not this item is currently being held.
    /// </summary>
    public bool IsHeld { get; internal set; }
    
    /// <summary>
    /// Gets the item's serial number.
    /// </summary>
    public ushort ItemSerial { get; internal set; }
    
    /// <summary>
    /// Called once an item spawns (or is added to inventory).
    /// </summary>
    public virtual void OnEnabled() { }
    
    /// <summary>
    /// Called once destroyed.
    /// </summary>
    public virtual void OnDisabled() { }

    /// <summary>
    /// Called when a player tries to select this item
    /// </summary>
    /// <param name="previousItem">The item that's currently being held.</param>
    /// <returns>true if the player should be allowed to switch their item.</returns>
    public virtual bool OnSelecting(ItemBase? previousItem) => true;
    
    /// <summary>
    /// Called when a player selects this item.
    /// </summary>
    public virtual void OnSelected() { }
    
    /// <summary>
    /// Called when a player tries to unselect this item.
    /// </summary>
    /// <param name="newItem">The item that's going to be held.</param>
    /// <returns>true if the player should be allowed to switch their item.</returns>
    public virtual bool OnDeselecting(ItemBase? newItem) => true;
    
    /// <summary>
    /// Called when a player unselects this item.
    /// </summary>
    public virtual void OnDeselected() { }

    /// <summary>
    /// Called when a player starts picking up this item.
    /// </summary>
    /// <param name="player">The player that is picking up this item.</param>
    /// <returns>true if the player should be allowed to pick the item up.</returns>
    public virtual bool OnPickingUp(ExPlayer player) => true;
    
    /// <summary>
    /// Called when a new player picks up this item.
    /// </summary>
    public virtual void OnPickedUp() { }

    /// <summary>
    /// Called once the player starts dropping this item.
    /// </summary>
    /// <param name="isThrow">Whether or not the item should be thrown.</param>
    /// <returns>true if the player should be able to drop the item.</returns>
    public virtual bool OnDropping(ref bool isThrow) => true;
    
    /// <summary>
    /// Called once the player drops this item.
    /// <param name="isThrow">Whether or not the item was thrown.</param>
    /// </summary>
    public virtual void OnDropped(bool isThrow) { }

    /// <summary>
    /// Called once the player attempts to throw this item.
    /// </summary>
    /// <returns>true if the player should be allowed to throw this item.</returns>
    public virtual bool OnThrowing(Rigidbody rigidbody, ref Vector3 startPosition, ref Vector3 velocity, ref Vector3 angularVelocity) => true;
    
    /// <summary>
    /// Called once this item gets thrown.
    /// </summary>
    public virtual void OnThrown() { }
    
    internal virtual void OnItemSet() { }
    internal virtual void OnPickupSet() { }

    internal virtual void SetItem(ItemBase item)
    {
        Pickup = null;
        Item = item;
        OnItemSet();
    }

    internal virtual void SetPickup(ItemPickupBase pickup)
    {
        Pickup = pickup;
        Item = null;
        OnPickupSet();
    }
}