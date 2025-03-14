using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;

using LabExtended.API;
using LabExtended.API.CustomItems;

using LabExtended.Core;
using LabExtended.Attributes;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Functions.Items
{
    public static class SwitchItemPatch
    {
        [EventPatch(typeof(PlayerSelectingItemArgs), true)]
        [EventPatch(typeof(PlayerSelectedItemArgs), true)]
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

                CustomItemInstance curCustomItem = null;
                CustomItemInstance newCustomItem = null;
                
                if (curItem != null)
                    CustomItemManager.InventoryItems.TryGetValue(curItem, out curCustomItem);
                
                if (itemSerial == 0 || __instance.UserInventory.Items.TryGetValue(itemSerial, out newItem))
                {
                    if ((__instance.CurItem.SerialNumber != 0 && flag && !curItem.AllowHolster))
                        return false;
                    
                    if (newItem is not null)
                        CustomItemManager.InventoryItems.TryGetValue(newItem, out newCustomItem);

                    if (curCustomItem != null && !curCustomItem.OnDeselecting(curItem))
                        return false;

                    if (newCustomItem != null && !newCustomItem.OnSelecting(newItem))
                        return false;

                    if (itemSerial == 0)
                    {
                        __instance.NetworkCurItem = ItemIdentifier.None;
                        __instance.CurInstance = null;

                        if (curCustomItem != null)
                        {
                            curCustomItem.IsHeld = false;
                            curCustomItem.OnDeselected();

                            player.Inventory.heldCustomItem = null;
                        }
                    }
                    else
                    {
                        __instance.NetworkCurItem = new ItemIdentifier(newItem.ItemTypeId, itemSerial);
                        __instance.CurInstance = newItem;

                        if (curCustomItem != null)
                        {
                            curCustomItem.IsHeld = false;
                            curCustomItem.OnDeselected();

                            player.Inventory.heldCustomItem = null;
                        }

                        if (newCustomItem != null)
                        {
                            newCustomItem.IsHeld = true;
                            newCustomItem.OnSelected();

                            player.Inventory.heldCustomItem = newCustomItem;
                        }
                    }
                }
                else if (!flag)
                {
                    if (curCustomItem != null && !curCustomItem.OnDeselecting(curItem))
                        return false;
                    
                    __instance.CurItem = ItemIdentifier.None;
                    __instance.CurInstance = null;
                    
                    if (curCustomItem != null)
                    {
                        curCustomItem.IsHeld = false;
                        curCustomItem.OnDeselected();

                        player.Inventory.heldCustomItem = null;
                    }
                }
                
                HookRunner.RunEvent(new PlayerSelectedItemArgs(player, curItem, newItem, prevIdentifier, __instance.CurItem));
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