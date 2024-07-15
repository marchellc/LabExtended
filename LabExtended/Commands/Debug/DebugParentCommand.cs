using CommandSystem;

using LabExtended.Commands.Debug.CustomItems;

using LabExtended.Commands.Debug.Hints;
using LabExtended.Commands.Debug.Hints.Image;
using LabExtended.Commands.Debug.Hints.SelectMenu;
using LabExtended.Commands.Debug.RemoteAdmin;
using LabExtended.Commands.Debug.Toys;

namespace LabExtended.Commands.Debug
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

            RegisterCommand(new HintDisableDebugHintCommand());
            RegisterCommand(new HintSetDebugContentCommand());
            RegisterCommand(new HintShowTemporaryCommand());
            RegisterCommand(new HintToggleDebugCommand());
            RegisterCommand(new HintShowImageCommand());
            RegisterCommand(new HintShowGifCommand());

            RegisterCommand(new AddMenuOptionCommand());
            RegisterCommand(new RemoveMenuOptionCommand());
            RegisterCommand(new ToggleSelectMenuCommand());

            RegisterCommand(new SpawnLightCommand());
            RegisterCommand(new SpawnPrimitiveCommand());

            RegisterCommand(new GiveDebugItemCommand());
            RegisterCommand(new PrintDebugItemCommand());
            RegisterCommand(new SpawnDebugItemCommand());
            RegisterCommand(new SelectDebugItemCommand());
        }
    }
}