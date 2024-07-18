using CommandSystem;
using LabExtended.API;
using LabExtended.API.Input;
using LabExtended.Core.Commands;
using LabExtended.Core.Hooking;

using LabExtended.Events.Player;

using UnityEngine;

namespace LabExtended.API.CustomCommands.Input
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class InputCommand : CommandInfo
    {
        public override string Command => "input";
        public override string Description => "Used for server key binds.";

        public object OnCalled(ExPlayer sender, KeyCode key)
        {
            if (!InputHandler._watchedKeys.Contains(key))
                return "This keybind is not registered.";

            if (!HookRunner.RunCancellable(new PlayerKeybindReceivedArgs(sender, key), true))
                return "Keybind cancelled.";

            InputHandler.OnPlayerKeybind(sender, key);
            return "Keybind received.";
        }
    }
}