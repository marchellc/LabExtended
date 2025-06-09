using HarmonyLib;

using LabExtended.API;
using LabExtended.Events;

using Mirror;

namespace LabExtended.Patches.Events.Mirror;

/// <summary>
/// Implements the <see cref="MirrorEvents.AddingObserver"/> event.
/// </summary>
public static class AddObserverPatch
{
    [HarmonyPatch(typeof(NetworkIdentity), nameof(NetworkIdentity.AddObserver))]
    private static bool Prefix(NetworkIdentity __instance, NetworkConnectionToClient conn)
    {
        if (__instance.observers.ContainsKey(conn.connectionId))
            return false;
        
        if (!ExPlayer.TryGet(conn, out var player))
            return true;

        if (!MirrorEvents.OnAddingObserver(new(player, __instance)))
            return false;
        
        if (__instance.observers.Count == 0)
            __instance.ClearAllComponentsDirtyBits();
        
        __instance.observers.Add(conn.connectionId, conn);
        
        conn.AddToObserving(__instance);
        return false;
    }
}