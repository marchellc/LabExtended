using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;

using MEC;

namespace LabExtended.API.CustomCommands.Debug.RemoteAdmin
{
    public class SendObjectHelpCommand : CustomCommand
    {
        public override string Command => "objecthelp";
        public override string Description => "Sends object help";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            Timing.CallDelayed(3f, () => sender.RemoteAdmin.SendHelp());
            ctx.RespondOk("Help sent");
        }
    }
}