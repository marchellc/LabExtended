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

            if (__instance.netIdentity.observers == null || __instance.netIdentity.observers.Count == 0)
                return false;

            var rpcMessage = new RpcMessage
            {
                netId = __instance.netId,
                componentIndex = __instance.ComponentIndex,

                functionHash = (ushort)functionHashCode,
                payload = writer.ToArraySegment()
            };

            var players = ListPool<ExPlayer>.Shared.Rent();

            for (var i = 0; i < ExPlayer.Players.Count; i++)
            {
                var player = ExPlayer.Players[i];

                if (player?.ReferenceHub == null || !player.Connection.isReady)
                    continue;

                if (__instance.netIdentity.connectionToClient != null && __instance.netIdentity.connectionToClient == player.Connection && !includeOwner)
                    continue;

                if (!__instance.netIdentity.observers.ContainsKey(player.ConnectionId))
                    continue;

                players.Add(player);
            }

            if (players.Count > 0)
            {
                if (MirrorEvents.OnSendingRpc(__instance, functionFullName, functionHashCode, writer, players, ref rpcMessage)
                    && players.Count > 0)
                {
                    using var writer2 = NetworkWriterPool.Get();

                    writer2.Write(rpcMessage);

                    var segment = writer2.ToArraySegment();

                    players.ForEach(ply => ply.ConnectionToClient.Send(segment, channelId));

                    MirrorEvents.OnSentRpc(__instance, functionFullName, functionHashCode, writer, players, ref rpcMessage);
                }
            }

            ListPool<ExPlayer>.Shared.Return(players);
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

            players.Add(player);

            if (MirrorEvents.OnSendingRpc(__instance, functionFullName, functionHashCode, writer, players, ref rpcMessage)
                && players.Count > 0)
            {
                using var writer2 = NetworkWriterPool.Get();

                writer2.Write(rpcMessage);

                var segment = writer2.ToArraySegment();

                player.ConnectionToClient.Send(segment, channelId);

                MirrorEvents.OnSentRpc(__instance, functionFullName, functionHashCode, writer, players, ref rpcMessage);
            }

            ListPool<ExPlayer>.Shared.Return(players);
            return false;
        }
    }
}