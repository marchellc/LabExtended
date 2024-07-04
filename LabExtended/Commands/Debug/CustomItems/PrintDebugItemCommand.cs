using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.CustomItems;
using LabExtended.Utilities.Debug;

namespace LabExtended.Commands.Debug.CustomItems
{
    public class PrintDebugItemCommand : CommandInfo
    {
        public override string Command => "printitem";
        public override string Description => "Prints debug info for all debug items.";

        public object OnCalled(ExPlayer sender)
        {
            foreach (var spawnedItem in CustomItem.GetSpawned<DebugCustomItem>())
                spawnedItem.PrintState();

            foreach (var ownedItem in CustomItem.GetItems<DebugCustomItem>(sender))
                ownedItem.PrintState();

            return "Printed state of all items";
        }
    }
}