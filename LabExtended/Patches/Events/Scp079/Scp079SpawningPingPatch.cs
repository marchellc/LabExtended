using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Enums;

using LabExtended.Events;
using LabExtended.Events.Scp079;

using Mirror;

using PlayerRoles.PlayableScps.Scp079.Pinging;

using RelativePositioning;

namespace LabExtended.Patches.Events.Scp079;

/// <summary>
/// Implements the <see cref="ExScp079Events.SpawningPing"/> and <see cref="ExScp079Events.SpawnedPing"/> events.
/// </summary>
public static class Scp079SpawningPingPatch
{
    [HarmonyPatch(typeof(Scp079PingAbility), nameof(Scp079PingAbility.ServerProcessCmd))]
    private static bool Prefix(Scp079PingAbility __instance, NetworkReader reader)
    {
        if (!__instance.IsReady || !__instance.Role.TryGetOwner(out var owner) || __instance.LostSignalHandler.Lost)
            return false;

        if (!ExPlayer.TryGet(owner, out var player))
            return false;

        var processorIndex = reader.ReadByte();

        if (processorIndex < Scp079PingAbility.PingProcessors.Length)
        {
            var processorType = (Scp079PingType)processorIndex;
            var processorPosition = reader.ReadRelativePosition();
            var processorNormal = reader.ReadVector3();

            var spawningArgs = new Scp079SpawningPingEventArgs(player, __instance.CastRole, __instance, processorType,
                processorPosition.Position, __instance._cost);

            if (!ExScp079Events.OnSpawningPing(spawningArgs))
                return false;

            __instance._syncProcessorIndex = (byte)spawningArgs.PingType;
            __instance._syncNormal = processorNormal;
            
            if (spawningArgs.Position != processorPosition.Position)
                __instance._syncPos = new(spawningArgs.Position);
            else
                __instance._syncPos = processorPosition;
            
            __instance.ServerSendRpc(x => __instance.ServerCheckReceiver(x, __instance._syncPos.Position, __instance._syncProcessorIndex));

            if (spawningArgs.AuxCost > 0f)
                __instance.AuxManager.CurrentAux -= spawningArgs.AuxCost;
            
            __instance._rateLimiter.RegisterInput();
            
            ExScp079Events.OnSpawnedPing(new(player, __instance.CastRole, __instance, spawningArgs.PingType, spawningArgs.Position));
        }

        return false;
    }
}