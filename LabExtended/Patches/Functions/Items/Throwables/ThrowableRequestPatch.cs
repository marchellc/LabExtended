using HarmonyLib;

using InventorySystem.Items.ThrowableProjectiles;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomGrenades;

using Mirror;

using UnityEngine;
using Utils.Networking;

namespace LabExtended.Patches.Functions.Items.Throwables;

public static class ThrowableRequestPatch
{
    [HarmonyPatch(typeof(ThrowableNetworkHandler), nameof(ThrowableNetworkHandler.ServerProcessRequest))]
    public static bool Prefix(NetworkConnection conn, ref ThrowableNetworkHandler.ThrowableItemRequestMessage msg)
    {
        if (!ExPlayer.TryGet(conn, out var player))
            return false;

        if (player.Inventory.CurrentItemIdentifier.SerialNumber != msg.Serial
            || player.Inventory.CurrentItem is not ThrowableItem throwableItem)
            return false;

        if (!CustomItemManager.InventoryItems.TryGetValue(throwableItem, out var customItemInstance)
            || customItemInstance is not CustomGrenadeInstance customGrenadeInstance)
            return true;

        if (customGrenadeInstance.IsDetonated || customGrenadeInstance.IsSpawned)
            return false;
        
        switch (msg.Request)
        {
            case ThrowableNetworkHandler.RequestType.BeginThrow:
            {
                customGrenadeInstance.IsReady = true;
                customGrenadeInstance.ReadyTime = Time.timeSinceLevelLoad;
                
                new ThrowableNetworkHandler.ThrowableItemAudioMessage(msg.Serial, msg.Request).SendToAuthenticated();
                return false;
            }

            case ThrowableNetworkHandler.RequestType.ConfirmThrowWeak:
            case ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce:
            {
                if (!customGrenadeInstance.IsReady || customGrenadeInstance.IsSpawned || customGrenadeInstance.IsDetonated)
                    return false;

                customGrenadeInstance.IsReady = false;
                customGrenadeInstance.ReadyTime = 0f;
                
                CustomGrenadeManager.SpawnItem(customGrenadeInstance);

                new ThrowableNetworkHandler.ThrowableItemAudioMessage(msg.Serial, msg.Request).SendToAuthenticated();
                return false;
            }

            case ThrowableNetworkHandler.RequestType.CancelThrow:
            {
                if (!customGrenadeInstance.IsReady || customGrenadeInstance.IsSpawned || customGrenadeInstance.IsDetonated)
                    return false;
                
                customGrenadeInstance.IsReady = false;
                customGrenadeInstance.ReadyTime = 0f;
                
                new ThrowableNetworkHandler.ThrowableItemAudioMessage(msg.Serial, msg.Request).SendToAuthenticated();
                return false;
            }
        }
        
        return true;
    }
}