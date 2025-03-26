using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Debug.Other
{
    public class NpcSwitchesCommand : CustomCommand
    {
        public override string Command => "npcswitches";
        public override string Description => "Set toggles of target as NPC or Player";

        public override ArgumentDefinition[] BuildArgs()
        {
            return GetArgs(x =>
            {
                x.WithArg<bool>("AsNpc", "Set NPC switches, otherwise Player switches");
                x.WithOptional<ExPlayer>("Target", "The player to set the switch on.");
            });
        }

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var asNpc = args.Get<bool>("AsNpc");
            var target = args.Get<ExPlayer>("Target") ?? sender;

            if (asNpc) {
                target.Toggles.ResetToNpc();
            } else {
                target.Toggles.ResetToPlayer();
            }

            ctx.RespondOk($"Set switches {target.Nickname} as {(asNpc ? "NPC" : "Player")}");
        }
    }
}