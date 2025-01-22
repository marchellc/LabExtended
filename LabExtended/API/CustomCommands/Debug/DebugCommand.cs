using CommandSystem;

using LabExtended.Commands;

using LabExtended.API.CustomCommands.Debug.Other;
using LabExtended.API.CustomCommands.Debug.Settings;

namespace LabExtended.API.CustomCommands.Debug
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class DebugCommand : VanillaParentCommandBase
    {
        public override string Command => "debug";
        public override string Description => "Debug commands for the LabExtended API.";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new SetSwitchCommand());
            RegisterCommand(new SetGravityCommand());
            RegisterCommand(new SettingsTestCommand());
            RegisterCommand(new ListPlayersCommand());
        }
    }
}