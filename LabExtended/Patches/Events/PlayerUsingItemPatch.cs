using CustomPlayerEffects;

using HarmonyLib;

using InventorySystem.Items.Usables;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

using Mirror;

using UnityEngine;

using Utils.Networking;

namespace LabExtended.Patches.Events
{
    public static class PlayerUsingItemPatch
    {
        [HookPatch(typeof(PlayerUsingItemArgs))]
        [HarmonyPatch(typeof(UsableItemsController), nameof(UsableItemsController.ServerReceivedStatus))]
        public static bool Prefix(NetworkConnection conn, StatusMessage msg)
        {
            if (!ExPlayer.TryGet(conn, out var player))
                return true;

            if (player.Inventory.CurrentItem is null || player.Inventory.CurrentItem is not UsableItem curUsable)
                return false;

            if (curUsable.ItemSerial != msg.ItemSerial)
                return false;

            if (msg.Status is StatusMessage.StatusType.Start)
            {
                if (!curUsable.ServerValidateStartRequest(player.Inventory.UsableItemsHandler))
                    return false;

                if (player.Inventory.UsableItemsHandler.CurrentUsable.ItemSerial != 0)
                    return false;

                if (!curUsable.CanStartUsing)
                    return false;

                var usingArgs = new PlayerUsingItemArgs(player, curUsable, UsableItemsController.GetCooldown(curUsable.ItemSerial, curUsable, player.Inventory.UsableItemsHandler), curUsable.ItemTypeId.GetSpeedMultiplier(player.Hub));

                if (!HookRunner.RunEvent(usingArgs, true))
                    return false;

                if (usingArgs.RemainingCooldown > 0f)
                {
                    player.Connection.Send(new ItemCooldownMessage(curUsable.ItemSerial, usingArgs.RemainingCooldown));
                    return false;
                }

                if (usingArgs.SpeedMultiplier > 0f)
                {
                    var usingEventArgs = new PlayerUsingItemEventArgs(player.Hub, curUsable);

                    PlayerEvents.OnUsingItem(usingEventArgs);

                    if (!usingEventArgs.IsAllowed)
                        return false;
                    
                    player.Inventory.UsableItemsHandler.CurrentUsable = new CurrentlyUsedItem(curUsable, curUsable.ItemSerial, Time.timeSinceLevelLoad);
                    player.Inventory.UsableItemsHandler.CurrentUsable.Item.OnUsingStarted();

                    msg.SendToAuthenticated();
                    return false;
                }
            }
            else
            {
                if (!curUsable.ServerValidateCancelRequest(player.Inventory.UsableItemsHandler))
                    return false;

                if (player.Inventory.CurrentlyUsedItem.ItemSerial == 0)
                    return false;

                var speedMultiplier = curUsable.ItemTypeId.GetSpeedMultiplier(player.Hub);

                if (player.Inventory.CurrentlyUsedItem.StartTime + curUsable.MaxCancellableTime / speedMultiplier > Time.timeSinceLevelLoad)
                {
                    var cancellingArgs = new PlayerCancellingUsingItemEventArgs(player.Hub, curUsable);

                    PlayerEvents.OnCancellingUsingItem(cancellingArgs);

                    if (!cancellingArgs.IsAllowed)
                        return false;

                    player.Inventory.CurrentlyUsedItem.Item.OnUsingCancelled();
                    player.Inventory.CurrentlyUsedItem = CurrentlyUsedItem.None;

                    msg.SendToAuthenticated();

                    PlayerEvents.OnCancelledUsingItem(new PlayerCancelledUsingItemEventArgs(player.Hub, curUsable));
                }
            }

            return false;
        }
    }
}