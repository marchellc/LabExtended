using CommandSystem;

using LabExtended.Commands;

using LabExtended.API.CustomCommands.Debug.Hints;
using LabExtended.API.CustomCommands.Debug.Other;

namespace LabExtended.API.CustomCommands.Debug
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class DebugParentCommand : VanillaParentCommandBase
    {
        public override string Command => "debug";
        public override string Description => "Debug commands for the LabExtended API.";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new HintShowTemporaryCommand());
            RegisterCommand(new HintToggleDebugCommand());

            RegisterCommand(new SetSwitchCommand());

            RegisterCommand(new SpawnNpcCommand());
        }
    }
}