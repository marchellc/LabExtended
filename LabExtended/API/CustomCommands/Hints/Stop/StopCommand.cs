using LabExtended.API.Hints;
using LabExtended.API.Hints.Elements.Personal;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Hints.Stop;

public class StopCommand : CustomCommand
{
    public override string Command { get; } = "stop";
    public override string Description { get; } = "Stops image playback.";

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);

        if (!sender.TryGetHintElement<PersonalImageElement>(out var personalImageElement))
        {
            ctx.RespondFail($"No personal image element found.");
            return;
        }
        
        personalImageElement.Reset();
        
        ctx.RespondOk($"Playback stopped.");
    }
}