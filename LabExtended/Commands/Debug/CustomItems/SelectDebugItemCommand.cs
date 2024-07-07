using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.Core.Commands;
using LabExtended.Utilities.Debug;

namespace LabExtended.Commands.Debug.CustomItems
{
    public class SelectDebugItemCommand : CommandInfo
    {
        public override string Command => "selectitem";
        public override string Description => "Selects the debug custom item.";

        public object OnCalled(ExPlayer sender)
        {
            DebugCustomItem.RegisterItem();

            var debugItem = CustomItem.GetItem<DebugCustomItem>(sender);

            if (debugItem is null)
                debugItem = CustomItem.Give<DebugCustomItem>(sender);

            if (!debugItem.IsSelected)
                debugItem.Select();

            return "Selected the debug custom item.";
        }
    }
}