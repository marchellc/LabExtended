using LabExtended.API.Toys;
using LabExtended.Commands.Attributes;

using LabExtended.Images.Playback;

namespace LabExtended.Commands.Custom.TextToy;

public partial class TextCommand
{
    [CommandOverload("disable", "Disables a playback option.")]
    public void DisableOverload(
        [CommandParameter("ID", "ID of the text toy.")] uint toyId, 
        [CommandParameter("Option", "The option to disable.")] PlaybackFlags option)
    {
        if (!AdminToy.TryGet<API.Toys.TextToy>(x => x.NetId == toyId, out var textToy))
        {
            Fail($"Unknown ID: {toyId}");
            return;
        }

        textToy.PlaybackDisplay.DisableOption(option);
        
        Ok($"Disabled option '{option}' on toy '{toyId}' (options: {textToy.PlaybackDisplay.Options})");
    }
}