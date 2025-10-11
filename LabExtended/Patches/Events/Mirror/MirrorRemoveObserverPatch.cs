using HarmonyLib;

using LabExtended.API;

using LabExtended.Events;
using LabExtended.Events.Mirror;

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

            var player = ExPlayer.Get(conn);

            if (!MirrorEvents.OnRemovingObserver(new MirrorRemovingObserverEventArgs(__instance, player, conn)))
                return false;

            if (__instance.observers.Remove(conn.connectionId))
                MirrorEvents.OnRemovedObserver(new MirrorRemovedObserverEventArgs(__instance, player, conn));

            return false;
        }

        [HarmonyPatch(typeof(NetworkIdentity), nameof(NetworkIdentity.ClearObservers))]
        private static bool ClearObserversPrefix(NetworkIdentity __instance)
        {
            foreach (var pair in __instance.observers.ToList())
            {
                if (pair.Value is null)
                    continue;

                var player = ExPlayer.Get(pair.Value);

                if (!MirrorEvents.OnRemovingObserver(new MirrorRemovingObserverEventArgs(__instance, player, pair.Value)))
                    continue;

                if (__instance.observers.Remove(pair.Key))
                    MirrorEvents.OnRemovedObserver(new MirrorRemovedObserverEventArgs(__instance, player, pair.Value));
            }

            return false;
        }
    }
}