using LabExtended.Core.Pooling.Pools;

using NorthwoodLib.Pools;

using PlayerRoles;

using UnityEngine;

namespace LabExtended.API.CustomTeams;

/// <summary>
/// Handles custom teams.
/// </summary>
public abstract class CustomTeamHandler<TInstance> : CustomTeamHandler
    where TInstance : CustomTeamInstance
{
    private int idClock = 0;
    
    /// <summary>
    /// Gets the team's respawn timer.
    /// </summary>
    public virtual CustomTeamTimer<TInstance>? WaveTimer { get; }

    /// <summary>
    /// Gets the type of the team instance class.
    /// </summary>
    public Type Type { get; } = typeof(TInstance);

    /// <summary>
    /// Gets a list of all spawned instances.
    /// </summary>
    public Dictionary<int, TInstance> Instances { get; } = new();
    
    /// <summary>
    /// Gets called once a new team instance is spawned.
    /// </summary>
    /// <param name="instance">The spawned instance.</param>
    public virtual void OnSpawned(TInstance instance) { }

    /// <summary>
    /// Gets called when an active team instance is despawned, aka when all of it's team members die, disconnect or someone despawns all the players.
    /// </summary>
    /// <param name="instance">The instance which was despawned.</param>
    public virtual void OnDespawned(TInstance instance) { }

    /// <inheritdoc cref="CustomTeamHandler.OnRegistered"/>
    public override void OnRegistered()
    {
        base.OnRegistered();

        if (WaveTimer != null)
        {
            WaveTimer.Handler = this;
            WaveTimer.Start();
        }
    }

    /// <inheritdoc cref="CustomTeamHandler.OnUnregistered"/>
    public override void OnUnregistered()
    {
        base.OnUnregistered();
        
        WaveTimer?.Dispose();
    }

    /// <summary>
    /// Despawns all active instances.
    /// </summary>
    /// <param name="playerRole">The role to set alive players to.</param>
    public override void DespawnAll(RoleTypeId playerRole = RoleTypeId.Spectator)
    {
        while (Instances.Count > 0)
        {
            Despawn(Instances.First().Value, playerRole);
        }
    }

    /// <summary>
    /// Despawns an active team instance and all of it's alive members.
    /// </summary>
    /// <param name="instance">The instance to despawn.</param>
    /// <param name="playerRole">The role to set alive players to.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Despawn(TInstance instance, RoleTypeId playerRole = RoleTypeId.Spectator)
    {
        if (instance is null)
            throw new ArgumentNullException(nameof(instance));

        for (var i = 0; i < instance.AlivePlayers.Count; i++)
        {
            var player = instance.AlivePlayers[i];
            
            if (player?.ReferenceHub == null)
                continue;
            
            if (!string.IsNullOrWhiteSpace(Name))
            {
                player.CustomInfo = string.Empty;
                player.InfoArea &= ~PlayerInfoArea.CustomInfo;
            }
            
            player.Role.CustomTeam = null;
            player.Role.Set(playerRole);
        }

        for (var i = 0; i < instance.OriginalPlayers.Count; i++)
        {
            var player = instance.OriginalPlayers[i];

            if (player?.ReferenceHub != null)
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    player.CustomInfo = string.Empty;
                    player.InfoArea &= ~PlayerInfoArea.CustomInfo;
                }
            }
        }

        instance.AlivePlayers.Clear();
        instance.OriginalPlayers.Clear();

        Instances.Remove(instance.Id);
        
        OnDespawned(instance);
        
        instance.OnDestroy();
    }

    /// <summary>
    /// Spawns a new team instance.
    /// </summary>
    /// <param name="minPlayerCount">The minimum amount of players required to spawn.</param>
    /// <param name="maxPlayerCount">The maximum amount of players allowed to spawn.</param>
    /// <param name="additionalChecks">Delegate used to check potential players, called after <see cref="CustomTeamHandler.IsSpawnable"/>.</param>
    /// <returns>The spawned team instance (if spawned, null if there weren't enough players).</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="Exception"></exception>
    public CustomTeamSpawnResult<TInstance> Spawn(int minPlayerCount, int maxPlayerCount, Predicate<ExPlayer>? additionalChecks = null)
    {
        var spawnablePlayers = ListPool<ExPlayer>.Shared.Rent();

        for (var i = 0; i < ExPlayer.Count; i++)
        {
            if (maxPlayerCount > 0 && spawnablePlayers.Count >= maxPlayerCount)
                break;
            
            var player = ExPlayer.Players[i];

            if (player?.ReferenceHub == null || player.IsUnverified)
                continue;

            if (!IsSpawnable(player))
                continue;

            if (additionalChecks != null && !additionalChecks(player))
                continue;

            spawnablePlayers.Add(player);
        }

        if ((minPlayerCount > 0 && spawnablePlayers.Count < minPlayerCount)
            || spawnablePlayers.Count < 1)
        {
            ListPool<ExPlayer>.Shared.Return(spawnablePlayers);
            return new(null, CustomTeamSpawnFail.NotEnoughPlayers);
        }

        var instance = Spawn(spawnablePlayers);

        ListPool<ExPlayer>.Shared.Return(spawnablePlayers);
        return instance;
    }

    /// <summary>
    /// Spawns a new team instance.
    /// </summary>
    /// <param name="players">The list of players to spawn.</param>
    /// <returns>The spawned team instance (if spawned, null if there weren't enough players).</returns>
    public CustomTeamSpawnResult<TInstance> Spawn(IEnumerable<ExPlayer> players)
    {
        if (players is null)
            throw new ArgumentNullException(nameof(players));

        if (players.Count() < 1)
            return new(null, CustomTeamSpawnFail.NotEnoughPlayers);

        if (Activator.CreateInstance(Type) is not TInstance teamInstance)
            throw new Exception($"Could not instantiate {typeof(TInstance).Name}");
        
        teamInstance.Id = idClock++;
        teamInstance.Handler = this;
        teamInstance.SpawnTime = Time.realtimeSinceStartup;
        
        var selectedRoles = DictionaryPool<ExPlayer, RoleTypeId>.Shared.Rent();

        foreach (var player in players)
        {
            var role = SelectRole(player, selectedRoles);

            selectedRoles[player] = role;
            
            teamInstance.SpawnPlayer(player, role);

            teamInstance.AlivePlayers.Add(player);
            teamInstance.OriginalPlayers.Add(player);

            player.Role.CustomTeam = teamInstance;
        }
        
        DictionaryPool<ExPlayer, RoleTypeId>.Shared.Return(selectedRoles);

        Instances[teamInstance.Id] = teamInstance;
        
        OnSpawned(teamInstance);
        
        teamInstance.OnSpawned();
        return new(teamInstance, null);
    }
    
    internal override void Internal_RemoveInstance(int id)
    {
        if (Instances.TryGetValue(id, out var instance))
        {
            Instances.Remove(id);
            
            OnDespawned(instance);
        }
    }

    internal override bool Internal_DespawnInstance(int id)
    {
        if (Instances.TryGetValue(id, out var instance))
        {
            Despawn(instance);
            return true;
        }

        return false;
    }

    internal override IEnumerable<CustomTeamInstance> Internal_GetInstances()
    {
        return Instances.Values;
    }

    internal override CustomTeamInstance Internal_SpawnInstance(int minPlayerCount, int maxPlayerCount)
    {
        return Spawn(minPlayerCount, maxPlayerCount).SpawnedWave!;
    }
}