using LabExtended.API.CustomTeams;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

using UnityEngine;

namespace LabExtended.Commands.Custom.CustomTeams;

/// <summary>
/// Provides server-side commands for managing custom teams and their active waves using the CustomTeams API.
/// </summary>
/// <remarks>This command allows administrators to list registered custom teams, view and manage active team
/// waves, and spawn or despawn team instances. All operations are performed via server-side commands and require
/// appropriate permissions. Use the available subcommands to interact with the CustomTeams system as needed.</remarks>
[Command("customteam", "Manages the CustomTeams API", "ct")]
public class CustomTeamsCommand : CommandBase, IServerSideCommand
{
    /// <summary>
    /// Displays a list of all registered custom teams to the user.
    /// </summary>
    /// <remarks>If no custom teams are registered, an error message is displayed instead of a list.</remarks>
    [CommandOverload("list", "Lists all custom teams.", null)]
    public void List()
    {
        if (CustomTeamRegistry.RegisteredHandlers.Count == 0)
        {
            Fail("No CustomTeams registered.");
            return;
        }
        
        Ok(x =>
        {
            x.AppendLine();
            
            foreach (var pair in CustomTeamRegistry.RegisteredHandlers)
            {
                x.AppendLine($"- {pair.Key.Name}: {pair.Value.Name ?? "(null)"}");
            }
        });
    }

    /// <summary>
    /// Lists all currently active waves for the specified team.
    /// </summary>
    /// <param name="name">The name of the team whose active waves are to be listed. Cannot be null or empty.</param>
    [CommandOverload("waves", "Lists all spawned waves of a team.", null)]
    public void Waves(
        [CommandParameter("Name", "Name of the team.")] string name)
    {
        if (!CustomTeamRegistry.TryGet(name, out CustomTeamHandler handler))
        {
            Fail($"Could not find custom team '{name}'");
            return;
        }

        var instances = handler.Internal_GetInstances();

        if (instances.Count() == 0)
        {
            Fail($"Team '{handler.GetType().Name}' has no active waves.");
            return;
        }
        
        Ok(x =>
        {
            x.AppendLine();
            
            foreach (var instance in instances)
            {
                x.AppendLine($"- {instance.Id}: Remaining players: {instance.AlivePlayers.Count} (spawned {Mathf.CeilToInt(instance.PassedTime)} seconds ago)");
            }
        });
    }

    /// <summary>
    /// Displays information about an active wave for the specified team.
    /// </summary>
    /// <param name="name">The name of the team for which to display wave information. Cannot be null or empty.</param>
    /// <param name="id">The identifier of the wave to display information for.</param>
    [CommandOverload("wave", "Shows information about an active wave.", null)]
    public void Wave(
        [CommandParameter("Name", "Name of the team.")] string name,
        [CommandParameter("ID", "ID of the wave.")] int id)
    {
        if (!CustomTeamRegistry.TryGet(name, out CustomTeamHandler handler))
        {
            Fail($"Could not find custom team '{name}'");
            return;
        }

        var instances = handler.Internal_GetInstances();

        if (instances.Count() == 0)
        {
            Fail($"Team '{handler.GetType().Name}' has no active waves.");
            return;
        }

        if (!instances.TryGetFirst(x => x.Id == id, out var instance))
        {
            Fail($"Could not find active wave '{id}' in team '{handler.GetType().Name}'");
            return;
        }
        
        Ok(x =>
        {
            x.AppendLine();
            x.AppendLine($"ID: {instance.Id}");
            x.AppendLine($"Spawned At: {DateTime.Now.ToLocalTime().Subtract(TimeSpan.FromSeconds(instance.PassedTime)).ToString("HH:mm:ss zz")}");
            x.AppendLine($"Active For: {Mathf.CeilToInt(instance.PassedTime)} seconds");
            x.AppendLine($"Is By Timer: {instance.IsByTimer}");
            
            x.AppendLine($"Alive Players ({instance.AlivePlayers.Count}):");

            foreach (var ply in instance.AlivePlayers)
            {
                if (ply?.ReferenceHub != null)
                {
                    x.AppendLine($" - [{ply.Role.Name}] |{ply.PlayerId}| {ply.Nickname} ({ply.UserId})");
                }
            }

            x.AppendLine($"Original Players ({instance.OriginalPlayers.Count}):");
            
            foreach (var ply in instance.OriginalPlayers)
            {
                if (ply?.ReferenceHub != null)
                {
                    x.AppendLine($" - [{ply.Role.Name}] |{ply.PlayerId}| {ply.Nickname} ({ply.UserId})");
                }
            }
        });
    }

    /// <summary>
    /// Spawns a new wave of the specified custom team by name.
    /// </summary>
    /// <remarks>If the specified team name does not exist in the custom team registry, the operation fails
    /// and no team is spawned. Only one wave is spawned per invocation.</remarks>
    /// <param name="name">The name of the custom team to spawn. This value is case-sensitive and must correspond to a registered custom
    /// team.</param>
    [CommandOverload("spawn", "Spawns a new wave of a custom team.", null)]
    public void Spawn(
        [CommandParameter("Name", "Name of the team to spawn.")] string name)
    {
        if (!CustomTeamRegistry.TryGet(name, out CustomTeamHandler handler))
        {
            Fail($"Could not find custom team '{name}'");
            return;
        }

        var instance = handler.Internal_SpawnInstance(-1, -1);

        if (instance is null)
        {
            Fail($"Could not spawn a new wave.");
            return;
        }
        
        Ok($"Spawned a new wave of '{handler.GetType().Name}' with '{instance.AlivePlayers.Count}' player(s) (ID: {instance.Id})");
    }

    /// <summary>
    /// Despawns an active wave of a custom team by name and instance ID.
    /// </summary>
    /// <remarks>If the specified team name does not exist, the operation fails. When "*" is provided for the
    /// ID, all active instances of the team are despawned.</remarks>
    /// <param name="name">The name of the custom team to despawn. This value is case-sensitive and must correspond to a registered team.</param>
    /// <param name="id">The ID of the team instance to despawn. Specify an integer value to target a specific instance, or use "*" to
    /// despawn all active instances of the specified team.</param>
    [CommandOverload("despawn", "Despawns an active wave of a custom team.", null)]
    public void Despawn(
        [CommandParameter("Name", "Name of the team to despawn.")] string name,
        [CommandParameter("ID", "ID of the instance to despawn (or * for all).")] string id)
    {
        if (!CustomTeamRegistry.TryGet(name, out CustomTeamHandler handler))
        {
            Fail($"Could not find custom team '{name}'");
            return;
        }

        if (int.TryParse(id, out var instanceId))
        {
            if (!handler.Internal_DespawnInstance(instanceId))
            {
                Fail($"Could not despawn team instance '{instanceId}' of team '{handler.GetType().Name}'");
            }
            else
            {
                Ok($"Despawned team instance '{instanceId}' of team '{handler.GetType().Name}'");
            }
        }
        else
        {
            handler.DespawnAll();
            
            Ok($"Despawned all active team instances of '{handler.GetType().Name}'");
        }
    }
}