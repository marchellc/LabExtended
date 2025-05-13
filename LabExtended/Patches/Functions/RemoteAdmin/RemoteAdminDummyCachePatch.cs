using HarmonyLib;

using LabExtended.API;
using LabExtended.Extensions;

using NetworkManagerUtils.Dummies;

using NorthwoodLib.Pools;

namespace LabExtended.Patches.Functions.RemoteAdmin;

/// <summary>
/// Implements custom Remote Admin actions.
/// </summary>
public static class RemoteAdminDummyCachePatch
{
    private static AccessTools.FieldRef<DummyActionCollector.CachedActions, IRootDummyActionProvider[]> Providers =
        AccessTools.FieldRefAccess<DummyActionCollector.CachedActions, IRootDummyActionProvider[]>(
            nameof(DummyActionCollector.CachedActions._providers));

    [HarmonyPatch(typeof(DummyActionCollector.CachedActions), MethodType.Constructor, typeof(ReferenceHub))]
    private static void Postfix(DummyActionCollector.CachedActions __instance, ReferenceHub hub)
    {
        if (hub.IsDummy || hub.IsHost)
            return;

        if (!ExPlayer.TryGet(hub, out var player))
            return;

        var providers = ListPool<IRootDummyActionProvider>.Shared.Rent();
        
        providers.Add(player.RemoteAdmin.Actions);

        Providers(__instance) = ListPool<IRootDummyActionProvider>.Shared.ToArrayReturn(providers);
    }
}