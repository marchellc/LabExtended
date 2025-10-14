using HarmonyLib;

using LabExtended.API;

using LabExtended.Events;
using LabExtended.Events.Mirror;

using Mirror;

namespace LabExtended.Patches.Events.Mirror
{
    /// <summary>
    /// Implements the <see cref="MirrorEvents.SendingRpc"/> and <see cref="MirrorEvents.SentRpc"/> events.
    /// </summary>
    public static class MirrorSendRpcPatch
    {
        [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.SendRPCInternal))]
        private static bool SendAllRpcPrefix(NetworkBehaviour __instance, string functionFullName, int functionHashCode, NetworkWriter writer,
            int channelId, bool includeOwner)
        {
            if (__instance.netIdentity.observers?.Count < 1)
                return false;

            var sendingRpcEventArgs = MirrorSendingRpcEventArgs.Singleton;

            sendingRpcEventArgs.IsAllowed = true;
            sendingRpcEventArgs.Identity = __instance.netIdentity;
            sendingRpcEventArgs.Writer = writer;
            sendingRpcEventArgs.Behaviour = __instance;
            sendingRpcEventArgs.RpcHash = functionHashCode;
            sendingRpcEventArgs.RpcName = functionFullName;

            var sentRpcEventArgs = MirrorSentRpcEventArgs.Singleton;

            sendingRpcEventArgs.Identity = __instance.netIdentity;
            sendingRpcEventArgs.Writer = writer;
            sendingRpcEventArgs.Behaviour = __instance;
            sendingRpcEventArgs.RpcHash = functionHashCode;
            sendingRpcEventArgs.RpcName = functionFullName;

            foreach (var pair in __instance.netIdentity.observers!)
            {
                if (__instance.connectionToClient == pair.Value && !includeOwner)
                    continue;

                if (!ExPlayer.TryGet(pair.Value, out var player))
                {
                    pair.Value.Send(new RpcMessage
                    {
                        netId = __instance.netId,
                        componentIndex = __instance.ComponentIndex,
                        functionHash = (ushort)functionHashCode,
                        payload = writer.ToArraySegment()
                    });
                }
                else
                {
                    sendingRpcEventArgs.IsAllowed = true;
                    sendingRpcEventArgs.Player = player;

                    if (!MirrorEvents.OnSendingRpc(sendingRpcEventArgs))
                        continue;

                    pair.Value.Send(new RpcMessage
                    {
                        netId = __instance.netId,
                        componentIndex = __instance.ComponentIndex,
                        functionHash = (ushort)functionHashCode,
                        payload = sendingRpcEventArgs.Writer.ToArraySegment()
                    });

                    sentRpcEventArgs.Player = player;
                    sentRpcEventArgs.Writer = sendingRpcEventArgs.Writer;

                    MirrorEvents.OnSentRpc(sentRpcEventArgs);
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.SendTargetRPCInternal))]
        private static bool SendTargetRpcPrefix(NetworkBehaviour __instance, NetworkConnection conn, string functionFullName, int functionHashCode, 
            NetworkWriter writer, int channelId)
        {
            conn ??= __instance.connectionToClient;

            if (conn is null)
                return false;

            if (ExPlayer.TryGet(conn, out var player))
            {
                var sendingRpcEventArgs = MirrorSendingRpcEventArgs.Singleton;

                sendingRpcEventArgs.IsAllowed = true;
                sendingRpcEventArgs.Identity = __instance.netIdentity;
                sendingRpcEventArgs.Writer = writer;
                sendingRpcEventArgs.Behaviour = __instance;
                sendingRpcEventArgs.RpcHash = functionHashCode;
                sendingRpcEventArgs.RpcName = functionFullName;

                if (!MirrorEvents.OnSendingRpc(sendingRpcEventArgs))
                    return false;

                conn.Send(new RpcMessage
                {
                    netId = __instance.netId,
                    componentIndex = __instance.ComponentIndex,
                    functionHash = (ushort)functionHashCode,
                    payload = sendingRpcEventArgs.Writer.ToArraySegment()
                });

                var sentRpcEventArgs = MirrorSentRpcEventArgs.Singleton;

                sendingRpcEventArgs.Identity = __instance.netIdentity;
                sendingRpcEventArgs.Writer = writer;
                sendingRpcEventArgs.Behaviour = __instance;
                sendingRpcEventArgs.RpcHash = functionHashCode;
                sendingRpcEventArgs.RpcName = functionFullName;

                MirrorEvents.OnSentRpc(sentRpcEventArgs);
            }
            else
            {
                conn.Send(new RpcMessage
                {
                    netId = __instance.netId,
                    componentIndex = __instance.ComponentIndex,
                    functionHash = (ushort)functionHashCode,
                    payload = writer.ToArraySegment()
                });
            }

            return false;
        }
    }
}