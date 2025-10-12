using LabExtended.API.Toys;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.TextToy;

public partial class TextCommand
{
    [CommandOverload("clear", "Clears the text of a text toy.", null)]
    public void ClearOverload(
        [CommandParameter("ID", "ID of the spawned text toy.")] uint toyId)
    {
        if (!AdminToy.TryGet<API.Toys.TextToy>(x => x.NetId == toyId, out var textToy))
        {
            Fail($"Unknown toy ID: {toyId}");
            return;
        }
        
        textToy.Clear();
        
        Ok($"Cleared text toy {toyId}");
    }
}