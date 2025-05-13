using HarmonyLib;

using RemoteAdmin.Communication;

namespace LabExtended.Patches.Functions.RemoteAdmin;

/// <summary>
/// Implements custom Remote Admin actions.
/// </summary>
public static class RemoteAdminDummyGatherPatch
{
    [HarmonyPatch(typeof(RaDummyActions), nameof(RaDummyActions.GatherData))]
    private static bool Prefix(RaDummyActions __instance)
    {
        foreach (var hub in ReferenceHub.AllHubs)
        {
            if (hub.IsHost)
                continue;
            
            __instance.AppendDummy(hub);
        }

        return false;
    }
}