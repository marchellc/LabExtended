using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;
using LabExtended.Utilities.Debug;

namespace LabExtended.API.CustomCommands.Debug.Hints
{
    public class HintDisableDebugHintCommand : CustomCommand
    {
        public override string Command => "hintdisable";
        public override string Description => "Disables the debug hint element.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            ctx.RespondOk(sender.Hints.RemoveElement<DebugHintElement>() ? "Removed debug element" : "Failed to remove debug element");
        }
    }
}
