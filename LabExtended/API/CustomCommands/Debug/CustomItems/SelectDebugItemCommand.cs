using LabExtended.API.CustomItems;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;
using LabExtended.Utilities.Debug;

namespace LabExtended.API.CustomCommands.Debug.CustomItems
{
    public class SelectDebugItemCommand : CustomCommand
    {
        public override string Command => "selectitem";
        public override string Description => "Selects the debug custom item.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            DebugCustomItem.RegisterItem();

            var debugItem = CustomItem.GetItem<DebugCustomItem>(sender);

            if (debugItem is null)
                debugItem = CustomItem.Give<DebugCustomItem>(sender);

            if (!debugItem.IsSelected)
                debugItem.Select();

            ctx.RespondOk("Selected the debug custom item.");
        }
    }
}