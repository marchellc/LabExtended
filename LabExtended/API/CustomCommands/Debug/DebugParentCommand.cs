using CommandSystem;

using LabExtended.Commands;

using LabExtended.API.CustomCommands.Debug.CustomItems;
using LabExtended.API.CustomCommands.Debug.RemoteAdmin;
using LabExtended.API.CustomCommands.Debug.Hints;

namespace LabExtended.API.CustomCommands.Debug
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class DebugParentCommand : VanillaParentCommandBase
    {
        public DebugParentCommand() : base("debug", "Debug utilities for LabExtended.") { }

        public override void OnInitialized()
        {
            base.OnInitialized();

            RegisterCommand(new AddDebugObjectCommand());
            RegisterCommand(new SendObjectHelpCommand());
            RegisterCommand(new ContinuedResponseCommand());

            RegisterCommand(new HintDisableDebugHintCommand());
            RegisterCommand(new HintSetDebugContentCommand());
            RegisterCommand(new HintShowTemporaryCommand());
            RegisterCommand(new HintToggleDebugCommand());

            RegisterCommand(new GiveDebugItemCommand());
            RegisterCommand(new PrintDebugItemCommand());
            RegisterCommand(new SpawnDebugItemCommand());
            RegisterCommand(new SelectDebugItemCommand());
        }
    }
}