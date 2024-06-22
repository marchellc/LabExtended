using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Functions.Items
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.ServerSelectItem))]
    public static class SwitchItemPatch
    {
        public static bool Prefix(Inventory __instance, ushort itemSerial)
        {
            var player = ExPlayer.Get(__instance._hub);

            if (player is null)
                return true;

            if (!player.Switches.CanSwitchItems)
                return false;

            if (itemSerial == __instance.CurItem.SerialNumber)
                return false;

            // Hate this language sometimes ..
            ItemBase curItem = null;
            ItemBase newItem = null;

            var curIdentifier = __instance.CurItem;
            var flag = __instance.CurItem.SerialNumber == 0 || (__instance.UserInventory.Items.TryGetValue(__instance.CurItem.SerialNumber, out curItem) && __instance.CurInstance != null);
            var switchingItemsEv = new PlayerSelectingItemArgs(player, __instance.CurInstance, itemSerial);

            if (!HookRunner.RunCancellable(switchingItemsEv, true))
                return false;

            itemSerial = switchingItemsEv.NextSerial;

            if (itemSerial == 0 || __instance.UserInventory.Items.TryGetValue(itemSerial, out newItem))
            {
                if ((__instance.CurItem.SerialNumber != 0 && flag && !curItem.CanHolster()) || (itemSerial != 0 && !newItem.CanEquip()))
                    return false;

                if (itemSerial == 0)
                {
                    __instance.NetworkCurItem = ItemIdentifier.None;

                    if (!__instance.isLocalPlayer)
                        __instance.CurInstance = null;
                }
                else
                {
                    __instance.NetworkCurItem = new ItemIdentifier(newItem.ItemTypeId, itemSerial);

                    if (!__instance.isLocalPlayer)
                        __instance.CurInstance = null;
                }
            }
            else if (!flag)
            {
                __instance.NetworkCurItem = ItemIdentifier.None;

                if (!__instance.isLocalPlayer)
                    __instance.CurInstance = null;
            }

            HookRunner.RunEvent(new PlayerSelectedItemArgs(player, curItem, newItem, curIdentifier, __instance.CurItem));
            return false;
        }
    }
}