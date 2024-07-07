using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.Core;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Functions.Items
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.ServerSelectItem))]
    public static class SwitchItemPatch
    {
        public static bool Prefix(Inventory __instance, ushort itemSerial)
        {
            try
            {
                if (itemSerial == __instance.CurItem.SerialNumber)
                    return false;

                if (!ExPlayer.TryGet(__instance._hub, out var player))
                    return false;

                if (!player.Switches.CanSwitchItems)
                    return false;

                ItemBase curItem = null;
                ItemBase newItem = null;

                CustomItem curCustomItem = null;
                CustomItem nextCustomItem = null;

                var prevIdentifier = __instance.NetworkCurItem;
                var switchingArgs = new PlayerSelectingItemArgs(player, __instance.CurInstance, itemSerial);

                if (!HookRunner.RunCancellable(switchingArgs, true))
                    return false;

                itemSerial = switchingArgs.NextSerial;

                if (__instance.CurInstance != null && CustomItem.TryGetItem(__instance.CurInstance, out curCustomItem))
                {
                    curCustomItem.OnDeselecting(switchingArgs);

                    if (!switchingArgs.Cancellation)
                        return false;
                }

                var flag = __instance.CurItem.SerialNumber == 0 || __instance.UserInventory.Items.TryGetValue(__instance.CurItem.SerialNumber, out curItem) && __instance.CurInstance != null;

                itemSerial = switchingArgs.NextSerial;

                if (itemSerial == 0 || __instance.UserInventory.Items.TryGetValue(itemSerial, out newItem))
                {
                    if ((__instance.CurItem.SerialNumber != 0 && flag && !curItem.CanHolster()) || (itemSerial != 0 && !newItem.CanEquip()))
                        return false;

                    if (newItem != null && CustomItem.TryGetItem(newItem, out nextCustomItem))
                    {
                        nextCustomItem.OnSelecting(switchingArgs);

                        if (!switchingArgs.Cancellation)
                            return false;
                    }

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

                if (curCustomItem != null)
                {
                    curCustomItem.IsSelected = false;
                    curCustomItem.OnDeselected(switchedArgs);
                }

                if (nextCustomItem != null)
                {
                    nextCustomItem.IsSelected = true;
                    nextCustomItem.OnSelected(switchedArgs);
                }

                return false;
            }
            catch (Exception ex)
            {
                ExLoader.Error("SwitchItemPatch", ex);
                return true;
            }
        }
    }
}