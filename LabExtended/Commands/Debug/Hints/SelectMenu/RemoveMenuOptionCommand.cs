using LabExtended.API;
using LabExtended.API.Hints.Elements.SelectMenu;
using LabExtended.Core.Commands;

namespace LabExtended.Commands.Debug.Hints.SelectMenu
{
    public class RemoveMenuOptionCommand : CommandInfo
    {
        public override string Command => "removemenu";
        public override string Description => "Removes an option from the select menu.";

        public object OnCalled(ExPlayer sender, string id)
        {
            if (!sender.Hints.TryGetElement<SelectMenuElement>(out var selectMenuElement))
                return "You do not have an active Select Menu.";

            if (!selectMenuElement.TryGetOption(id, out _))
                return "Unknown option.";

            selectMenuElement.RemoveOption(id);
            return "Option removed.";
        }
    }
}
