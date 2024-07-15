using LabExtended.API;
using LabExtended.Core.Commands;

using MEC;

namespace LabExtended.Commands.Debug.RemoteAdmin
{
    public class SendObjectHelpCommand : CommandInfo
    {
        public override string Command => "objecthelp";
        public override string Description => "Sends object help";

        public object OnCalled(ExPlayer sender)
        {
            Timing.CallDelayed(3f, () => sender.RemoteAdmin.SendHelp());
            return "Help sent";
        }
    }
}