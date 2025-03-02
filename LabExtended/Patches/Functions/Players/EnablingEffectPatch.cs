using CustomPlayerEffects;

using HarmonyLib;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Utilities;

using UnityEngine;

namespace LabExtended.Patches.Functions.Players
{
    public static class EnablingEffectPatch
    {
        public static FastEvent<Action<StatusEffectBase>> OnEnabled { get; } =
            FastEvents.DefineEvent<Action<StatusEffectBase>>(typeof(StatusEffectBase),
                nameof(StatusEffectBase.OnEnabled));
        
        public static FastEvent<Action<StatusEffectBase>> OnDisabled { get; } =
            FastEvents.DefineEvent<Action<StatusEffectBase>>(typeof(StatusEffectBase),
                nameof(StatusEffectBase.OnDisabled));
        
        public static FastEvent<Action<StatusEffectBase, byte, byte>> OnIntensityChanged { get; } =
            FastEvents.DefineEvent<Action<StatusEffectBase, byte, byte>>(typeof(StatusEffectBase),
                nameof(StatusEffectBase.OnIntensityChanged));
        
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
            var isEnabling = prevIntensity == 0 && value > 0;

            if (isEnabling)
            {
                var receivingArgs =
                    new PlayerEffectUpdatingEventArgs(player.Hub, __instance, value, __instance.Duration);

                PlayerEvents.OnUpdatingEffect(receivingArgs);

                if (!receivingArgs.IsAllowed)
                    return false;

                value = receivingArgs.Intensity;
                
                __instance.Duration = receivingArgs.Duration;
            }

            __instance._intensity = (byte)Mathf.Min(value, __instance.MaxIntensity);
            __instance.Hub.playerEffectsController.ServerSyncEffect(__instance);
            
            PlayerEvents.OnUpdatedEffect(new PlayerEffectUpdatedEventArgs(player.Hub, __instance, value, __instance.Duration));

            if (isEnabling)
            {
                __instance.Enabled();

                OnEnabled.InvokeEvent(null, __instance);
            }
            else if (prevIntensity > 0 && value == 0)
            {
                __instance.Disabled();
                
                OnDisabled.InvokeEvent(null, __instance);
            }

            __instance.IntensityChanged(prevIntensity, value);
            
            OnIntensityChanged.InvokeEvent(null, __instance, prevIntensity, value);
            return false;
        }
    }
}