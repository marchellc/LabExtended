using LabExtended.API.Hints;
using LabExtended.API.Hints.Elements.Personal;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Hints.Loop;

public class LoopCommand : CustomCommand
{
    public override string Command { get; } = "loop";
    public override string Description { get; } = "Toggles looping.";

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);

        if (!sender.TryGetHintElement<PersonalImageElement>(out var personalImageElement))
        {
            ctx.RespondFail($"No personal image element found.");
            return;
        }
        
        personalImageElement.IsLooping = !personalImageElement.IsLooping;
        
        if (personalImageElement.IsLooping)
            ctx.RespondOk($"Enabled looping.");
        else
            ctx.RespondOk($"Disabled looping.");
    }
}