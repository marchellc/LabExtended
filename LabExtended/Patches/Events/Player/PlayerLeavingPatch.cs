using HarmonyLib;

using LabExtended.API;
using LabExtended.Extensions;
using LabExtended.Attributes;

using LabExtended.Events;
using LabExtended.Events.Player;

using LiteNetLib;

using Mirror.LiteNetLib4Mirror;

namespace LabExtended.Patches.Events.Player
{
    public static class PlayerLeavingPatch
    {
        [EventPatch(typeof(PlayerLeavingEventArgs))]
        [HarmonyPatch(typeof(LiteNetLib4MirrorServer), nameof(LiteNetLib4MirrorServer.OnPeerDisconnected))]
        public static bool Prefix(NetPeer peer, DisconnectInfo disconnectinfo)
        {
            ExPlayer player = null;
            
            if ((player = ExPlayer.Get(peer)) is null)
                return true;
            
            ExPlayerEvents.OnLeaving(new(player, disconnectinfo is { Reason: DisconnectReason.Timeout }, disconnectinfo));
            
            LiteNetLib4MirrorCore.LastDisconnectError = disconnectinfo.SocketErrorCode;
            LiteNetLib4MirrorCore.LastDisconnectReason = disconnectinfo.Reason;
            
            LiteNetLib4MirrorTransport.Singleton.OnServerDisconnected.InvokeSafe(peer.Id + 1);
            return false;
        }
    }
}
