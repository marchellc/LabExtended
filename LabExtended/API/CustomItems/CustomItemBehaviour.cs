using LabExtended.API.CustomItems.Behaviours;
using LabExtended.Extensions;

namespace LabExtended.API.CustomItems;

/// <summary>
/// Represents the base class for a behaviour component of a Custom Item.
/// </summary>
public class CustomItemBehaviour
{
    /// <summary>
    /// Whether or not the behaviour is enabled.
    /// </summary>
    public bool IsEnabled { get; internal set; }
    
    /// <summary>
    /// Gets the behaviour's handler.
    /// </summary>
    public CustomItemHandler? Handler { get; internal set; }
    
    /// <summary>
    /// Gets called once per frame.
    /// </summary>
    public virtual void OnUpdate() { }
    
    /// <summary>
    /// Gets called once the behaviour is enabled.
    /// </summary>
    public virtual void OnEnabled() { }
    
    /// <summary>
    /// Gets called once the behaviour is disabled.
    /// </summary>
    public virtual void OnDisabled() { }

    /// <summary>
    /// Destroys this behaviour instance.
    /// <param name="destroyItem">Whether or not to destroy the base item / pickup too.</param>
    /// </summary>
    public void Destroy(bool destroyItem = false)
    {
        if (Handler != null)
        {
            if (this is CustomItemInventoryBehaviour inventoryBehaviour)
            {
                if (destroyItem && inventoryBehaviour.Item != null)
                {
                    inventoryBehaviour.Item.DestroyItem();
                    inventoryBehaviour.Item = null;
                }
                
                Handler.DestroyItem(inventoryBehaviour);
            }
            else if (this is CustomItemPickupBehaviour pickupBehaviour)
            {
                if (destroyItem && pickupBehaviour.Pickup != null)
                {
                    pickupBehaviour.Pickup.DestroySelf();
                    pickupBehaviour.Pickup = null;
                }
                
                Handler.DestroyPickup(pickupBehaviour);
            }
        }
    }
}