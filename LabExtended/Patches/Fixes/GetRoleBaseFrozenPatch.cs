using HarmonyLib;

using LabExtended.Extensions;

using PlayerRoles;

namespace LabExtended.Patches.Fixes
{
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.GetRoleBase))]
    public static class GetRoleBaseFrozenPatch
    {
        public static bool Prefix(RoleTypeId targetId, ref PlayerRoleBase __result)
        {
            __result = targetId.GetInstance();
            return false;
        }
    }
}