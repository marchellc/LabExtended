using HarmonyLib;

using LabExtended.API;

using PlayerRoles.PlayableScps.Scp079.Cameras;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(CameraRotationAxis), nameof(CameraRotationAxis.Update))]
    public static class Scp079CameraRotPatch
    {
        public static bool Prefix(CameraRotationAxis __instance, Scp079Camera cam)
        {
            var camera = ExMap.GetCamera(cam);

            if (camera is null)
                return true;

            if (__instance._isVertical && camera._vertical.HasValue)
            {
                __instance.TargetValue = camera._vertical.Value;
                return false;
            }

            if (!__instance._isVertical && camera._horizontal.HasValue)
            {
                __instance.TargetValue = camera._horizontal.Value;
                return false;
            }

            return true;
        }
    }
}