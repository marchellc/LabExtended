using CustomPlayerEffects;

using HarmonyLib;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;

using Mirror;

using UnityEngine;

namespace LabExtended.Patches.Functions.Players
{
    public static class EnablingEffectPatch
    {
        [HarmonyPatch(typeof(StatusEffectBase), nameof(StatusEffectBase.ForceIntensity))]
        public static bool Prefix(StatusEffectBase __instance, byte value)
        {
            if (__instance.Intensity == value)
                return false;

            if (!ExPlayer.TryGet(__instance.Hub, out var player))
                return true;

            if (!player.Switches.CanReceiveEffects || player.Switches.IgnoredEffects.Contains(__instance.GetType()))
                return false;

            var prevIntensity = __instance.Intensity;
            var serverActive = NetworkServer.active;
            var isEnabling = prevIntensity == 0 && value > 0;

            if (serverActive && isEnabling)
            {
                var receivingArgs =
                    new PlayerEffectUpdatingEventArgs(player.Hub, __instance, value, __instance.Duration);

                PlayerEvents.OnUpdatingEffect(receivingArgs);

                if (!receivingArgs.IsAllowed)
                    return false;

                value = receivingArgs.Intensity;

                if (__instance.Duration != receivingArgs.Duration)
                    __instance.Duration = receivingArgs.Duration;
            }

            __instance._intensity = (byte)Mathf.Min(value, __instance.MaxIntensity);

            if (serverActive)
                __instance.Hub.playerEffectsController.ServerSyncEffect(__instance);

            if (isEnabling)
                __instance.Enabled();
            else if (prevIntensity > 0 && value == 0)
                __instance.Disabled();

            __instance.IntensityChanged(prevIntensity, value);

            PlayerEvents.OnUpdatedEffect(new PlayerEffectUpdatedEventArgs(player.Hub, __instance, value, __instance.Duration));
            return false;
        }
    }
}