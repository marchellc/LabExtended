using LabExtended.API.Images;
using LabExtended.API.Toys;

using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.TextToy;

public partial class TextCommand
{
    [CommandOverload("image", "Plays an image.", null)]
    public void ImageOverload(
        [CommandParameter("ID", "ID of the text toy.")] uint toyId,
        [CommandParameter("Name", "Name of the image to play.")] string imageName)
    {
        if (!AdminToy.TryGet<API.Toys.TextToy>(x => x.NetId == toyId, out var textToy))
        {
            Fail($"Unknown ID: {toyId}");
            return;
        }

        if (!ImageLoader.TryGet(imageName, out var image))
        {
            Fail($"Unknown image: {imageName}");
            return;
        }
        
        textToy.PlaybackDisplay.Play(image);
        
        Ok($"Started playing image {imageName} on toy {toyId}");
    }
}