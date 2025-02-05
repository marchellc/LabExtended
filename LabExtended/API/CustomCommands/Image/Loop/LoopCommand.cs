using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Image.Loop;

public class LoopCommand : CustomCommand
{
    public override string Command { get; } = "loop";
    public override string Description { get; } = "Toggles image looping.";

    public override ArgumentDefinition[] BuildArgs()
    {
        return GetArg<int>("ID", "ID of the image to loop");
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
        
        image.IsLooping = !image.IsLooping;
        
        if (image.IsLooping)
            ctx.RespondOk($"Looping enabled.");
        else
            ctx.RespondOk($"Looping disabled.");
    }
}