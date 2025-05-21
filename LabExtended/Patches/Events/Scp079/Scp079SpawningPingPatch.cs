using HarmonyLib;

using LabExtended.API;

using LabExtended.Events;
using LabExtended.Events.Scp079;

using LabExtended.Extensions;

using Mirror;

using PlayerRoles.PlayableScps.Scp079.Pinging;

using RelativePositioning;

namespace LabExtended.Patches.Events.Scp079
{
    public static class Scp079SpawningPingPatch
    {
        [HarmonyPatch(typeof(Scp079PingAbility), nameof(Scp079PingAbility.ServerProcessCmd))]
        public static bool Prefix(Scp079PingAbility __instance, NetworkReader reader)
        {
            if (!__instance.IsReady || !__instance.Role.TryGetOwner(out var owner) || __instance.LostSignalHandler.Lost)
                return false;

            var player = ExPlayer.Get(owner);

            if (player is null)
                return true;

            __instance._syncProcessorIndex = reader.ReadByte();

            if (__instance._syncProcessorIndex < Scp079PingAbility.PingProcessors.Length)
            {
                var processor = Scp079PingAbility.PingProcessors[__instance._syncProcessorIndex];

                var position = reader.ReadRelativePosition();
                var normal = reader.ReadVector3();

                var ev = new Scp079SpawningPingEventArgs(player, __instance.CastRole, __instance, processor.GetPingType(), position.Position, __instance._cost);

                if (!ExScp079Events.OnSpawningPing(ev))
                    return false;

                __instance._syncIndex = (byte)ev.PingType;
                __instance._syncPos = new RelativePosition(ev.Position);
                __instance._syncNormal = normal;

                __instance.ServerSendRpc(hub => __instance.ServerCheckReceiver(hub, __instance._syncPos.Position, __instance._syncProcessorIndex));

                if (ev.AuxCost > 0f)
                    __instance.AuxManager.CurrentAux -= ev.AuxCost;

                __instance._rateLimiter.RegisterInput();
                
                ExScp079Events.OnSpawnedPing(new(player, __instance.CastRole, __instance, ev.PingType, ev.Position));
            }

            return false;
        }
    }
}