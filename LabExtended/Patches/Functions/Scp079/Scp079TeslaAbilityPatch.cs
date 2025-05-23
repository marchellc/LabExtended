﻿using HarmonyLib;

using LabApi.Events.Arguments.Scp079Events;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Extensions;

using MapGeneration;

using Mirror;

using PlayerRoles.PlayableScps.Scp079;

namespace LabExtended.Patches.Functions.Scp079;

public static class Scp079TeslaAbilityPatch
{
    [HarmonyPatch(typeof(Scp079TeslaAbility), nameof(Scp079TeslaAbility.ServerProcessCmd))]
    public static bool Prefix(Scp079TeslaAbility __instance, NetworkReader reader)
    {
        if (!__instance.IsReady)
            return false;

        var camera = __instance.CurrentCamSync.CurrentCamera;

        if (camera is null)
            return false;

        if (!ExTeslaGate.Lookup.TryGetFirst(
                x => RoomUtils.CompareCoords(x.Value.Position, camera.Position), out var gate))
            return false;

        if (gate.Value.IsDisabled)
            return false;

        var usingTeslaArgs = new Scp079UsingTeslaEventArgs(__instance.Owner, gate.Value.Base);
        
        Scp079Events.OnUsingTesla(usingTeslaArgs);

        if (!usingTeslaArgs.IsAllowed)
            return false;
        
        __instance.RewardManager.MarkRoom(camera.Room);
        __instance.AuxManager.CurrentAux -= __instance._cost;
        
        gate.Value.Base.RpcInstantBurst();

        __instance._nextUseTime = NetworkTime.time + __instance._cooldown;
        __instance.ServerSendRpc(false);
        
        Scp079Events.OnUsedTesla(new(__instance.Owner, gate.Value.Base));
        return false;
    }
}