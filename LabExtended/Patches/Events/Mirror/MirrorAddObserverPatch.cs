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
        var addingEventArgs = MirrorAddingObserverEventArgs.Singleton;

        addingEventArgs.IsAllowed = true;
        addingEventArgs.Identity = __instance;
        addingEventArgs.Observer = observer;
        addingEventArgs.Connection = conn;

        if (!MirrorEvents.OnAddingObserver(addingEventArgs))
            return false;
        
        if (__instance.observers.Count == 0)
            __instance.ClearAllComponentsDirtyBits();
        
        __instance.observers.Add(conn.connectionId, conn);
        
        conn.AddToObserving(__instance);

        var addedEventArgs = MirrorAddedObserverEventArgs.Singleton;

        addedEventArgs.Identity = __instance;
        addedEventArgs.Observer = observer;
        addedEventArgs.Connection = conn;

        MirrorEvents.OnAddedObserver(addedEventArgs);
        return false;
    }
}