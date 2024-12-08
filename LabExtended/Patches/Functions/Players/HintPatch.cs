using HarmonyLib;

using Hints;

using LabExtended.API;
using LabExtended.API.Hints;

using MEC;

namespace LabExtended.Patches.Functions.Players
{
    public static class HintPatch
    {
        [HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
        public static bool Prefix(HintDisplay __instance, Hint hint)
        {
            if (hint is null || __instance.connectionToClient is null)
                return false;

            if (HintDisplay.SuppressedReceivers.Contains(__instance.connectionToClient))
                return false;

            if (!ExPlayer.TryGet(__instance.connectionToClient, out var player))
                return true;

            HintController.PauseHints(player);

            player.Connection.Send(new HintMessage(hint));

            Timing.CallDelayed(hint.DurationScalar + 0.1f, () => HintController.ResumeHints(player));
            return false;
        }
    }
}