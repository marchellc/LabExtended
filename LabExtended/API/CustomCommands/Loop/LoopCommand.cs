using CommandSystem;

using LabExtended.API.CustomCommands.Loop.Disable;
using LabExtended.API.CustomCommands.Loop.List;
using LabExtended.API.CustomCommands.Loop.Reset;

using LabExtended.Commands;

namespace LabExtended.API.CustomCommands.Loop
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class LoopCommand : VanillaParentCommandBase
    {
        public override string Command => "loop";
        public override string Description => "Commands for Unity's PlayerLoop system management.";

        public override void LoadGeneratedCommands()
        {
            base.LoadGeneratedCommands();

            RegisterCommand(new ListCommand());
            RegisterCommand(new DisableCommand());
            RegisterCommand(new ResetCommand());
        }
    }
}