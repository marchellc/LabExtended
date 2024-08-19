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
            if (!ExPlayer.TryGet(hub, out var player) || player.IsNpc)
                return false;

            if (newRole != RoleTypeId.Spectator || !__instance.IsScpButNot079(hub.roleManager.CurrentRole))
                return false;

            if (!Scp079Role.ActiveInstances.Any(x => x.TryGetOwner(out var owner) && ExPlayer.TryGet(owner, out var player) && !player.IsNpc && player.Switches.CanBeRecontainedAs079))
                return false;

            if (ReferenceHub.AllHubs.Any(x => x != hub && __instance.IsScpButNot079(x.roleManager.CurrentRole) && ExPlayer.TryGet(x, out var player) && !player.IsNpc))
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