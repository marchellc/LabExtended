using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Image.Stop;

public class StopCommand : CustomCommand
{
    public override string Command { get; } = "stop";
    public override string Description { get; } = "Stops image playback.";

    public override ArgumentDefinition[] BuildArgs()
    {
        return GetArg<int>("ID", "ID of the image to stop.");
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
        
        image.Reset();
        
        if (image.ClearOnFinish)
            image.Toy.Clear();
        
        ctx.RespondOk($"Stopped image playback.");
    }
}