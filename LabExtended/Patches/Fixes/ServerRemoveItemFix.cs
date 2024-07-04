using Common.Extensions;

using HarmonyLib;

using InventorySystem;
using InventorySystem.Items.Pickups;

using System.Reflection;

namespace LabExtended.Patches.Fixes
{
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerRemoveItem))]
    public static class ServerRemoveItemFix
    {
        private static readonly EventInfo _event = typeof(InventoryExtensions).Event("OnItemRemoved");

        public static bool Prefix(Inventory inv, ushort itemSerial, ItemPickupBase ipb)
        {
            if (inv.DestroyItemInstance(itemSerial, ipb, out var item))
            {
                if (itemSerial != 0 && itemSerial == inv.CurItem.SerialNumber)
                    inv.ServerSelectItem(0);

                inv.UserInventory.Items.Remove(itemSerial);
                inv.SendItemsNextFrame = true;

                try
                {
                    _event.Raise(null, inv._hub, item, ipb);
                }
                catch { }
            }

            return false;
        }
    }
}