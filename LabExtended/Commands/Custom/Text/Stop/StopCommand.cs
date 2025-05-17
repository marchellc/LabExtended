using LabExtended.API.Toys;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.TextToy;

public partial class TextCommand
{
    [CommandOverload("stop", "Stops the playback.")]
    public void StopOverload(uint toyId)
    {
        if (!AdminToy.TryGet<API.Toys.TextToy>(x => x.NetId == toyId, out var textToy))
        {
            Fail($"Unknown ID: {toyId}");
            return;
        }
        
        textToy.PlaybackDisplay.Stop();
        
        Ok($"Stopped playback of text toy '{toyId}'");
    }
}