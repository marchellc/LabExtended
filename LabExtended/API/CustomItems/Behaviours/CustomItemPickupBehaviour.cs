using InventorySystem.Items.Pickups;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;
using LabExtended.Events.Map;
using PlayerThrowingItemEventArgs = LabExtended.Events.Player.PlayerThrowingItemEventArgs;

namespace LabExtended.API.CustomItems.Behaviours;

/// <summary>
/// Represents the behaviour of a pickup Custom Item.
/// </summary>
public class CustomItemPickupBehaviour : CustomItemBehaviour
{
    /// <summary>
    /// Gets the target pickup.
    /// </summary>
    public ItemPickupBase? Pickup { get; internal set; }

    /// <summary>
    /// Gets the player that dropped this pickup.
    /// <remarks>Will be null if the pickup was spawned with a null owner.</remarks>
    /// </summary>
    public ExPlayer? Player { get; internal set; }
    
    /// <summary>
    /// Gets called once the pickup is spawned.
    /// <param name="item">The item that was dropped, null if spawned by code.</param>
    /// </summary>
    public virtual void OnSpawned(CustomItemInventoryBehaviour? item = null) { }
    
    /// <summary>
    /// Gets called when a player starts picking up this item.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnPickingUp(PlayerPickingUpItemEventArgs args) { }
    
    /// <summary>
    /// Gets called when a player picks up this item.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    /// <param name="item">The item behaviour that was created.</param>
    public virtual void OnPickedUp(PlayerPickedUpItemEventArgs args, CustomItemInventoryBehaviour item) { }
    
    /// <summary>
    /// Gets called when a player starts throwing the pickup.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnThrowing(PlayerThrowingItemEventArgs args) { }
    
    /// <summary>
    /// Gets called once the player throws this item.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnThrown(PlayerThrewItemEventArgs args) { }
    
    /// <summary>
    /// Gets called once a pickup collides.
    /// <remarks>Only called for pickups that inherit from <see cref="CollisionDetectionPickup"/></remarks>
    /// </summary>
    /// <param name="args">The event arguments</param>
    public virtual void OnCollided(PickupCollidedEventArgs args) { }
    
    /// <summary>
    /// Gets called before the pickup is upgraded in SCP-914.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnUpgrading(Scp914ProcessingPickupEventArgs args) { }
    
    /// <summary>
    /// Gets called once a pickup gets upgraded by SCP-914.
    /// </summary>
    /// <param name="args"></param>
    public virtual void OnUpgraded(Scp914ProcessedPickupEventArgs args) { }
}