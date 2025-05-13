using HarmonyLib;

using NetworkManagerUtils.Dummies;

namespace LabExtended.Patches.Functions.RemoteAdmin;

/// <summary>
/// Implements custom Remote Admin actions.
/// </summary>
public static class RemoteAdminDummyGetCachePatch
{
    [HarmonyPatch(typeof(DummyActionCollector), nameof(DummyActionCollector.GetCache))]
    private static bool Prefix(ReferenceHub hub, ref DummyActionCollector.CachedActions __result)
    {
        if (hub == null || hub.IsHost)
            throw new ArgumentException("Provided argument is null", nameof(hub));

        if (!DummyActionCollector.CollectionCache.TryGetValue(hub, out __result))
            DummyActionCollector.CollectionCache.Add(hub, __result = new(hub));

        return false;
    }
}