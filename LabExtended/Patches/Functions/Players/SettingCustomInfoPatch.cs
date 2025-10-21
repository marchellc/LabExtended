using HarmonyLib;

using LabExtended.API;

namespace LabExtended.Patches.Functions.Players
{
    /// <summary>
    /// Provides functionality of setting custom player info strings.
    /// </summary>
    public static class SettingCustomInfoPatch
    {
        [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.Network_customPlayerInfoString), MethodType.Setter)]
        private static bool Prefix(NicknameSync __instance, ref string value)
        {
            if (!ExPlayer.TryGet(__instance._hub, out var player)
                || player.infoPropertyBuilder == null)
                return true;

            player.infoPropertyBuilder.Clear();
            player.infoPropertyBuilder.Append(value);

            return false;
        }
    }
}