using HarmonyLib;

using Hints;

using LabExtended.API;

using MEC;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
    public static class HintPatch
    {
        public static bool Prefix(HintDisplay __instance, Hint hint)
        {
            if (hint is null || __instance.connectionToClient is null)
                return false;

            if (HintDisplay.SuppressedReceivers.Contains(__instance.connectionToClient))
                return false;

            if (!ExPlayer.TryGet(__instance.connectionToClient, out var player) || player.Hints._paused)
                return true;

            player.Hints._paused = true;
            player.Connection.Send(new HintMessage(hint));

            Timing.CallDelayed(hint.DurationScalar + (player.Ping * 10), () => player.Hints._paused = false);
            return false;
        }
    }
}