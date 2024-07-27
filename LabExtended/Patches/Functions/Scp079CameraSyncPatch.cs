using HarmonyLib;

using LabExtended.API;

using PlayerRoles.PlayableScps.Scp079.Cameras;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(Scp079Camera), nameof(Scp079Camera.IsUsedByLocalPlayer), MethodType.Getter)]
    public static class Scp079CameraSyncPatch
    {
        public static bool Prefix(Scp079Camera __instance, ref bool __result)
        {
            if (Camera._camSync != null && (Camera._camSync.CurrentCamera != null && Camera._camSync.CurrentCamera == __instance) || (Camera._camSync._switchTarget != null && Camera._camSync._switchTarget == __instance))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}