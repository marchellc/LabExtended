using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.Hints;
using LabExtended.Utilities.Debug;

namespace LabExtended.Commands.Debug.Hints
{
    public class HintSetDebugContentCommand : CommandInfo
    {
        public override string Command => "hintcontent";
        public override string Description => "Sets the content shown in the debug hint element.";

        public object OnCalled(ExPlayer player, HintAlign hintAlign, string newContent, string verticalOffset = "1")
        {
            if (!player.Hints.TryGetElement<DebugHintElement>(out var debugHintElement))
                debugHintElement = player.Hints.AddElement<DebugHintElement>();

            debugHintElement.ContentToAdd = newContent;
            debugHintElement.Alignment = hintAlign;
            debugHintElement.VerticalOffset = float.Parse(verticalOffset);

            return $"Content changed to {newContent} ({hintAlign} - {debugHintElement.VerticalOffset})";
        }
    }
}