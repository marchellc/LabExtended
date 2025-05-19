using HarmonyLib;

using InventorySystem;
using InventorySystem.Items.Pickups;

namespace LabExtended.Patches.Fixes;

/// <summary>
/// Fixes setting current item to none if the destroyed item is the one currently held.
/// </summary>
public static class ServerRemoveItemFix
{
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerRemoveItem))]
    private static bool Prefix(Inventory inv, ushort itemSerial, ItemPickupBase ipb)
    {
        if (inv.DestroyItemInstance(itemSerial, ipb, out _))
        {
            if (itemSerial != 0 && itemSerial == inv.CurItem.SerialNumber)
                inv.ServerSelectItem(0);

            inv.UserInventory.Items.Remove(itemSerial);
            inv.SendItemsNextFrame = true;
        }

        return false;
    }
}