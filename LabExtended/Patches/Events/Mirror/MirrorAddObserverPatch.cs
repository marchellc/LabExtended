using HarmonyLib;

using LabExtended.API;

using LabExtended.Events;
using LabExtended.Events.Mirror;

using Mirror;

namespace LabExtended.Patches.Events.Mirror;

/// <summary>
/// Implements the <see cref="MirrorEvents.AddingObserver"/> and <see cref="MirrorEvents.AddedObserver"/> events.
/// </summary>
public static class MirrorAddObserverPatch
{
    [HarmonyPatch(typeof(NetworkIdentity), nameof(NetworkIdentity.AddObserver))]
    private static bool Prefix(NetworkIdentity __instance, NetworkConnectionToClient conn)
    {
        if (__instance.observers.ContainsKey(conn.connectionId))
            return false;

        var observer = ExPlayer.Get(conn);

        if (!MirrorEvents.OnAddingObserver(new MirrorAddingObserverEventArgs(__instance, observer, conn)))
            return false;
        
        if (__instance.observers.Count == 0)
            __instance.ClearAllComponentsDirtyBits();
        
        __instance.observers.Add(conn.connectionId, conn);
        
        conn.AddToObserving(__instance);

        MirrorEvents.OnAddedObserver(new MirrorAddedObserverEventArgs(__instance, observer, conn));
        return false;
    }
}