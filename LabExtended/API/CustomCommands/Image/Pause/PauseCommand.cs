using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Image.Pause;

public class PauseCommand : CustomCommand
{
    public override string Command { get; } = "pause";
    public override string Description { get; } = "Toggles image pause.";

    public override ArgumentDefinition[] BuildArgs()
    {
        return GetArg<int>("ID", "ID of the image to pause / resume");
    }

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        var id = args.Get<int>("ID");

        if (!ImageCommand.SpawnedImages.TryGetValue(id, out var image))
        {
            ctx.RespondFail($"No image with ID {id}");
            return;
        }
        
        image.IsPaused = !image.IsPaused;
        
        if (image.IsPaused)
            ctx.RespondOk($"Image paused.");
        else
            ctx.RespondOk($"Image resumed.");
    }
}