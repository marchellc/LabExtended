using InventorySystem;

namespace LabExtended.Extensions
{
    public static class HubExtensions
    {
        public static void ClearInventory(this ReferenceHub hub)
        {
            while (hub.inventory.UserInventory.Items.Count > 0)
                hub.inventory.ServerRemoveItem(hub.inventory.UserInventory.Items.ElementAt(0).Key, null);
        }

        public static void RemoveItems(this ReferenceHub hub, ItemType itemType, int toRemove)
        {
            if (!hub.inventory.UserInventory.Items.Any(item => item.Value.ItemTypeId == itemType))
                return;

            var curAmount = hub.inventory.UserInventory.Items.Count(item => item.Value.ItemTypeId == itemType);
            var newAmount = 0;

            if (curAmount < toRemove)
                newAmount = 0;
            else
                newAmount = curAmount - toRemove;

            while (hub.inventory.UserInventory.Items.Count(item => item.Value.ItemTypeId == itemType) != newAmount)
                hub.inventory.ServerRemoveItem(hub.inventory.UserInventory.Items.First(item => item.Value.ItemTypeId == itemType).Key, null);
        }
    }
}