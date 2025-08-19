using HarmonyLib;

using LabExtended.Utilities;

using PlayerRoles.FirstPersonControl.NetworkMessages;

namespace LabExtended.Patches.Functions.Players;

/// <summary>
/// Prevents base-game from synchronizing positions.
/// </summary>
public static class DisablePositionSyncPatch
{
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.LateUpdate))]
    private static bool Prefix() => !PositionSync.IsEnabled;
}