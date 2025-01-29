using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.CustomData;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

namespace LabExtended.API.CustomCommands.Hints.Show;

public class ShowCommand : CustomCommand
{
    public override string Command { get; } = "show";
    public override string Description { get; } = "Shows a command to a specific player.";

    public override ArgumentDefinition[] BuildArgs()
    {
        return GetArgs(x =>
        {
            x.WithArg<PlayerListData>("Players", "List of players to show this hint to (* for all players)");
            x.WithArg<ushort>("Duration", "Duration of the hint (in seconds)");
            x.WithArg<string>("Content", "Content of the hint");
        });
    }

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        var players = args.Get<PlayerListData>("Players");
        var duration = args.Get<ushort>("Duration");
        var content = args.Get<string>("Content");
        
        players.ForEach(x => x.SendHint(content, duration));
        
        ctx.RespondOk($"Hint displayed to {players.Count} player(s).");
    }
}