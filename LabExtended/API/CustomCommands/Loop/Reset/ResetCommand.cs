using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using LabExtended.Utilities.Unity;

namespace LabExtended.API.CustomCommands.Loop.Reset
{
    public class ResetCommand : CustomCommand
    {
        public override string Command => "reset";
        public override string Description => "Resets the player loop system into it's default state.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            PlayerLoopHelper.ResetSystem();

            ctx.RespondOk("System reset.");
        }
    }
}