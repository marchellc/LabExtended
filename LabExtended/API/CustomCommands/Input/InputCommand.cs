using CommandSystem;

using LabExtended.API.Input;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;
using LabExtended.Core.Hooking;

using LabExtended.Events.Player;

using UnityEngine;

namespace LabExtended.API.CustomCommands.Input
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class InputCommand : CustomCommand
    {
        public override string Command => "input";
        public override string Description => "Used for server key binds.";

        public override ArgumentDefinition[] Arguments { get; } = new ArgumentDefinition[]
        {
            ArgumentDefinition.FromType<KeyCode>("key", "Key to send to the server."),
        };

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var key = args.Get<KeyCode>("key");

            if (!InputHandler._watchedKeys.Contains(key))
            {
                ctx.RespondFail("This key is not watched.");
                return;
            }

            if (!HookRunner.RunCancellable(new PlayerKeybindReceivedArgs(sender, key), true))
            {
                ctx.RespondFail("Keybind cancelled by a plugin.");
                return;
            }

            InputHandler.OnPlayerKeybind(sender, key);
            ctx.RespondOk("Keybind received.");
        }
    }
}