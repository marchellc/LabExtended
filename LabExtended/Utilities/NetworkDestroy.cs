using HarmonyLib;

using LabExtended.API.Collections.Locked;

using Mirror;

namespace LabExtended.Utilities
{
    public static class NetworkDestroy
    {
        private static readonly LockedDictionary<uint, List<Action<NetworkIdentity>>> _disposeHandlers = new LockedDictionary<uint, List<Action<NetworkIdentity>>>();

        public static event Action<NetworkIdentity> OnIdentityDestroyed;
        public static event Action<NetworkIdentity, uint, uint> OnIdentitySet;

        public static void Subscribe(uint netId, Action<NetworkIdentity> handler)
        {
            if (!_disposeHandlers.TryGetValue(netId, out var handlers))
                _disposeHandlers[netId] = handlers = new List<Action<NetworkIdentity>>();

            if (handlers.Contains(handler))
                return;

            handlers.Add(handler);
        }

        public static void Unsubscribe(uint netId, Action<NetworkIdentity> handler)
        {
            if (!_disposeHandlers.TryGetValue(netId, out var handlers))
                return;

            handlers.Remove(handler);
        }

        [HarmonyPatch(typeof(NetworkIdentity), nameof(NetworkIdentity.netId), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool PropertyPrefix(NetworkIdentity __instance, uint value)
        {
            try
            {
                if (__instance.netId != 0)
                    _disposeHandlers.Remove(__instance.netId);

                OnIdentitySet?.Invoke(__instance, __instance.netId, value);

                _disposeHandlers.Remove(value);
            }
            catch { }

            return true;
        }

        [HarmonyPatch(typeof(NetworkIdentity), nameof(NetworkIdentity.OnDestroy))]
        [HarmonyPrefix]
        private static bool DestroyPrefix(NetworkIdentity __instance)
        {
            try
            {
                if (_disposeHandlers.TryGetValue(__instance.netId, out var handlers))
                {
                    foreach (var handler in handlers)
                        handler?.Invoke(__instance);
                }

                _disposeHandlers.Remove(__instance.netId);

                OnIdentityDestroyed?.Invoke(__instance);
            }
            catch { }

            return true;
        }
    }
}