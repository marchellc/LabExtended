using HarmonyLib;
using InventorySystem.Items.Usables;

using LabExtended.API;

using LabExtended.API.CustomItems;
using LabExtended.API.CustomUsables.Behaviours;

using Mirror;
using Utils.Networking;

namespace LabExtended.Patches.Functions.Items;

/// <summary>
/// Implements use logic for custom usable items.
/// </summary>
public static class UsableItemStatusPatch
{
    [HarmonyPatch(typeof(UsableItemsController), nameof(UsableItemsController.ServerReceivedStatus))]
    private static bool Prefix(NetworkConnectionToClient conn, StatusMessage msg)
    {
        if (!ExPlayer.TryGet(conn, out var player))
            return true;

        if (player.Inventory.CurrentItem is not UsableItem usableItem
            || usableItem.ItemSerial != msg.ItemSerial
            || !CustomItemUtils.TryGetBehaviour<CustomUsableInventoryBehaviour>(usableItem.ItemSerial,
                out var behaviour))
            return true;

        if (msg.Status is StatusMessage.StatusType.Start)
        {
            if (behaviour.RemainingCooldown > 0f)
            {
                conn.Send(new ItemCooldownMessage(msg.ItemSerial, behaviour.RemainingCooldown));
                return false;
            }

            if (!behaviour.IsReady)
                return false;

            var duration = behaviour.UseDuration;

            behaviour.IsSelected = true;
            
            if (!behaviour.OnUsing(ref duration))
                return false;
            
            behaviour.InternalStart(duration);
            
            usableItem.OnUsingStarted();
            
            msg.SendToAuthenticated();
        }
        else
        {
            if (behaviour.IsUsing)
            {
                behaviour.InternalCancel(true);
                
                usableItem.OnUsingCancelled();
                
                msg.SendToAuthenticated();
            }
        }
        
        return false;
    }
}