using LabExtended.API.Hints;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;

namespace LabExtended.API.CustomCommands.Debug.Hints
{
    public class HintToggleDebugCommand : CustomCommand
    {
        public override string Command => "hintdebug";
        public override string Description => "Toggles debug output of the hint module.";

        public override void OnCommand(ExPlayer sender, Commands.Interfaces.ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);
            ctx.RespondOk((HintModule.ShowDebug = !HintModule.ShowDebug) ? "Hint debug enabled." : "Hint debug disabled.");
        }
    }
}