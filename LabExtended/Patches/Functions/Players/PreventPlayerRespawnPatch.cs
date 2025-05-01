using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Containers;

using PlayerRoles.Spectating;

using Respawning.Waves;

namespace LabExtended.Patches.Functions.Players;

/// <summary>
/// Implements the <see cref="SwitchContainer.CanBeRespawned"/> toggle.
/// </summary>
public static class PreventPlayerRespawnPatch
{
    [HarmonyPatch(typeof(WaveSpawner), nameof(WaveSpawner.CanBeSpawned))]
    private static bool Prefix(ReferenceHub player, ref bool __result)
    {
        if (player?.roleManager?.CurrentRole is SpectatorRole spectatorRole)
        {
            if (!spectatorRole.ReadyToRespawn)
                return __result = false;

            if (!ExPlayer.TryGet(player, out var ply))
            {
                __result = true;
                return false;
            }

            __result = ply.Toggles.CanBeRespawned;
            return false;
        }

        return __result = false;
    }
}