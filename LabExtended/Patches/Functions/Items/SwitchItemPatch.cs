using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;

using LabApi.Features.Wrappers;

using LabExtended.API;

using LabExtended.Core;
using LabExtended.Attributes;
using LabExtended.Utilities;

using LabExtended.Events;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Functions.Items
{
    public static class SwitchItemPatch
    {
        [EventPatch(typeof(PlayerSelectingItemEventArgs), true)]
        [EventPatch(typeof(PlayerSelectedItemEventArgs), true)]
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

                ItemBase curItem = __instance.UserInventory.Items.TryGetValue(__instance.CurItem.SerialNumber, out var curItemBase) ? curItemBase : null;
                ItemBase newItem = itemSerial != 0 && __instance.UserInventory.Items.TryGetValue(itemSerial, out var newItemBase) ? newItemBase : null;

                var prevIdentifier = __instance.NetworkCurItem;
                
                var switchingArgs = new PlayerSelectingItemEventArgs(player,
                    curItem != null ? Item.Get(curItem) : null,
                    newItem != null ? Item.Get(newItem) : null);

                if (!ExPlayerEvents.OnSelectingItem(switchingArgs))
                    return false;
                
                ItemTracker curTracker = null;
                ItemTracker newTracker = null;
                
                if (curItem != null)
                    ItemTracker.Trackers.TryGetValue(curItem.ItemSerial, out curTracker);

                if (curTracker?.CustomItem != null)
                {
                    curTracker.CustomItem.OnDeselecting(switchingArgs);

                    if (!switchingArgs.IsAllowed)
                        return false;
                }
                
                if (__instance.CurInstance != null && player.Inventory.Snake.Keycard != null && __instance.CurInstance == player.Inventory.Snake.Keycard)
                    player.Inventory.Snake.Reset(false, true);

                itemSerial = switchingArgs.NextItem?.Serial ?? 0;

                var flag = __instance.CurItem.SerialNumber == 0 || __instance.UserInventory.Items.TryGetValue(__instance.CurItem.SerialNumber, out curItem) && __instance.CurInstance != null;

                if (itemSerial == 0 || itemSerial == newItem?.ItemSerial || __instance.UserInventory.Items.TryGetValue(itemSerial, out newItem))
                {
                    if ((__instance.CurItem.SerialNumber != 0 && flag && !curItem.AllowHolster))
                        return false;

                    if (newItem is not null)
                        ItemTracker.Trackers.TryGetValue(newItem.ItemSerial, out newTracker);

                    if (newTracker?.CustomItem != null)
                    {
                        newTracker.CustomItem.OnSelecting(switchingArgs);

                        if (!switchingArgs.IsAllowed)
                            return false;
                    }

                    if (itemSerial == 0)
                    {
                        __instance.NetworkCurItem = ItemIdentifier.None;
                        __instance.CurInstance = null;

                        curTracker?.SetSelected(false);
                    }
                    else
                    {
                        __instance.NetworkCurItem = new ItemIdentifier(newItem.ItemTypeId, itemSerial);
                        __instance.CurInstance = newItem;
                        
                        curTracker?.SetSelected(false);
                        newTracker?.SetSelected(true);
                    }
                }
                else if (!flag)
                {
                    if (curTracker?.CustomItem != null)
                    {
                        curTracker.CustomItem.OnDeselecting(switchingArgs);

                        if (!switchingArgs.IsAllowed)
                            return false;
                    }
                    
                    __instance.CurItem = ItemIdentifier.None;
                    __instance.CurInstance = null;
                    
                    curTracker?.SetSelected(false);
                }
                
                ExPlayerEvents.OnSelectedItem(new (player, curItem, newItem, prevIdentifier,
                    newItem?.ItemId ?? ItemIdentifier.None));
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