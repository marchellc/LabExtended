using LabExtended.Extensions;

using Mirror;

namespace LabExtended.Core.Networking
{
    public static class MirrorEvents
    {
        public static event Action<NetworkIdentity> OnDestroy;
        public static event Action<NetworkIdentity> OnSpawn;

        internal static void InternalInvokeDestroy(NetworkIdentity identity)
            => OnDestroy.InvokeSafe(identity);

        internal static void InternalInvokeSpawn(NetworkIdentity identity)
            => OnSpawn.InvokeSafe(identity);
    }
}