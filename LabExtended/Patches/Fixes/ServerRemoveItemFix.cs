using HarmonyLib;

using InventorySystem;
using InventorySystem.Items.Pickups;

namespace LabExtended.Patches.Fixes
{
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerRemoveItem))]
    public static class ServerRemoveItemFix
    {
        public static bool Prefix(Inventory inv, ushort itemSerial, ItemPickupBase ipb)
        {
            if (inv.DestroyItemInstance(itemSerial, ipb, out var item))
            {
                if (itemSerial != 0 && itemSerial == inv.CurItem.SerialNumber)
                    inv.ServerSelectItem(0);

                inv.UserInventory.Items.Remove(itemSerial);
                inv.SendItemsNextFrame = true;
            }

            return false;
        }
    }
}