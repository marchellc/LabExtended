using CustomPlayerEffects;

using HarmonyLib;

using LabExtended.API;

using Mirror;

using PluginAPI.Events;

using UnityEngine;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(StatusEffectBase), nameof(StatusEffectBase.ForceIntensity))]
    public static class EnablingEffectPatch
    {
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
                var effectEvent = new PlayerReceiveEffectEvent(__instance.Hub, __instance, value, __instance.Duration);

                if (!EventManager.ExecuteEvent(effectEvent))
                    return false;

                value = effectEvent.Intensity;

                __instance.Duration = effectEvent.Duration;
            }

            __instance._intensity = (byte)Mathf.Min(value, __instance.MaxIntensity);

            if (serverActive)
                __instance.Hub.playerEffectsController.ServerSyncEffect(__instance);

            if (isEnabling)
                __instance.Enabled();
            else if (prevIntensity > 0 && value == 0)
                __instance.Disabled();

            __instance.IntensityChanged(prevIntensity, value);
            return false;
        }
    }
}