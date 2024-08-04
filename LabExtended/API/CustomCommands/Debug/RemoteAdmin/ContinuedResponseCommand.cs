using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Debug.RemoteAdmin
{
    public class ContinuedResponseCommand : CustomCommand
    {
        public override string Command => "contresponse";
        public override string Description => "Creates a new continued response";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            void Continue(ContinuedContext context)
                => context.RespondContinued($"Received: {context.RawInput}", Continue);

            ctx.RespondContinued($"Type something", Continue);
        }
    }
}