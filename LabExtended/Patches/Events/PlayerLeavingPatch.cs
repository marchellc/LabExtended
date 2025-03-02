using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using LiteNetLib;

using Mirror.LiteNetLib4Mirror;

namespace LabExtended.Patches.Events
{
    public static class PlayerLeavingPatch
    {
        [HookPatch(typeof(PlayerLeavingArgs), true)]
        [HarmonyPatch(typeof(LiteNetLib4MirrorServer), nameof(LiteNetLib4MirrorServer.OnPeerDisconnected))]
        public static bool Prefix(NetPeer peer, DisconnectInfo disconnectinfo)
        {
            ExPlayer player = null;
            
            if ((player = ExPlayer.Get(peer)) is null)
                return true;

            HookRunner.RunEvent(new PlayerLeavingArgs(player, disconnectinfo.Reason is DisconnectReason.Timeout, disconnectinfo));
            
            LiteNetLib4MirrorCore.LastDisconnectError = disconnectinfo.SocketErrorCode;
            LiteNetLib4MirrorCore.LastDisconnectReason = disconnectinfo.Reason;
            
            LiteNetLib4MirrorTransport.Singleton.OnServerDisconnected.InvokeSafe(peer.Id + 1);
            return false;
        }
    }
}
