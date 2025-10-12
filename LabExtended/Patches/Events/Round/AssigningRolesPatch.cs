using GameCore;

using HarmonyLib;

using PlayerRoles.RoleAssign;
using PlayerRoles;

using LabExtended.API;

using LabExtended.Events;

using LabExtended.Utilities;
using LabExtended.Utilities.RoleSelection;

namespace LabExtended.Patches.Events.Round;

/// <summary>
/// Implements the <see cref="ExRoundEvents.AssigningRoles"/> and <see cref="ExRoundEvents.AssignedRoles"/> events.
/// </summary>
public static class AssigningRolesPatch
{
    /// <summary>
    /// Contains round-start roles.
    /// </summary>
    public static readonly Dictionary<ExPlayer, RoleTypeId> Roles = new();

    /// <summary>
    /// A list of valid players.
    /// </summary>
    public static readonly List<ExPlayer> Players = new();

    private static FastEvent<Action> OnPlayersSpawned { get; } =
        FastEvents.DefineEvent<Action>(typeof(RoleAssigner), nameof(RoleAssigner.OnPlayersSpawned));

    [HarmonyPatch(typeof(RoleAssigner), nameof(RoleAssigner.OnRoundStarted))]
    private static bool Prefix()
    {
        var options = RoleSelectorOptions.ModifyHumanHistory | RoleSelectorOptions.ModifyScpTickets;

        if (ConfigFile.ServerConfig.GetBool("allow_scp_overflow", false))
            options |= RoleSelectorOptions.AllowScpOverflow;
        
        Players.Clear();
        Players.AddRange(ExPlayer.Players.Where(p => p != null && !p.IsUnverified));
        
        Roles.Clear();
        
        var result = Roles.SelectRoles(Players, null, options,
            ConfigFile.ServerConfig.GetString("team_respawn_queue", RoleSelector.DefaultTeamQueue));

        if (!ExRoundEvents.OnAssigningRoles(new(Roles)))
            return false;

        RoleAssigner._spawned = true;
        RoleAssigner.LateJoinTimer.Restart();

        RoleAssigner.ScpsOverflowing = result.ScpsOverflowing;
        RoleAssigner.ScpOverflowMaxHsMultiplier = result.ScpOverflowHumeShieldMultiplier;

        HumanSpawner._humanQueue = result.HumanTeamQueue;
        HumanSpawner._queueLength = result.HumanTeamQueueLength;

        foreach (var pair in Roles)
        {
            if (pair.Value != RoleTypeId.None)
            {
                pair.Key.Role.RoundStartRole = pair.Value;
                pair.Key.Role.Set(pair.Value, RoleChangeReason.RoundStart, RoleSpawnFlags.All);

                if (pair.Key.Role.IsAlive)
                    RoleAssigner.AlreadySpawnedPlayers.Add(pair.Key.UserId);
            }
        }

        OnPlayersSpawned.InvokeEvent(null!, Array.Empty<object>());

        ExRoundEvents.OnAssignedRoles(new(Roles));

        Roles.Clear();
        Players.Clear();

        return false;
    }
}