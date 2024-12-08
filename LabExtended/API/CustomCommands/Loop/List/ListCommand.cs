using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using LabExtended.Utilities.Unity;

namespace LabExtended.API.CustomCommands.Loop.List
{
    public class ListCommand : CustomCommand
    {
        public override string Command => "list";
        public override string Description => "Lists all player loops.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);
            ctx.RespondOk(PlayerLoopHelper.GetPlayerLoopNames(PlayerLoopHelper.System, "-  "));
        }
    }
}