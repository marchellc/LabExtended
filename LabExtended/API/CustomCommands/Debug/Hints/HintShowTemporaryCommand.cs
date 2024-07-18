using LabExtended.API;
using LabExtended.Core.Commands;

namespace LabExtended.API.CustomCommands.Debug.Hints
{
    public class HintShowTemporaryCommand : CommandInfo
    {
        public override string Command => "hintshow";
        public override string Description => "Shows a temporary hint";

        public object OnCalled(ExPlayer player, string hintDuration, string hintContent)
        {
            player.Hints.Show(hintContent, ushort.Parse(hintDuration));
            return "Shown hint";
        }
    }
}