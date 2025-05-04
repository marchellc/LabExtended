using HarmonyLib;

using InventorySystem.Items.Autosync;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Keycards.Snake;

using LabExtended.API;
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

                if (!player.Inventory.snakeStatus)
                {
                    player.Inventory.snakeStatus = true;
                    
                    ExPlayerEvents.OnSnakeStarted(new(player, __instance, player.Inventory.SnakeEngine));
                }
            }

            if (message.HasFlag(SnakeNetworkMessage.SyncFlags.GameOver))
            {
                var engine = player.Inventory.SnakeEngine;
                
                ExPlayerEvents.OnSnakeGameOver(new(player, __instance, engine, engine.Score, engine.CurLength));
                
                player.Inventory.ResetSnake();
            }

            if (message.HasFlag(SnakeNetworkMessage.SyncFlags.GameReset))
            {
                ExPlayerEvents.OnSnakeReset(new(player, __instance, player.Inventory.SnakeEngine));
                
                player.Inventory.ResetSnake();
                player.Inventory.snakeStatus = true;
            }

            if (message.HasFlag(SnakeNetworkMessage.SyncFlags.HasNewFood))
                ExPlayerEvents.OnSnakeEaten(new(player, __instance, player.Inventory.SnakeEngine, message.NextFoodPosition));
        }
        else if (type is ChaosKeycardItem.ChaosMsgType.MovementSwitch)
        {
            var xAxis = reader.ReadSByte();
            var yAxis = reader.ReadSByte();
            
            var direction = Vector2Int.CeilToInt(new(Mathf.Sign(xAxis), Mathf.Sign(yAxis)));
            
            if (!player.Inventory.snakeStatus)
            {
                player.Inventory.snakeStatus = true;
                    
                ExPlayerEvents.OnSnakeStarted(new(player, __instance, player.Inventory.SnakeEngine));
            }
            
            ExPlayerEvents.OnSnakeChangedDirection(new(player, __instance, player.Inventory.SnakeEngine, 
                player.Inventory.snakeDirection, direction));

            player.Inventory.snakeDirection = direction;
            
            __instance.ServerSendPublicRpc(writer =>
            {
                writer.WriteSubheader(KeycardItem.MsgType.Custom);
                writer.WriteSubheader(ChaosKeycardItem.ChaosMsgType.MovementSwitch);
                
                writer.WriteSByte(xAxis);
                writer.WriteSByte(yAxis);
            });
        }

        return false;
    }
}