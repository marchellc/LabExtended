using HarmonyLib;

using LabExtended.API;

using LabExtended.Core;
using LabExtended.Core.Networking;

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
        if (!ExPlayer.TryGet(conn, out var player))
        {
            ApiLog.Debug("AddObserverPatch", $"Could not get observer for connection &6{conn.connectionId}&r (&6{conn.address ?? "null"}&r)");
            return true;
        }
        
        if (__instance.observers.ContainsKey(conn.connectionId))
            return false;

        if (!MirrorEvents.OnAddingObserver(new(player, __instance)))
            return false;
        
        if (__instance.observers.Count == 0)
            __instance.ClearAllComponentsDirtyBits();
        
        __instance.observers.Add(conn.connectionId, conn);
        
        conn.AddToObserving(__instance);
        return false;
    }
}