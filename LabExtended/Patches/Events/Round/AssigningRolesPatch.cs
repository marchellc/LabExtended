using HarmonyLib;

using PlayerRoles.RoleAssign;
using PlayerRoles;

using LabExtended.Events;
using LabExtended.API;

namespace LabExtended.Patches.Events.Round
{
    public static class AssigningRolesPatch
    {
        [HarmonyPatch(typeof(RoleAssigner), nameof(RoleAssigner.OnRoundStarted))]
        public static bool Prefix()
        {
            var roles = ExRound.ChooseRoles();

            if (!ExRoundEvents.OnAssigningRoles(new(roles)))
                return false;

            foreach (var pair in roles)
                pair.Key.Role.Set(pair.Value, RoleChangeReason.RoundStart, RoleSpawnFlags.All);

            return false;
        }
    }
}