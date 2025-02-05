using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Image.Parent;

public class ParentCommand : CustomCommand
{
    public override string Command { get; } = "parent";
    public override string Description { get; } = "Sets or removes a parent from an image.";

    public override ArgumentDefinition[] BuildArgs()
    {
        return GetArgs(x =>
        {
            x.WithArg<int>("ID", "ID of the image.");
            x.WithOptional<ExPlayer>("Target", "The targeted parent", null);
        });
    }

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        var id = args.Get<int>("ID");
        var target = args.Get<ExPlayer>("Target");
        
        if (!ImageCommand.SpawnedImages.TryGetValue(id, out var image))
        {
            ctx.RespondFail($"Image with ID {id} does not exist.");
            return;
        }

        if (target is null)
        {
            image.Toy.Parent = null;
            
            ctx.RespondOk($"Removed parent from {id}.");
            return;
        }

        image.Toy.Parent = target.Transform;
        
        ctx.RespondOk($"Set parent of {id} to {target.Name} ({target.UserId})");
    }
}