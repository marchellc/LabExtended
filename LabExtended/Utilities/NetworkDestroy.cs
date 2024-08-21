using LabExtended.API.Collections.Locked;

using Mirror;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.API;

namespace LabExtended.Utilities
{
    public static class NetworkDestroy
    {
        private static readonly LockedDictionary<uint, List<Action<NetworkIdentity>>> _disposeHandlers = new LockedDictionary<uint, List<Action<NetworkIdentity>>>();

        public static event Action<NetworkIdentity> OnIdentityDestroyed;

        public static void Subscribe(uint netId, Action<NetworkIdentity> handler)
        {
            if (netId == 0)
                throw new ArgumentOutOfRangeException(nameof(netId));

            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            if (!_disposeHandlers.TryGetValue(netId, out var handlers))
                _disposeHandlers[netId] = handlers = new List<Action<NetworkIdentity>>();

            if (handlers.Contains(handler))
                return;

            handlers.Add(handler);
        }

        public static void Unsubscribe(uint netId, Action<NetworkIdentity> handler)
        {
            if (netId == 0)
                throw new ArgumentOutOfRangeException(nameof(netId));

            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            if (!_disposeHandlers.TryGetValue(netId, out var handlers))
                return;

            handlers.Remove(handler);
        }

        internal static void InternalHandleDestroy(NetworkIdentity identity)
        {
            try
            {
                if (_disposeHandlers.TryGetValue(identity.netId, out var handlers))
                {
                    foreach (var handler in handlers)
                    {
                        try
                        {
                            handler(identity);
                        }
                        catch (Exception ex)
                        {
                            ApiLoader.Error("Network API", $"Failed to invoke destruction handler &3{handler.Method.GetMemberName()}&r!\n{ex.ToColoredString()}");
                        }
                    }
                }

                _disposeHandlers.Remove(identity.netId);

                ExMap.OnIdentityDestroyed(identity);

                OnIdentityDestroyed?.Invoke(identity);
            }
            catch (Exception ex)
            {
                ApiLoader.Error("Network API", $"Failed to handle identity destruction!\n{ex.ToColoredString()}");
            }
        }
    }
}