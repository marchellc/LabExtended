using CommandSystem;

using LabExtended.API.CustomCommands.Tick.Status;
using LabExtended.Commands;

namespace LabExtended.API.CustomCommands.Tick
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class TickCommand : VanillaParentCommandBase
    {
        public override string Command => "tick";
        public override string Description => "Commands for tick distribution management.";

        public override void LoadGeneratedCommands()
        {
            base.LoadGeneratedCommands();

            RegisterCommand(new TickStatusCommand());
        }
    }
}
