using LabExtended.API.Toys;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.TextToy;

public partial class TextCommand
{
    [CommandOverload("set", "Sets the text of a text toy.")]
    public void SetOverload(
        [CommandParameter("ID", "ID of the text toy.")] uint toyId, 
        [CommandParameter("Text", "The text to set.")] string text)
    {
        if (!AdminToy.TryGet<API.Toys.TextToy>(x => x.NetId == toyId, out var textToy))
        {
            Fail($"Unknown ID: {toyId}");
            return;
        }

        textToy.Format = "{0}";
        textToy.Add(true, text);
        
        Ok($"Changed text of toy {toyId}");
    }
}