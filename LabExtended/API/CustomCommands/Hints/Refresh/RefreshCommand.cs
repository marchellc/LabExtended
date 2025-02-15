using LabExtended.API.Hints;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Hints.Refresh;

public class RefreshCommand : CustomCommand
{
    public override string Command { get; } = "refresh";
    public override string Description { get; } = "Performs a manual tick.";

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        HintController.ForceSendHints();
        
        ctx.RespondOk($"Tick complete.");
    }
}