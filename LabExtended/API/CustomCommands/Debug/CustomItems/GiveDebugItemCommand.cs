using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.Utilities.Debug;

namespace LabExtended.API.CustomCommands.Debug.CustomItems
{
    public class GiveDebugItemCommand : CommandInfo
    {
        public override string Command => "giveitem";
        public override string Description => "Gives you the debug custom item.";

        public object OnCalled(ExPlayer sender)
        {
            DebugCustomItem.RegisterItem();
            API.CustomItems.CustomItem.Give<DebugCustomItem>(sender);

            return "Gave you the debug custom item.";
        }
    }
}