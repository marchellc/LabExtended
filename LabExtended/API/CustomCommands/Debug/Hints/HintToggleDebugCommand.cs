using LabExtended.API;
using LabExtended.API.Hints;
using LabExtended.Core.Commands;

namespace LabExtended.API.CustomCommands.Debug.Hints
{
    public class HintToggleDebugCommand : CommandInfo
    {
        public override string Command => "hintdebug";
        public override string Description => "Toggles debug output of the hint module.";

        public object OnCalled(ExPlayer player)
            => (HintModule.ShowDebug = !HintModule.ShowDebug) ? "Hint debug enabled." : "Hint debug disabled.";
    }
}