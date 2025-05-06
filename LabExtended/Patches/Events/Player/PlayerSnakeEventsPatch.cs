using HarmonyLib;

using InventorySystem.Items.Autosync;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Keycards.Snake;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Events;

using Mirror;

using UnityEngine;

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements all snake events.
/// </summary>
public static class PlayerSnakeEventsPatch
{
    [HarmonyPatch(typeof(ChaosKeycardItem), nameof(ChaosKeycardItem.ServerProcessCustomCmd))]
    private static bool Prefix(ChaosKeycardItem __instance, NetworkReader reader)
    {
        if (!ExPlayer.TryGet(__instance.Owner, out var player))
            return false;

        var type = (ChaosKeycardItem.ChaosMsgType)reader.ReadByte();
        
        if (type is ChaosKeycardItem.ChaosMsgType.SnakeMsgSync)
        {
            var message = new SnakeNetworkMessage(reader);

            if (message.HasFlag(SnakeNetworkMessage.SyncFlags.Delta))
            {
                __instance.ServerSendMessage(message);
                
                if (message.HasFlag(SnakeNetworkMessage.SyncFlags.GameReset) &&
                    message.HasFlag(SnakeNetworkMessage.SyncFlags.HasNewFood))
                {
                    player.Inventory.Snake.Reset(false, true);
                    player.Inventory.Snake.syncReceived = true;
                    
                    return false;
                }

                if (!player.Inventory.Snake.syncReceived)
                    return false;

                if (!player.Inventory.Snake.deltaReceived)
                {
                    player.Inventory.Snake.deltaReceived = true;
                    return false;
                }

                if (!player.Inventory.Snake.eventCalled)
                {
                    player.Inventory.Snake.eventCalled = true;
                    player.Inventory.Snake.Reset(true, false);
                    
                    player.Inventory.Snake.Keycard = __instance;
                    
                    ExPlayerEvents.OnSnakeStarted(new(player));
                    return false;
                }

                if (message.HasFlag(SnakeNetworkMessage.SyncFlags.GameOver))
                {
                    ExPlayerEvents.OnSnakeGameOver(new(player));

                    player.Inventory.Snake.Reset(true, true);
                    return false;
                }

                if (message.HasFlag(SnakeNetworkMessage.SyncFlags.HasNewFood))
                {
                    player.Inventory.Snake.Length++;
                    player.Inventory.Snake.Score++;

                    ExPlayerEvents.OnSnakeEaten(new(player));
                    return false;
                }
                
                // Right: X+
                // Left: X-
                
                // Up: Y+
                // Down: Y-
                
                if (message.NextFoodPosition.HasValue)
                    player.Inventory.Snake.FoodPosition = message.NextFoodPosition.Value;
                
                ExPlayerEvents.OnSnakeMoved(new(player));
            }

            return false;
        }
        
        if (type is ChaosKeycardItem.ChaosMsgType.MovementSwitch)
        {
            var x = reader.ReadSByte();
            var y = reader.ReadSByte();
            
            var direction = new Vector2Int(x, y);

            ExPlayerEvents.OnSnakeChangedDirection(new(player, direction));

            player.Inventory.Snake.Direction = direction;
            
            __instance.ServerSendPublicRpc(writer =>
            {
                writer.WriteSubheader(KeycardItem.MsgType.Custom);
                writer.WriteSubheader(ChaosKeycardItem.ChaosMsgType.MovementSwitch);
                
                writer.WriteSByte(x);
                writer.WriteSByte(y);
            });
        }
        
        return false;
    }
}