using GameCore;

using HarmonyLib;

using PlayerRoles.RoleAssign;
using PlayerRoles;

using LabExtended.Core.Pooling.Pools;

using LabExtended.API;
using LabExtended.Events;
using LabExtended.Utilities;

namespace LabExtended.Patches.Events.Round;

/// <summary>
/// Implements the <see cref="ExRoundEvents.AssigningRoles"/> event.
/// </summary>
public static class AssigningRolesPatch
{
    /// <summary>
    /// Contains round-start roles.
    /// </summary>
    public static readonly Dictionary<ExPlayer, RoleTypeId> Roles = new();

    public static FastEvent<Action> OnPlayersSpawned { get; } =
        FastEvents.DefineEvent<Action>(typeof(RoleAssigner), nameof(RoleAssigner.OnPlayersSpawned));

    [HarmonyPatch(typeof(RoleAssigner), nameof(RoleAssigner.OnRoundStarted))]
    private static bool Prefix()
    {
        Roles.Clear();
        
        RoleSelector.GetRolesNonAlloc(Roles, out var hsOverflowMultiplier,
            ConfigFile.ServerConfig.GetBool("allow_scp_overflow"),
            true, true, ConfigFile.ServerConfig.GetString("team_respawn_queue", "4014314031441404134041434414"));

        if (!ExRoundEvents.OnAssigningRoles(new(Roles)))
            return false;

        RoleAssigner._spawned = true;
        RoleAssigner.LateJoinTimer.Restart();
        
        if (hsOverflowMultiplier.HasValue)
        {
            RoleAssigner.ScpsOverflowing = true;
            RoleAssigner.ScpOverflowMaxHsMultiplier = hsOverflowMultiplier.Value;
        }
        else
        {
            RoleAssigner.ScpsOverflowing = false;
        }

        foreach (var pair in Roles)
        {
            pair.Key.Role.RoundStartRole = pair.Value;
            pair.Key.Role.Set(pair.Value, RoleChangeReason.RoundStart, RoleSpawnFlags.All);

            if (pair.Key.Role.IsAlive)
                RoleAssigner.AlreadySpawnedPlayers.Add(pair.Key.UserId);
        }

        OnPlayersSpawned.InvokeEvent(null, Array.Empty<object>());
        return false;
    }
}