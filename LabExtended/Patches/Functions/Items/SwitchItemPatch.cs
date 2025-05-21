using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;

using LabApi.Features.Wrappers;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Behaviours;

using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Player;

using NorthwoodLib.Pools;

namespace LabExtended.Patches.Functions.Items
{
    public static class SwitchItemPatch
    {
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

                ItemBase curItem =
                    __instance.UserInventory.Items.TryGetValue(__instance.CurItem.SerialNumber, out var curItemBase)
                        ? curItemBase
                        : null;
                
                ItemBase newItem =
                    itemSerial != 0 && __instance.UserInventory.Items.TryGetValue(itemSerial, out var newItemBase)
                        ? newItemBase
                        : null;

                var prevIdentifier = __instance.NetworkCurItem;

                var switchingArgs = new PlayerSelectingItemEventArgs(player,
                    curItem != null ? Item.Get(curItem) : null,
                    newItem != null ? Item.Get(newItem) : null);

                if (!ExPlayerEvents.OnSelectingItem(switchingArgs))
                    return false;

                ItemTracker? curTracker = curItem?.GetTracker();
                ItemTracker? newTracker = newItem?.GetTracker();

                var currentBehaviours = ListPool<CustomItemInventoryBehaviour>.Shared.Rent();
                var newBehaviours = ListPool<CustomItemInventoryBehaviour>.Shared.Rent();

                if (curItem != null)
                    CustomItemUtils.GetInventoryBehavioursNonAlloc(curItem.ItemSerial, currentBehaviours);

                if (newItem != null)
                    CustomItemUtils.GetInventoryBehavioursNonAlloc(newItem.ItemSerial, newBehaviours);

                if (__instance.CurInstance != null && player.Inventory.Snake.Keycard != null &&
                    __instance.CurInstance == player.Inventory.Snake.Keycard)
                    player.Inventory.Snake.Reset(false, true);

                itemSerial = switchingArgs.NextItem?.Serial ?? 0;

                var flag = __instance.CurItem.SerialNumber == 0 ||
                           __instance.UserInventory.Items.TryGetValue(__instance.CurItem.SerialNumber, out curItem) &&
                           __instance.CurInstance != null;

                if (itemSerial == 0 || itemSerial == newItem?.ItemSerial ||
                    __instance.UserInventory.Items.TryGetValue(itemSerial, out newItem))
                {
                    if ((__instance.CurItem.SerialNumber != 0 && flag && !curItem.AllowHolster))
                    {
                        ListPool<CustomItemInventoryBehaviour>.Shared.Return(currentBehaviours);
                        ListPool<CustomItemInventoryBehaviour>.Shared.Return(newBehaviours);

                        return false;
                    }

                    if (itemSerial == 0)
                    {
                        for (var i = 0; i < currentBehaviours.Count; i++)
                            currentBehaviours[i].OnUnselecting(switchingArgs);

                        if (!switchingArgs.IsAllowed)
                        {
                            ListPool<CustomItemInventoryBehaviour>.Shared.Return(currentBehaviours);
                            ListPool<CustomItemInventoryBehaviour>.Shared.Return(newBehaviours);

                            return false;
                        }

                        __instance.NetworkCurItem = ItemIdentifier.None;
                        __instance.CurInstance = null;

                        curTracker?.SetSelected(false);

                        var selectedArgs = new PlayerSelectedItemEventArgs(player, curItem, newItem, prevIdentifier,
                            newItem?.ItemId ?? ItemIdentifier.None);

                        ExPlayerEvents.OnSelectedItem(selectedArgs);

                        for (var i = 0; i < currentBehaviours.Count; i++)
                        {
                            var behaviour = currentBehaviours[i];

                            behaviour.IsSelected = false;
                            behaviour.OnUnselected(selectedArgs);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < currentBehaviours.Count; i++)
                            currentBehaviours[i].OnUnselecting(switchingArgs);

                        if (!switchingArgs.IsAllowed)
                        {
                            ListPool<CustomItemInventoryBehaviour>.Shared.Return(currentBehaviours);
                            ListPool<CustomItemInventoryBehaviour>.Shared.Return(newBehaviours);

                            return false;
                        }

                        for (var i = 0; i < newBehaviours.Count; i++)
                            newBehaviours[i].OnSelecting(switchingArgs);

                        if (!switchingArgs.IsAllowed)
                        {
                            ListPool<CustomItemInventoryBehaviour>.Shared.Return(currentBehaviours);
                            ListPool<CustomItemInventoryBehaviour>.Shared.Return(newBehaviours);

                            return false;
                        }

                        __instance.NetworkCurItem = new ItemIdentifier(newItem.ItemTypeId, itemSerial);
                        __instance.CurInstance = newItem;

                        curTracker?.SetSelected(false);
                        newTracker?.SetSelected(true);

                        var selectedArgs = new PlayerSelectedItemEventArgs(player, curItem, newItem, prevIdentifier,
                            newItem?.ItemId ?? ItemIdentifier.None);

                        ExPlayerEvents.OnSelectedItem(selectedArgs);

                        for (var i = 0; i < currentBehaviours.Count; i++)
                        {
                            var behaviour = currentBehaviours[i];

                            behaviour.IsSelected = false;
                            behaviour.OnUnselected(selectedArgs);
                        }

                        for (var i = 0; i < newBehaviours.Count; i++)
                        {
                            var behaviour = newBehaviours[i];

                            behaviour.IsSelected = true;
                            behaviour.OnSelected(selectedArgs);
                        }
                    }
                }
                else if (!flag)
                {
                    for (var i = 0; i < currentBehaviours.Count; i++)
                        currentBehaviours[i].OnUnselecting(switchingArgs);

                    if (!switchingArgs.IsAllowed)
                    {
                        ListPool<CustomItemInventoryBehaviour>.Shared.Return(currentBehaviours);
                        ListPool<CustomItemInventoryBehaviour>.Shared.Return(newBehaviours);

                        return false;
                    }
                    
                    __instance.CurItem = ItemIdentifier.None;
                    __instance.CurInstance = null;

                    curTracker?.SetSelected(false);

                    var selectedArgs = new PlayerSelectedItemEventArgs(player, curItem, newItem, prevIdentifier,
                        newItem?.ItemId ?? ItemIdentifier.None);

                    ExPlayerEvents.OnSelectedItem(selectedArgs);
                    
                    for (var i = 0; i < currentBehaviours.Count; i++)
                    {
                        var behaviour = currentBehaviours[i];

                        behaviour.IsSelected = false;
                        behaviour.OnUnselected(selectedArgs);
                    }
                }

                ListPool<CustomItemInventoryBehaviour>.Shared.Return(currentBehaviours);
                ListPool<CustomItemInventoryBehaviour>.Shared.Return(newBehaviours);
                
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