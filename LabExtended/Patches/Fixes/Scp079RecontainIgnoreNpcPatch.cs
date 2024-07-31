using HarmonyLib;

using LabExtended.API;

using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;

namespace LabExtended.Patches.Fixes
{
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.OnServerRoleChanged))]
    public static class Scp079RecontainIgnoreNpcPatch
    {
        public static bool Prefix(Scp079Recontainer __instance, ReferenceHub hub, RoleTypeId newRole, RoleChangeReason reason)
        {
            if (newRole != RoleTypeId.Spectator || !__instance.IsScpButNot079(hub.roleManager.CurrentRole))
                return false;

            var count = 0;

            foreach (var role in Scp079Role.ActiveInstances)
            {
                if (!role.TryGetOwner(out var owner))
                    continue;

                if (!ExPlayer.TryGet(owner, out var ply))
                    continue;

                if (ply.IsNpc)
                    continue;

                count++;
            }

            if (count == 0)
                return false;

            if (ReferenceHub.AllHubs.Count(x => x != hub && __instance.IsScpButNot079(x.roleManager.CurrentRole)) > 0)
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