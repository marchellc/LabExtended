using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.Utilities.Debug;

namespace LabExtended.Commands.Debug.CustomItems
{
    public class GiveDebugItemCommand : CommandInfo
    {
        public override string Command => "giveitem";
        public override string Description => "Gives you the debug custom item.";

        public object OnCalled(ExPlayer sender)
        {
            DebugCustomItem.RegisterItem();
            DebugCustomItem.Give<DebugCustomItem>(sender);

            return "Gave you the debug custom item.";
        }
    }
}