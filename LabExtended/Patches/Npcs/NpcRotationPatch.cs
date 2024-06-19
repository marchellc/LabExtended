using HarmonyLib;

using LabExtended.API.Npcs;

using PlayerRoles.FirstPersonControl;

namespace LabExtended.Patches.Npcs
{
    [HarmonyPatch(typeof(FpcMouseLook), nameof(FpcMouseLook.UpdateRotation))]
    public static class NpcRotationPatch
    {
        public static bool Prefix(FpcMouseLook __instance)
        {
            var npc = NpcHandler.Get(__instance._hub);

            if (npc is null)
                return true;

            npc.OnRotationUpdated();
            return false;
        }
    }
}