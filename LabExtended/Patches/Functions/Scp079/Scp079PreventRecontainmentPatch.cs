using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Containers;

using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;

namespace LabExtended.Patches.Functions.Scp079;

/// <summary>
/// Implements the <see cref="SwitchContainer.PreventsRecontaining079"/> toggle.
/// </summary>
public static class Scp079PreventRecontainmentPatch
{
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.IsScpButNot079))]
    private static bool Prefix(PlayerRoleBase prb, ref bool __result)
    {
        if (prb.TryGetOwner(out var owner)
            && ExPlayer.TryGet(owner, out var player)
            && player.Role.IsScp
            && !player.Role.Is(RoleTypeId.Scp079)
            && !player.Toggles.PreventsRecontaining079)
            return __result = false;

        __result = prb.Team is Team.SCPs && prb.RoleTypeId is not RoleTypeId.Scp079;
        return false;
    }
}