using HarmonyLib;

using LabExtended.API;
using LabExtended.Events;

using Mirror;

namespace LabExtended.Patches.Events.Mirror
{
    /// <summary>
    /// Implements the <see cref="MirrorEvents.RemovingObserver"/> and <see cref="MirrorEvents.RemovedObserver"/> events.
    /// </summary>
    public static class MirrorRemoveObserverPatch
    {
        [HarmonyPatch(typeof(NetworkIdentity), nameof(NetworkIdentity.RemoveObserver))]
        private static bool RemoveObserverPrefix(NetworkIdentity __instance, NetworkConnection conn)
        {
            if (conn is null)
                return false;

            if (!ExPlayer.TryGet(conn, out var player))
                return true;

            if (!MirrorEvents.OnRemovingObserver(__instance, player))
                return false;

            if (__instance.observers.Remove(conn.connectionId))
                MirrorEvents.OnRemovedObserver(__instance, player);

            return false;
        }

        [HarmonyPatch(typeof(NetworkIdentity), nameof(NetworkIdentity.ClearObservers))]
        private static bool ClearObserversPrefix(NetworkIdentity __instance)
        {
            foreach (var conn in __instance.observers.Values)
            {
                if (conn is null)
                    continue;

                if (ExPlayer.TryGet(conn, out var player))
                {
                    if (!MirrorEvents.OnRemovingObserver(__instance, player))
                        continue;

                    __instance.observers.Remove(conn.connectionId);

                    MirrorEvents.OnRemovedObserver(__instance, player);
                }
                else
                {
                    __instance.observers.Remove(conn.connectionId);
                }
            }

            return false;
        }
    }
}