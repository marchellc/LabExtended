using LabExtended.API.Enums;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;
using LabExtended.Utilities.Debug;

namespace LabExtended.API.CustomCommands.Debug.Hints
{
    public class HintSetDebugContentCommand : CustomCommand
    {
        public override string Command => "hintcontent";
        public override string Description => "Sets the content shown in the debug hint element.";

        public override ArgumentDefinition[] Arguments { get; } = new ArgumentDefinition[]
        {
            ArgumentDefinition.FromType<string>("content", "Content of the hint"),
            ArgumentDefinition.FromType<HintAlign>("alignment", "The hint's alignment"),
            ArgumentDefinition.FromType<float>("offset", "Hint's vertical offset", ArgumentFlags.Optional, 0f)
        };

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var hintAlign = args.Get<HintAlign>("alignment");
            var newContent = args.Get<string>("content");
            var verticalOffset = args.Get<float>("offset");

            if (!sender.Hints.TryGetElement<DebugHintElement>(out var debugHintElement))
                debugHintElement = sender.Hints.AddElement<DebugHintElement>();

            debugHintElement.ContentToAdd = newContent;
            debugHintElement.Alignment = hintAlign;
            debugHintElement.VerticalOffset = verticalOffset;

            ctx.RespondOk($"Content changed to {newContent} ({hintAlign} - {debugHintElement.VerticalOffset})");
        }
    }
}