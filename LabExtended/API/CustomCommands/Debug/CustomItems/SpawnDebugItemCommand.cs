using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;
using LabExtended.Utilities.Debug;

namespace LabExtended.API.CustomCommands.Debug.CustomItems
{
    public class SpawnDebugItemCommand : CustomCommand
    {
        public override string Command => "spawnitem";
        public override string Description => "Spawns the debug custom item.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            DebugCustomItem.RegisterItem();
            API.CustomItems.CustomItem.Spawn<DebugCustomItem>(sender.Position, sender.Rotation, null, sender);

            ctx.RespondOk("Spawned the debug custom item.");
        }
    }
}