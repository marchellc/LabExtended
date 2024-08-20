using HarmonyLib;

using LabExtended.API;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;

namespace LabExtended.Patches.Fixes
{
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.OnServerRoleChanged))]
    public static class Scp079RecontainIgnoreNpcPatch
    {
        public static bool Prefix(Scp079Recontainer __instance, ReferenceHub hub, RoleTypeId newRole)
        {
            if (newRole != RoleTypeId.Spectator || !__instance.IsScpButNot079(hub.roleManager.CurrentRole))
                return false;

            if (!ExPlayer.TryGet(hub, out var player) || !player.Switches.PreventsRecontaining079)
                return false;

            if (!ExPlayer.AllPlayers.Any(x => x.Role.Type == RoleTypeId.Scp079 && x.Switches.CanBeRecontainedAs079))
                return false;

            if (ExPlayer.AllPlayers.Any(x => x.Hub != hub && __instance.IsScpButNot079(x.Role.Role) && x.Switches.PreventsRecontaining079))
                return false;

            __instance.SetContainmentDoors(true, true);

            __instance._alreadyRecontained = true;
            __instance._recontainLater = 3f;

            foreach (var gen in Scp079Recontainer.AllGenerators)
                gen.Engaged = true;

            return false;
        }
    }
}