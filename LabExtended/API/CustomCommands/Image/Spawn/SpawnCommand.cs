using LabExtended.API.Toys.Primitives;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Image.Spawn;

public class SpawnCommand : CustomCommand
{
    public override string Command { get; } = "spawn";
    public override string Description { get; } = "Spawns a primitive image of a specific size.";

    public override ArgumentDefinition[] BuildArgs()
    {
        return GetArgs(x =>
        {
            x.WithArg<int>("Height", "Height of the primitive.");
            x.WithArg<int>("Width", "Width of the primitive.");
            
            x.WithArg<float>("Scale", "Scale of each pixel.");
        });
    }

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        var height = args.Get<int>("Height");
        var width = args.Get<int>("Width");
        var scale = args.Get<float>("Scale");

        var id = ImageCommand.NewId;
        var toy = PrimitiveImageToy.Create(height, width, scale);
        var image = new PrimitiveDynamicImage(toy);

        toy.Position = sender.Position;
        toy.Rotation = sender.Rotation;
        
        ImageCommand.SpawnedImages[id] = image;
        
        ctx.RespondOk($"Spawned a new image of ID {id} ({height}x{width})");
    }
}