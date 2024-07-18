using LabExtended.API.CustomItems;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;
using LabExtended.Utilities.Debug;

namespace LabExtended.API.CustomCommands.Debug.CustomItems
{
    public class PrintDebugItemCommand : CustomCommand
    {
        public override string Command => "printitem";
        public override string Description => "Prints debug info for all debug items.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            foreach (var spawnedItem in CustomItem.GetSpawned<DebugCustomItem>())
                spawnedItem.PrintState();

            foreach (var ownedItem in CustomItem.GetItems<DebugCustomItem>(sender))
                ownedItem.PrintState();

            ctx.RespondOk("Printed state of all items.");
        }
    }
}