using CustomPlayerEffects;
using HarmonyLib;

namespace LabExtended.Patches.Fixes {
    [HarmonyPatch(typeof(StatusEffectBase), nameof(StatusEffectBase.Intensity), MethodType.Setter)]
    public static class StrangledVitalityPatch {
        public static bool Prefix(StatusEffectBase __instance, byte value) {
            if (value <= __instance._intensity || __instance.AllowEnabling || __instance is Strangled) {
                __instance.ForceIntensity(value);
            }
            return false;
        }
    }
}
