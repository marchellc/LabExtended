using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Debug.Hints
{
    public class HintShowTemporaryCommand : CustomCommand
    {
        public override string Command => "hintshow";
        public override string Description => "Shows a temporary hint";

        public override ArgumentDefinition[] BuildArgs()
        {
            return ArgumentBuilder.Get(x =>
            {
                x.WithArg<ushort>("Duration", "Hint's duration.")
                 .WithArg<string>("Content", "Hint's content.");
            });
        }

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var duration = args.Get<ushort>("Duration");
            var content = args.Get<string>("Content");

            sender.Hints.Show(content, duration);

            ctx.RespondOk("Hint shown.");
        }
    }
}