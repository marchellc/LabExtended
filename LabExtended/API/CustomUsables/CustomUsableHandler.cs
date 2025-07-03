using InventorySystem.Items.Usables;

using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Behaviours;

using LabExtended.API.CustomUsables.Behaviours;
using LabExtended.API.CustomUsables.Properties;

namespace LabExtended.API.CustomUsables;

/// <summary>
/// Custom Item handler targeting usable items.
/// </summary>
public abstract class CustomUsableHandler : CustomItemHandler
{
    /// <inheritdoc cref="CustomItemHandler.InventoryProperties"/>
    public CustomUsableInventoryProperties UsableInventoryProperties => InventoryProperties as CustomUsableInventoryProperties;

    internal override void InternalInitializeItem(CustomItemInventoryBehaviour item, CustomItemPickupBehaviour? pickup)
    {
        base.InternalInitializeItem(item, pickup);

        if (UsableInventoryProperties != null && item.Item is UsableItem usableItem)
        {
            if (UsableInventoryProperties.UseDuration == -1f)
                UsableInventoryProperties.UseDuration = usableItem.UseTime;

            if (item is CustomUsableInventoryBehaviour usableInventory)
            {
                usableInventory.UseDuration = UsableInventoryProperties.UseDuration;
                usableInventory.CooldownDuration = UsableInventoryProperties.CooldownDuration;
            }
        }
    }
}