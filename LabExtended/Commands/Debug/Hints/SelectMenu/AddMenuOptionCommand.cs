using LabExtended.API;
using LabExtended.API.Hints.Elements.SelectMenu;

using LabExtended.Core.Commands;

namespace LabExtended.Commands.Debug.Hints.SelectMenu
{
    public class AddMenuOptionCommand : CommandInfo
    {
        public override string Command => "addmenu";
        public override string Description => "Adds an option to the select menu.";

        public object OnCalled(ExPlayer sender, string id, string label)
        {
            if (!sender.Hints.TryGetElement<SelectMenuElement>(out var selectMenuElement))
                return "You do not have an active Select Menu.";

            if (selectMenuElement.TryGetOption(id, out _))
                return "An option with the same ID already exists.";

            selectMenuElement.CreateOption(new SelectMenuOption(label, id));
            return "Option added.";
        }
    }
}
