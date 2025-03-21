using HarmonyLib;

using LabApi.Features.Wrappers;

using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Events;

namespace LabExtended.Patches.Functions.Players;

public static class HostPlayerPatch
{
    [HarmonyPatch(typeof(Server), nameof(Server.AddHost))]
    public static bool JoinPrefix(ReferenceHub referenceHub)
    {
        if (referenceHub != null && referenceHub.isLocalPlayer)
        {
            if (ExPlayer.host != null)
            {
                Server.Host = ExPlayer.host;
                return false;
            }
            
            var player = new ExPlayer(referenceHub, SwitchContainer.GetNewNpcToggles(true));

            Server.Host = player;
            ExPlayer.host = player;
        }

        return false;
    }

    [HarmonyPatch(typeof(Server), nameof(Server.RemoveHost))]
    public static bool LeavePrefix(ReferenceHub referenceHub)
    {
        if (referenceHub != null && referenceHub.isLocalPlayer && Server.Host is ExPlayer hostPlayer)
        {
            ExPlayer.host = null;
            
            InternalEvents.HandlePlayerLeave(hostPlayer);
            
            hostPlayer.Dispose();

            Server.Host = null;
        }

        return false;
    }
}