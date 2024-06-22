using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.Hints;

namespace LabExtended.Commands.Debug.Hints
{
    public class HintToggleDebugCommand : CommandInfo
    {
        public override string Command => "hintdebug";
        public override string Description => "Toggles debug output of the hint module.";

        public object OnCalled(ExPlayer player)
            => (HintModule.ShowDebug = !HintModule.ShowDebug) ? "Hint debug enabled." : "Hint debug disabled.";
    }
}