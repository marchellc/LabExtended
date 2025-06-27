using LabExtended.API.Images;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Spawn;

public partial class SpawnCommand
{
    [CommandOverload("image", "Spawns an image.")]
    public void ImageOverload(
        [CommandParameter("Name", "Name of the image to spawn.")] string name)
    {
        if (!ImageLoader.TryGet(name, out var image))
        {
            Fail($"Unknown image: {name}");
            return;
        }

        var toy = image.SpawnImage(Sender.Position, Sender.Rotation);
        
        Ok($"Spawned image '{image.Name}' (ID: {toy.NetId})");
    }
}