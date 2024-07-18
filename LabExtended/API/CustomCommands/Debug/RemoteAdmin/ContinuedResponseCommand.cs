using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Debug.RemoteAdmin
{
    public class ContinuedResponseCommand : CustomCommand
    {
        public override string Command => "contresponse";
        public override string Description => "Creates a new continued response";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            ctx.RespondContinued($"Type something", onCtx =>
            {
                onCtx.RespondOk($"Received: {onCtx.RawInput}");
            });
        }
    }
}