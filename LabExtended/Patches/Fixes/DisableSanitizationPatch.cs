using HarmonyLib;

namespace LabExtended.Patches.Fixes
{
    [HarmonyPatch(typeof(Misc), nameof(Misc.SanitizeRichText))]
    public static class DisableSanitizationPatch
    {
        public static bool Prefix(string content, ref string __result)
        {
            __result = content;
            return false;
        }
    }
}