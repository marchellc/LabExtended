using CustomPlayerEffects;

using HarmonyLib;

using InventorySystem.Items.Usables;

using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Usables;

using LabExtended.Extensions;

using PluginAPI.Events;

using Respawning;

using UnityEngine;

namespace LabExtended.Patches.Functions.Items
{
    [HarmonyPatch(typeof(UsableItemsController), nameof(UsableItemsController.Update))]
    public static class UpdateUsableItemPatch
    {
        public static bool Prefix()
        {
            if (!StaticUnityMethods.IsPlaying)
                return false;

            UsableItemsController.Handlers.ForEach(x =>
            {
                x.Value.DoUpdate(x.Key);

                if (x.Value.CurrentUsable.ItemSerial != 0)
                {
                    if (CustomItem.TryGetItem<CustomUsable>(x.Value.CurrentUsable.Item, out var customUsable))
                    {
                        if (customUsable.IsUsing)
                        {
                            if (customUsable.Serial != x.Key.inventory.CurItem.SerialNumber)
                            {
                                customUsable.IsUsing = false;

                                customUsable.RemainingTime = 0f;

                                if (customUsable.CooldownTime > 0f)
                                {
                                    customUsable.RemainingCooldown = customUsable.CooldownTime;
                                    customUsable.OnEnteredCooldown();
                                }

                                customUsable.OnCancelled(CancelReason.SwitchedItems);

                                x.Key.inventory.connectionToClient.Send(new StatusMessage(StatusMessage.StatusType.Cancel, x.Value.CurrentUsable.ItemSerial), 0);
                            }
                            else
                            {
                                customUsable.RemainingTime -= Time.deltaTime;
                                customUsable.IsUsing = customUsable.RemainingTime <= 0f;

                                if (!customUsable.IsUsing)
                                {
                                    if (customUsable.CooldownTime > 0f)
                                    {
                                        customUsable.RemainingCooldown = customUsable.CooldownTime;
                                        customUsable.OnEnteredCooldown();
                                    }

                                    customUsable.OnUsed();
                                }
                                else
                                {
                                    customUsable.UpdateUsing();
                                }
                            }
                        }
                    }
                    else
                    {
                        var speed = x.Value.CurrentUsable.Item.ItemTypeId.GetSpeedMultiplier(x.Key);

                        if (x.Value.CurrentUsable.ItemSerial != x.Key.inventory.CurItem.SerialNumber)
                        {
                            x.Value.CurrentUsable.Item?.OnUsingCancelled();
                            x.Value.CurrentUsable = CurrentlyUsedItem.None;

                            x.Key.inventory.connectionToClient.Send(new StatusMessage(StatusMessage.StatusType.Cancel, x.Value.CurrentUsable.ItemSerial), 0);
                        }
                        else if (Time.timeSinceLevelLoad >= (x.Value.CurrentUsable.StartTime + x.Value.CurrentUsable.Item.UseTime / speed))
                        {
                            if (!EventManager.ExecuteEvent(new PlayerUsedItemEvent(x.Key, x.Value.CurrentUsable.Item)))
                                return;

                            x.Value.CurrentUsable.Item.ServerOnUsingCompleted();

                            ScpItemsTokens.ServerOnUsingCompleted(x.Key, x.Value.CurrentUsable.Item);

                            x.Value.CurrentUsable = CurrentlyUsedItem.None;
                        }
                    }
                }
            });

            return false;
        }
    }
}