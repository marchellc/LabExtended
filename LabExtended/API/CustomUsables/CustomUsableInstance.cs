using InventorySystem.Items.Usables;

using LabExtended.API.CustomItems;

namespace LabExtended.API.CustomUsables;

/// <summary>
/// Represents a custom usable item.
/// </summary>
public class CustomUsableInstance : CustomItemInstance
{
    /// <summary>
    /// Gets the usable item.
    /// </summary>
    public new UsableItem? Item => base.Item as UsableItem;
    
    /// <summary>
    /// Gets the usable item configuration.
    /// </summary>
    public new CustomUsableData? CustomData => base.CustomData as CustomUsableData;

    /// <summary>
    /// Gets or sets the item's remaining cooldown.
    /// </summary>
    public float RemainingCooldown { get; set; }
    
    /// <summary>
    /// Gets or sets the item's remaining use time.
    /// </summary>
    public float RemainingTime { get; set; }

    /// <summary>
    /// Whether or not the item is currently being used.
    /// </summary>
    public bool IsUsing { get; internal set; }

    /// <summary>
    /// Called when a player tries to use this item.
    /// </summary>
    /// <returns>true if the player should be allowed to use this item.</returns>
    public virtual bool OnStartUsing() => true;
    
    /// <summary>
    /// Called when a player starts using this item.
    /// </summary>
    public virtual void OnStartedUsing() { }

    /// <summary>
    /// Called when the player tries to cancel item usage.
    /// </summary>
    /// <returns>true if the player should be allowed to cancel this item's usage.</returns>
    public virtual bool OnCancelling() => true;
    
    /// <summary>
    /// Called when the player cancels this item's usage.
    /// </summary>
    public virtual void OnCancelled() { }
    
    /// <summary>
    /// Called when the player completes using this item.
    /// </summary>
    public virtual void OnCompleted() { }

    /// <summary>
    /// Sends a cooldown message to the player.
    /// </summary>
    /// <param name="remainingCooldown">The remaining cooldown.</param>
    public void SendCooldown(float remainingCooldown = 0f)
        => Player?.Send(new ItemCooldownMessage(ItemSerial, remainingCooldown));
}