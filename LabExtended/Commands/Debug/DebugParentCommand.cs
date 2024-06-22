using CommandSystem;

using LabExtended.Commands.Debug.Hints;

namespace LabExtended.Commands.Debug
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class DebugParentCommand : VanillaParentCommandBase
    {
        public DebugParentCommand() : base("debug", "Debug utilities for LabExtended.") { }

        public override void OnInitialized()
        {
            base.OnInitialized();

            RegisterCommand(new HintDisableDebugHintCommand());
            RegisterCommand(new HintSetDebugContentCommand());
            RegisterCommand(new HintShowTemporaryCommand());
            RegisterCommand(new HintToggleDebugCommand());
        }
    }
}