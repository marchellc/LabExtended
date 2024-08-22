using HarmonyLib;

using PlayerRoles.RoleAssign;
using PlayerRoles;

using LabExtended.Core.Hooking;
using LabExtended.Events.Round;
using LabExtended.API;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(RoleAssigner), nameof(RoleAssigner.OnRoundStarted))]
    public static class AssigningRolesPatch
    {
        public static bool Prefix()
        {
            var roles = ExRound.ChooseRoles();

            if (!HookRunner.RunEvent(new AssigningRolesArgs(roles), true))
                return false;

            foreach (var pair in roles)
                pair.Key.Role.Set(pair.Value, RoleChangeReason.RoundStart, RoleSpawnFlags.All);

            return false;
        }
    }
}