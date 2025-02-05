using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Image.Destroy;

public class DestroyCommand : CustomCommand
{
    public override string Command { get; } = "destroy";
    public override string Description { get; } = "Destroys an active primitive image.";

    public override ArgumentDefinition[] BuildArgs()
    {
        return GetArg<int>("ID", "ID of the image.");
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
        
        image.Dispose();
        ImageCommand.SpawnedImages.Remove(id);
        
        ctx.RespondOk($"Image with ID {id} has been destroyed.");
    }
}