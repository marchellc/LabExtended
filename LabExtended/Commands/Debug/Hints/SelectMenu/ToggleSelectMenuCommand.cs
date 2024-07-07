using LabExtended.API;
using LabExtended.API.Hints;
using LabExtended.API.Hints.Elements.SelectMenu;
using LabExtended.Core.Commands;

namespace LabExtended.Commands.Debug.Hints.SelectMenu
{
    public class ToggleSelectMenuCommand : CommandInfo
    {
        public override string Command => "togglemenu";
        public override string Description => "Togggles the Select Menu hint.";

        public object OnCalled(ExPlayer sender, HintAlign align, float vOffset = 0f)
        {
            if (sender.Hints.TryGetElement<SelectMenuElement>(out _))
            {
                sender.Hints.RemoveElement<SelectMenuElement>();
                return "Menu removed.";
            }
            else
            {
                sender.Hints.AddElement(new SelectMenuElement(align, vOffset));
                return "Menu added.";
            }
        }
    }
}
