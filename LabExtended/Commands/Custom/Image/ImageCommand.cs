using LabExtended.API.Images;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Image;

[Command("image", "Image API commands.")]
public class ImageCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("reload", "Reloads a specified image file.", null)]
    public void Reload(
        [CommandParameter("Name", "Name of the image to reload.")]
        string name)
    {
        if (!ImageLoader.Reload(name))
        {
            Fail($"Could not reload image '{name}'");
            return;
        }
        
        Ok($"Reloaded image '{name}'");
    }
}