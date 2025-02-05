using LabExtended.API.Hints;
using LabExtended.API.Hints.Elements.Personal;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Hints.Pause;

public class PauseCommand : CustomCommand
{
    public override string Command { get; } = "pause";
    public override string Description { get; } = "Toggles image pause.";

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);

        if (!sender.TryGetHintElement<PersonalImageElement>(out var personalImageElement))
        {
            ctx.RespondFail($"No personal image element found.");
            return;
        }
        
        personalImageElement.IsPaused = !personalImageElement.IsPaused;
        
        if (personalImageElement.IsPaused)
            ctx.RespondOk($"Paused image.");
        else
            ctx.RespondOk($"Unpaused image.");
    }
}