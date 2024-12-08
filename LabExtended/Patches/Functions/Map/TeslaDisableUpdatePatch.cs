using HarmonyLib;

namespace LabExtended.Patches.Functions.Map
{
    public static class TeslaDisableUpdatePatch
    {
        [HarmonyPatch(typeof(TeslaGateController), nameof(TeslaGateController.FixedUpdate))]
        public static bool Prefix() => false;
    }
}