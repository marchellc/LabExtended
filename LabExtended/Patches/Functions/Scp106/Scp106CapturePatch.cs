using HarmonyLib;

using LabExtended.API;

using PlayerRoles.PlayableScps.Scp106;

namespace LabExtended.Patches.Functions.Scp106
{
    public static class Scp106CapturePatch
    {
        [HarmonyPatch(typeof(Scp106Attack), nameof(Scp106Attack.ServerShoot))]
        public static bool Prefix(Scp106Attack __instance)
        {
            if (!ExPlayer.TryGet(__instance._targetHub, out var player) || !ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            if (!player.Toggles.CanBeCapturedBy106 || !scp.Toggles.CanCaptureAs106)
            {
                __instance.SendCooldown(__instance._missCooldown);
                return false;
            }

            return true;
        }
    }
}