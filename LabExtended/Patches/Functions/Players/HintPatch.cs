using HarmonyLib;

using Hints;

using LabExtended.API;
using LabExtended.API.Hints;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Patches.Functions.Players;

/// <summary>
/// Allows vanilla hints to be displayed over custom hint displays.
/// </summary>
public static class HintPatch
{
    [HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
    private static bool Prefix(HintDisplay __instance, Hint hint)
    {
        if (HintDisplay.SuppressedReceivers.Contains(__instance.connectionToClient))
            return false;

        if (!ExPlayer.TryGet(__instance.connectionToClient, out var player))
            return false;
        
        player.ShowHint(hint);
        return false;
    }
}