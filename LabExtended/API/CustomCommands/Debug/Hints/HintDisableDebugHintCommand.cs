using CommandSystem;
using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.Utilities.Debug;

namespace LabExtended.API.CustomCommands.Debug.Hints
{
    public class HintDisableDebugHintCommand : CommandInfo
    {
        public override string Command => "hintdisable";
        public override string Description => "Disables the debug hint element.";

        public object OnCalled(ExPlayer player)
            => player.Hints.RemoveElement<DebugHintElement>() ? "Removed debug element" : "Failed to remove debug element";
    }
}
