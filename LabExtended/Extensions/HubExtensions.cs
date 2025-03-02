using InventorySystem;
using LabExtended.API;

namespace LabExtended.Extensions
{
    public static class HubExtensions
    {
        public static void ClearInventory(this ReferenceHub hub)
        {
            while (hub.inventory.UserInventory.Items.Count > 0 && ExServer.IsRunning)
                hub.inventory.ServerRemoveItem(hub.inventory.UserInventory.Items.ElementAt(0).Key, null);
        }

        public static void RemoveItems(this ReferenceHub hub, ItemType itemType, int toRemove)
        {
            if (hub.inventory.UserInventory.Items.Count(item => item.Value.ItemTypeId == itemType) < 1)
                return;

            var curAmount = hub.inventory.UserInventory.Items.Count(item => item.Value.ItemTypeId == itemType);
            var items = hub.inventory.UserInventory.Items.TakeWhere(Math.Min(curAmount, toRemove), p => p.Value.ItemTypeId == itemType);

            foreach (var item in items)
                hub.inventory.ServerRemoveItem(item.Key, item.Value.PickupDropModel);
        }
    }
}