using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;

namespace LabExtended.Patches.Fixes
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.ServerSendItems))]
    public static class AllowNullItemsPatch
    {
        public static bool Prefix(Inventory __instance)
        {
            var inv = __instance.UserInventory.Items;
            var array = new ItemIdentifier[inv.Count];
            var index = 0;

            foreach (var pair in __instance.UserInventory.Items)
            {
                array[index] = new ItemIdentifier(pair.Value?.ItemTypeId ?? ItemType.None, pair.Key);
                index++;
            }

            __instance.TargetRefreshItems(array);
            return false;
        }
    }
}