using HarmonyLib;

using PlayerRoles.RoleAssign;
using PlayerRoles;

using LabExtended.Core.Hooking;
using LabExtended.Events.Round;
using LabExtended.API;
using LabExtended.Attributes;

namespace LabExtended.Patches.Events
{
    public static class AssigningRolesPatch
    {
        [HookPatch(typeof(AssigningRolesArgs))]
        [HarmonyPatch(typeof(RoleAssigner), nameof(RoleAssigner.OnRoundStarted))]
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