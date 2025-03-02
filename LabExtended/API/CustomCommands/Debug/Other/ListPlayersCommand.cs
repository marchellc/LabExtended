using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.API.CustomCommands.Debug.Other;

public class ListPlayersCommand : CustomCommand
{
    public override string Command { get; } = "listplayers";
    public override string Description { get; } = "Lists all player objects in ExPlayer.AllPlayers";

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        ctx.RespondOk(StringBuilderPool.Shared.BuildString(x =>
        {
            if (ExPlayer.AllPlayers.Count < 1)
            {
                x.AppendLine("No players found.");
                return;
            }

            x.AppendLine($"All={ExPlayer.AllPlayers.Count}");

            foreach (var player in ExPlayer.AllPlayers)
                x.AppendLine($"[{player.PlayerId} - {player.NetId}] {player.Name} ({player.UserId}); IsServer={player.IsServer}; IsValid={(bool)player}; IsNpc={player.IsNpc}");

            x.AppendLine();
            x.AppendLine($"Real={ExPlayer.Players.Count}");

            foreach (var player in ExPlayer.Players)
                x.AppendLine($"[{player.PlayerId} - {player.NetId}] {player.Name} ({player.UserId}); IsServer={player.IsServer}; IsValid={(bool)player}; IsNpc={player.IsNpc}");

            x.AppendLine();
            x.AppendLine($"NPC={ExPlayer.NpcCount}");
            
            foreach (var player in ExPlayer.NpcPlayers)
                x.AppendLine($"[{player.PlayerId} - {player.NetId}] {player.Name} ({player.UserId}); IsServer={player.IsServer}; IsValid={(bool)player}; IsNpc={player.IsNpc}");
        }));
    }
}