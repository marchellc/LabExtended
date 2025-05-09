using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using LabApi.Events.Arguments.PlayerEvents;
using LabExtended.Events.Player;
using LabExtended.Utilities;
using UnityEngine;
using PlayerDroppingItemEventArgs = LabExtended.Events.Player.PlayerDroppingItemEventArgs;
using PlayerThrowingItemEventArgs = LabExtended.Events.Player.PlayerThrowingItemEventArgs;

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
    /// Gets the item's tracker.
    /// </summary>
    public ItemTracker? Tracker { get; internal set; }

    /// <summary>
    /// Gets the associated item.
    /// </summary>
    public ItemBase? Item => Tracker?.Item;
    
    /// <summary>
    /// Gets the associated item pickup.
    /// </summary>
    public ItemPickupBase? Pickup => Tracker?.Pickup;
    
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
    public bool IsHeld => Tracker?.IsSelected ?? false;
    
    /// <summary>
    /// Gets the item's serial number.
    /// </summary>
    public ushort ItemSerial => Tracker?.ItemSerial ?? 0;
    
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
    public virtual void OnSelecting(PlayerSelectingItemEventArgs args) { }
    
    /// <summary>
    /// Called when a player selects this item.
    /// </summary>
    public virtual void OnSelected(PlayerSelectedItemEventArgs args) { }
    
    /// <summary>
    /// Called when a player tries to unselect this item.
    /// </summary>
    public virtual void OnDeselecting(PlayerSelectingItemEventArgs args) { }
    
    /// <summary>
    /// Called when a player unselects this item.
    /// </summary>
    public virtual void OnDeselected(PlayerSelectedItemEventArgs args) { }

    /// <summary>
    /// Called when a player starts picking up this item.
    /// </summary>
    public virtual void OnPickingUp(PlayerPickingUpItemEventArgs args) { }
    
    /// <summary>
    /// Called when a new player picks up this item.
    /// </summary>
    public virtual void OnPickedUp(PlayerPickedUpItemEventArgs args) { }

    /// <summary>
    /// Called once the player starts dropping this item.
    /// </summary>
    public virtual void OnDropping(PlayerDroppingItemEventArgs args) { }
    
    /// <summary>
    /// Called once the player drops this item.
    /// </summary>
    public virtual void OnDropped(PlayerDroppedItemEventArgs args) { }

    /// <summary>
    /// Called once the player attempts to throw this item.
    /// </summary>
    /// <returns>true if the player should be allowed to throw this item.</returns>
    public virtual void OnThrowing(PlayerThrowingItemEventArgs args) { }
    
    /// <summary>
    /// Called once this item gets thrown.
    /// </summary>
    public virtual void OnThrown(PlayerThrewItemEventArgs args) { }
}