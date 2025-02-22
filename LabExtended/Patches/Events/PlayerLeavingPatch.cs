﻿using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

using LiteNetLib;

using Mirror.LiteNetLib4Mirror;

namespace LabExtended.Patches.Events
{
    public static class PlayerLeavingPatch
    {
        [HookPatch(typeof(PlayerLeavingArgs))]
        [HarmonyPatch(typeof(LiteNetLib4MirrorServer), nameof(LiteNetLib4MirrorServer.OnPeerDisconnected))]
        public static bool Prefix(NetPeer peer, DisconnectInfo disconnectinfo)
        {
            if (peer is null)
                return true;

            var player = ExPlayer.Get(peer);

            if (player is null)
                return true;

            HookRunner.RunEvent(new PlayerLeavingArgs(player, disconnectinfo.Reason is DisconnectReason.Timeout, disconnectinfo));
            return true;
        }
    }
}
