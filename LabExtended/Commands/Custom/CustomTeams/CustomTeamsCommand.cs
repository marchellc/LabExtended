using LabExtended.API.CustomTeams;
using LabExtended.API.CustomTeams.Internal;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

using UnityEngine;

namespace LabExtended.Commands.Custom.CustomTeams;

[Command("customteam", "Manages the CustomTeams API", "ct")]
public class CustomTeamsCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("list", "Lists all custom teams.")]
    public void List()
    {
        if (CustomTeamRegistry.RegisteredHandlers.Count == 0)
        {
            Fail("No CustomTeams registered.");
            return;
        }
        
        Ok(x =>
        {
            foreach (var pair in CustomTeamRegistry.RegisteredHandlers)
            {
                x.AppendLine($"- {pair.Key}: {pair.Value.Name ?? "(null)"}");
            }
        });
    }

    [CommandOverload("waves", "Lists all spawned waves of a team.")]
    public void Waves(
        [CommandParameter("Name", "Name of the team.")] string name)
    {
        if (!CustomTeamRegistry.TryGet(name, out CustomTeamHandlerBase handler))
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
            foreach (var instance in instances)
            {
                x.AppendLine($"- {instance.Id}: Remaining players: {instance.AlivePlayers.Count} (spawned {Mathf.CeilToInt(instance.PassedTime)} seconds ago)");
            }
        });
    }

    [CommandOverload("wave", "Shows information about an active wave.")]
    public void Wave(
        [CommandParameter("Name", "Name of the team.")] string name,
        [CommandParameter("ID", "ID of the wave.")] int id)
    {
        if (!CustomTeamRegistry.TryGet(name, out CustomTeamHandlerBase handler))
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

    [CommandOverload("spawn", "Spawns a new wave of a custom team.")]
    public void Spawn(
        [CommandParameter("Name", "Name of the team to spawn.")] string name,
        [CommandParameter("Players", "The maximum amount of players to spawn.")] int players)
    {
        if (!CustomTeamRegistry.TryGet(name, out CustomTeamHandlerBase handler))
        {
            Fail($"Could not find custom team '{name}'");
            return;
        }

        var instance = handler.Internal_SpawnInstance(players);

        if (instance is null)
        {
            Fail($"Could not spawn a new wave.");
            return;
        }
        
        Ok($"Spawned a new wave of '{handler.GetType().Name}' with '{instance.AlivePlayers.Count}' player(s) (ID: {instance.Id})");
    }

    [CommandOverload("despawn", "Despawns an active wave of a custom team.")]
    public void Despawn(
        [CommandParameter("Name", "Name of the team to despawn.")] string name,
        [CommandParameter("ID", "ID of the instance to despawn (or * for all).")] string id)
    {
        if (!CustomTeamRegistry.TryGet(name, out CustomTeamHandlerBase handler))
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