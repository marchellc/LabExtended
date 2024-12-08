using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using LabExtended.Utilities.Unity;

namespace LabExtended.API.CustomCommands.Loop.Disable
{
    public class DisableCommand : CustomCommand
    {
        public override string Command => "disable";
        public override string Description => "Disables a player loop.";

        public override ArgumentDefinition[] BuildArgs()
            => GetArg<string>("Type", "The loop type.");

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var type = args.Get<string>("Type");
            var done = false;

            PlayerLoopHelper.ModifySystem(x =>
            {
                x.RemoveSystems(s => done = s.type.FullName == type || s.type.Name == type);

                if (done)
                    return x;

                return null;
            });

            if (done)
                ctx.RespondOk("System removed.");
            else
                ctx.RespondFail("Unknown system.");
        }
    }
}
