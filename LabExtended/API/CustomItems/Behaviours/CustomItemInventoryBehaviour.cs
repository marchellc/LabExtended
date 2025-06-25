using InventorySystem.Items; 
using InventorySystem.Items.ToggleableLights;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

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
    /// Whether or not this item's light is enabled.
    /// <remarks>Applicable only to the Flashlight and Lantern.</remarks>
    /// </summary>
    public bool IsLightEnabled
    {
        get
        {
            if (Item is not ToggleableLightItemBase lightItem)
                return false;

            return lightItem.IsEmittingLight;
        }
        set
        {
            if (Player is null)
                return;
            
            if (Item is not ToggleableLightItemBase lightItem)
                return;

            if (lightItem.IsEmittingLight == value)
                return;
            
            lightItem.IsEmittingLight = value;
            
            Player.Send(new FlashlightNetworkHandler.FlashlightMessage(lightItem.ItemSerial, value));
        }
    }

    /// <summary>
    /// Selects this item.
    /// </summary>
    /// <returns>true if the item was selected</returns>
    public bool Select()
        => !IsSelected && Item != null && Player?.Inventory != null && Player.Inventory.Select(Item);

    /// <summary>
    /// Drops this item.
    /// </summary>
    /// <returns>The newly created pickup.</returns>
    public virtual CustomItemPickupBehaviour Drop()
        => Handler.DropItem(this);

    /// <summary>
    /// Drops this item at the specified position.
    /// </summary>
    /// <param name="position">The position to drop the item at.</param>
    /// <param name="rotation">The rotation of the dropped item.</param>
    /// <returns>The newly created pickup.</returns>
    public virtual CustomItemPickupBehaviour Spawn(Vector3 position, Quaternion? rotation = null)
        => Handler.SpawnItem(this, position, rotation);

    /// <summary>
    /// Throws this item.
    /// </summary>
    /// <param name="force">Throw force.</param>
    /// <returns>The newly created pickup.</returns>
    public virtual CustomItemPickupBehaviour Throw(float force = 1f)
        => Handler.ThrowItem(this, force);

    /// <summary>
    /// Gives this item to another player.
    /// </summary>
    /// <param name="target">The player to give the item to.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual bool Give(ExPlayer target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (Item == null)
            throw new Exception(
                "This Custom Item cannot be given to other players as it's Item has already been destroyed.");

        if (Player != null && Player == target)
            return false;
        
        OnRemoved(null);
        
        Item.TransferItem(target.ReferenceHub);

        Player = target;
        
        OnAdded(null);
        return true;
    }
    
    /// <summary>
    /// Gets called once the item is added to a player's inventory.
    /// <param name="pickup">The behaviour of the pickup which this item was picked up, null if given by code.</param>
    /// </summary>
    public virtual void OnAdded(CustomItemPickupBehaviour? pickup = null) { }
    
    /// <summary>
    /// Gets called once the item is removed from a player's inventory.
    /// </summary>
    /// <param name="pickup">The behaviour of the pickup which this item was picked up, null if removed by code.</param>
    public virtual void OnRemoved(CustomItemPickupBehaviour? pickup = null) { }
    
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
    
    /// <summary>
    /// Gets called before a player toggles their light.
    /// <remarks>Called only for the Flashlight and Lantern.</remarks>
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnTogglingLight(PlayerTogglingFlashlightEventArgs args) { }
    
    /// <summary>
    /// Gets called once the player toggles their light.
    /// <remarks>Called only for the Flashlight and Lantern.</remarks>
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnToggledLight(PlayerToggledFlashlightEventArgs args) { }
    
    /// <summary>
    /// Gets called before a player flips their coin.
    /// <remarks>Called only for the Coin item.</remarks>
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnFlippingCoin(PlayerFlippingCoinEventArgs args) { }
    
    /// <summary>
    /// Gets called once a player flips their coin.
    /// <remarks>Called only for the Coin item.</remarks>
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnFlippedCoin(PlayerFlippedCoinEventArgs args) { }
    
    /// <summary>
    /// Gets called before the item gets upgraded in SCP-914.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnUpgrading(Scp914ProcessingInventoryItemEventArgs args) { }
    
    /// <summary>
    /// Gets called once the item is upgraded by SCP-914.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnUpgraded(Scp914ProcessedInventoryItemEventArgs args) { }
    
    /// <summary>
    /// Gets called when a player starts disarming a player.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnDisarming(PlayerCuffingEventArgs args) { }
    
    /// <summary>
    /// Gets called after a player disarms another player.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnDisarmed(PlayerCuffedEventArgs args) { }
    
    /// <summary>
    /// Gets called when the item's owner starts escaping.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnEscaping(PlayerEscapingEventArgs args) { }
    
    /// <summary>
    /// Gets called when the item's owner succesfully escapes.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnEscaped(PlayerEscapedEventArgs args) { }
    
    /// <summary>
    /// Gets called when the item's owner receives damage (before the damage is applied).
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnHurting(PlayerHurtingEventArgs args) { }
    
    /// <summary>
    /// Gets called when the item's owner receives damage.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnHurt(PlayerHurtEventArgs args) { }
    
    /// <summary>
    /// Gets called when the item's owner receives mortal damage (before the player is set to spectator).
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnDying(PlayerDyingEventArgs args) { }
    
    /// <summary>
    /// Gets called when the item's owner dies.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnDied(PlayerDeathEventArgs args) { }
    
    /// <summary>
    /// Gets called before the item's owner changes role.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnChangingRole(PlayerChangingRoleEventArgs args) { }
    
    /// <summary>
    /// Gets called after the item's owner role is changed.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnChangedRole(PlayerChangedRoleEventArgs args) { }
}