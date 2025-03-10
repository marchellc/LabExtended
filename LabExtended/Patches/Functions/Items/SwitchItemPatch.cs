using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;

using LabExtended.API;
using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Hooking;

using LabExtended.Events.Player;

namespace LabExtended.Patches.Functions.Items
{
    public static class SwitchItemPatch
    {
        [HookPatch(typeof(PlayerSelectingItemArgs), true)]
        [HookPatch(typeof(PlayerSelectedItemArgs), true)]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.ServerSelectItem))]
        public static bool Prefix(Inventory __instance, ushort itemSerial)
        {
            try
            {
                if (itemSerial == __instance.CurItem.SerialNumber)
                    return false;

                if (!ExPlayer.TryGet(__instance._hub, out var player))
                    return false;

                if (!player.Toggles.CanSwitchItems)
                    return false;

                ItemBase curItem = null;
                ItemBase newItem = null;

                var prevIdentifier = __instance.NetworkCurItem;
                var switchingArgs = new PlayerSelectingItemArgs(player, __instance.CurInstance, itemSerial);

                if (!HookRunner.RunEvent(switchingArgs, true))
                    return false;

                itemSerial = switchingArgs.NextSerial;

                var flag = __instance.CurItem.SerialNumber == 0 || __instance.UserInventory.Items.TryGetValue(__instance.CurItem.SerialNumber, out curItem) && __instance.CurInstance != null;

                if (itemSerial == 0 || __instance.UserInventory.Items.TryGetValue(itemSerial, out newItem))
                {
                    if ((__instance.CurItem.SerialNumber != 0 && flag && !curItem.AllowHolster))
                        return false;

                    if (itemSerial == 0)
                    {
                        __instance.NetworkCurItem = ItemIdentifier.None;
                        __instance.CurInstance = null;
                    }
                    else
                    {
                        __instance.NetworkCurItem = new ItemIdentifier(newItem.ItemTypeId, itemSerial);
                        __instance.CurInstance = newItem;
                    }
                }
                else if (!flag)
                {
                    __instance.CurItem = ItemIdentifier.None;
                    __instance.CurInstance = null;
                }

                var switchedArgs = new PlayerSelectedItemArgs(player, curItem, newItem, prevIdentifier, __instance.CurItem);

                HookRunner.RunEvent(switchedArgs);
                return false;
            }
            catch (Exception ex)
            {
                ApiLog.Error("SwitchItemPatch", ex);
                return true;
            }
        }
    }
}