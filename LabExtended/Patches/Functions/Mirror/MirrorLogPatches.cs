using HarmonyLib;

using Mirror;

namespace LabExtended.Patches.Functions.Mirror
{
    /// <summary>
    /// Removes pointless Mirror log spam.
    /// </summary>
    public static class MirrorLogPatches
    {
        [HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.BroadcastToConnection))]
        private static bool BroadcastToConnectionPrefix(NetworkConnectionToClient connection)
        {
            foreach (var networkIdentity in connection.observing)
            {
                if (networkIdentity != null)
                {
                    var networkWriter = NetworkServer.SerializeForConnection(networkIdentity, connection);

                    if (networkWriter != null)
                    {
                        connection.Send(new EntityStateMessage
                        {
                            netId = networkIdentity.netId,
                            payload = networkWriter.ToArraySegment()
                        });
                    }
                }
            }

            return false;
        }
    }
}