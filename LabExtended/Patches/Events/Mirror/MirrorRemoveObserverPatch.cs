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
            var removingArgs = MirrorRemovingObserverEventArgs.Singleton;

            removingArgs.IsAllowed = true;
            removingArgs.Identity = __instance;
            removingArgs.Connection = conn;
            removingArgs.Observer = player;

            if (!MirrorEvents.OnRemovingObserver(removingArgs))
                return false;

            if (__instance.observers.Remove(conn.connectionId))
            {
                var removedArgs = MirrorRemovedObserverEventArgs.Singleton;

                removedArgs.Identity = __instance;
                removedArgs.Connection = conn;
                removedArgs.Observer = player;

                MirrorEvents.OnRemovedObserver(removedArgs);
            }

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
                var removingArgs = MirrorRemovingObserverEventArgs.Singleton;

                removingArgs.IsAllowed = true;
                removingArgs.Identity = __instance;
                removingArgs.Connection = pair.Value;
                removingArgs.Observer = player;

                if (!MirrorEvents.OnRemovingObserver(removingArgs))
                    continue;

                if (__instance.observers.Remove(pair.Key))
                {
                    var removedArgs = MirrorRemovedObserverEventArgs.Singleton;

                    removedArgs.Identity = __instance;
                    removedArgs.Connection = pair.Value;
                    removedArgs.Observer = player;

                    MirrorEvents.OnRemovedObserver(removedArgs);
                }
            }

            return false;
        }
    }
}