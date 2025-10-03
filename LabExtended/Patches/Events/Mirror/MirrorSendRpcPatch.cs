using HarmonyLib;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Events;

using Mirror;

using NorthwoodLib.Pools;

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
            if (!__instance.isServer)
            {
                ApiLog.Warn("Mirror", "ClientRpc " + functionFullName + " called on un-spawned object: " + __instance.name);
                return false;
            }

            var rpcMessage = new RpcMessage
            {
                netId = __instance.netId,
                componentIndex = __instance.ComponentIndex,
                functionHash = (ushort)functionHashCode,
                payload = writer.ToArraySegment()
            };

            if (__instance.netIdentity.observers == null || __instance.netIdentity.observers.Count == 0)
                return false;

            var players = ListPool<ExPlayer>.Shared.Rent();
            var connections = ListPool<NetworkConnection>.Shared.Rent();

            foreach (var pair in __instance.netIdentity.observers)
            {
                if (ExPlayer.TryGet(pair.Value, out var player))
                {
                    players.Add(player);
                }
                else
                {
                    if (__instance.connectionToClient == pair.Value && !includeOwner)
                        continue;

                    connections.Add(pair.Value);
                }
            }

            if (MirrorEvents.OnSendingRpc(__instance, functionFullName, functionHashCode, writer, connections, players, ref rpcMessage))
            {
                using var writer2 = NetworkWriterPool.Get();

                writer2.Write(rpcMessage);

                players.ForEach(ply =>
                {
                    if (ply.Connection.isReady)
                    {
                        ply.Connection.Send(rpcMessage, channelId);
                    }
                });

                connections.ForEach(conn =>
                {
                    if (conn.isReady)
                    {
                        conn.Send(rpcMessage, channelId);
                    }
                });

                MirrorEvents.OnSentRpc(__instance, functionFullName, functionHashCode, writer, players, ref rpcMessage);
            }

            ListPool<ExPlayer>.Shared.Return(players);
            ListPool<NetworkConnection>.Shared.Return(connections);

            return false;
        }

        [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.SendTargetRPCInternal))]
        private static bool SendTargetRpcPrefix(NetworkBehaviour __instance, NetworkConnection conn, string functionFullName, int functionHashCode, 
            NetworkWriter writer, int channelId)
        {
            if (!__instance.isServer)
            {
                ApiLog.Warn("Mirror", "ClientRpc " + functionFullName + " called on un-spawned object: " + __instance.name);
                return false;
            }

            conn ??= __instance.connectionToClient;

            if (conn is null)
                return false;

            if (!ExPlayer.TryGet(conn, out var player))
                return true;

            var rpcMessage = new RpcMessage
            {
                netId = __instance.netId,
                componentIndex = __instance.ComponentIndex,
                functionHash = (ushort)functionHashCode,
                payload = writer.ToArraySegment()
            };

            var players = ListPool<ExPlayer>.Shared.Rent();
            var connections = ListPool<NetworkConnection>.Shared.Rent();

            players.Add(player);
            connections.Add(conn);

            if (MirrorEvents.OnSendingRpc(__instance, functionFullName, functionHashCode, writer, connections, players, ref rpcMessage))
            {
                conn.Send(rpcMessage, channelId);

                MirrorEvents.OnSentRpc(__instance, functionFullName, functionHashCode, writer, players, ref rpcMessage);
            }

            ListPool<ExPlayer>.Shared.Return(players);
            ListPool<NetworkConnection>.Shared.Return(connections);

            return false;
        }
    }
}