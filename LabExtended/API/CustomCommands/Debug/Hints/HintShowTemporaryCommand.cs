using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Debug.Hints
{
    public class HintShowTemporaryCommand : CustomCommand
    {
        public override string Command => "hintshow";
        public override string Description => "Shows a temporary hint";

        public override ArgumentDefinition[] Arguments { get; } = new ArgumentDefinition[]
        {
            ArgumentDefinition.FromType<ushort>("duration", "The hint's duration"),
            ArgumentDefinition.FromType<string>("content", "The hint's content")
        };

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var duration = args.Get<ushort>("duration");
            var content = args.Get<string>("content");

            sender.Hints.Show(content, duration);

            ctx.RespondOk("Hint shown.");
        }
    }
}