using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;
using LabExtended.Utilities.Debug;

namespace LabExtended.API.CustomCommands.Debug.CustomItems
{
    public class GiveDebugItemCommand : CustomCommand
    {
        public override string Command => "giveitem";
        public override string Description => "Gives you the debug custom item.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            DebugCustomItem.RegisterItem();

            LabExtended.API.CustomItems.CustomItem.Give<DebugCustomItem>(sender);

            ctx.RespondOk("Gave you the custom item.");
        }
    }
}