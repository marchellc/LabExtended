using LabExtended.API;

using LabExtended.Core.Commands;

using LabExtended.Hints;
using LabExtended.Hints.Elements.SelectMenu;

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
