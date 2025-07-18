using LabExtended.API.CustomRoles;
using LabExtended.API.CustomTeams.Internal;

using LabExtended.Core.Pooling.Pools;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

using PlayerRoles;

using UnityEngine;

namespace LabExtended.API.CustomTeams;

/// <summary>
/// Handles custom teams.
/// </summary>
public abstract class CustomTeamHandler<TInstance> : CustomTeamHandlerBase
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

    /// <inheritdoc cref="CustomTeamHandlerBase.OnRegistered"/>
    public override void OnRegistered()
    {
        base.OnRegistered();

        if (WaveTimer != null)
        {
            WaveTimer.Handler = this;
            WaveTimer.Start();
        }
    }

    /// <inheritdoc cref="CustomTeamHandlerBase.OnUnregistered"/>
    public override void OnUnregistered()
    {
        base.OnUnregistered();
        
        WaveTimer?.Dispose();
    }

    internal override void Internal_RemoveInstance(int id)
    {
        if (Instances.TryGetValue(id, out var instance))
        {
            Instances.Remove(id);
            
            OnDespawned(instance);
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
            
            if (!string.IsNullOrWhiteSpace(Name))
            {
                player.CustomInfo = string.Empty;
                player.InfoArea &= ~PlayerInfoArea.CustomInfo;
            }
            
            player.Role.CustomTeam = null;
            player.Role.Set(playerRole);
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
    /// <param name="maxPlayerCount">The maximum amount of players to spawn.</param>
    /// <param name="throwIfNotEnoughPlayers">Whether or not to throw an exception if there aren't enough players to match <paramref name="maxPlayerCount"/></param>
    /// <param name="additionalChecks">Delegate used to check potential players, called after <see cref="IsSpawnable"/>.</param>
    /// <returns>The spawned team instance (if spawned, null if there weren't enough players).</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="Exception"></exception>
    public TInstance? Spawn(int maxPlayerCount, bool throwIfNotEnoughPlayers = false, Predicate<ExPlayer>? additionalChecks = null)
    {
        if (maxPlayerCount < 1)
            throw new ArgumentOutOfRangeException(nameof(maxPlayerCount));

        var spawnablePlayers = ListPool<ExPlayer>.Shared.Rent();

        for (var i = 0; i < ExPlayer.Count; i++)
        {
            var player = ExPlayer.Players[i];
            
            if (player?.ReferenceHub == null || player.IsUnverified)
                continue;
            
            if (!IsSpawnable(player))
                continue;
            
            if (additionalChecks != null && !additionalChecks(player))
                continue;
            
            spawnablePlayers.Add(player);
        }

        if (spawnablePlayers.Count < maxPlayerCount && throwIfNotEnoughPlayers)
            throw new Exception($"Could not find {maxPlayerCount} player(s) to spawn.");

        if (spawnablePlayers.Count == 0)
        {
            ListPool<ExPlayer>.Shared.Return(spawnablePlayers);
            return null;
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
    public TInstance? Spawn(IList<ExPlayer> players)
    {
        if (players is null)
            throw new ArgumentNullException(nameof(players));

        if (Activator.CreateInstance(Type) is not TInstance teamInstance)
            throw new Exception($"Could not instantiate {typeof(TInstance).Name}");

        var selectedRoles = DictionaryPool<ExPlayer, object>.Shared.Rent();

        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var role = SelectRole(player, selectedRoles);
            
            if (role is null)
                continue;

            if (role is RoleTypeId || role is CustomRoleData)
                selectedRoles[player] = role;
            else
                throw new Exception($"Unknown return type from SelectRole(): {role.GetType().FullName}");
        }

        teamInstance.Id = idClock++;
        teamInstance.Handler = this;
        teamInstance.SpawnTime = Time.realtimeSinceStartup;

        foreach (var pair in selectedRoles)
        {
            var spawnPosition = SelectPosition(pair.Key);
            
            if (pair.Value is RoleTypeId roleType)
            {
                pair.Key.Role.Set(roleType, RoleChangeReason.Respawn, spawnPosition.HasValue 
                                                        ? RoleSpawnFlags.AssignInventory
                                                        : RoleSpawnFlags.All);
            }
            else if (pair.Value is CustomRoleData customRoleData)
            {
                pair.Key.SetCustomRole(customRoleData.Type, !spawnPosition.HasValue);
            }

            if (spawnPosition.HasValue)
                pair.Key.Position.Position = spawnPosition.Value;

            if (!string.IsNullOrWhiteSpace(Name))
            {
                pair.Key.CustomInfo = Name;

                if ((pair.Key.InfoArea & PlayerInfoArea.CustomInfo) != PlayerInfoArea.CustomInfo)
                    pair.Key.InfoArea |= PlayerInfoArea.CustomInfo;
            }

            teamInstance.AlivePlayers.Add(pair.Key);
            teamInstance.OriginalPlayers.Add(pair.Key);

            pair.Key.Role.CustomTeam = teamInstance;
        }
        
        DictionaryPool<ExPlayer, object>.Shared.Return(selectedRoles);

        Instances[teamInstance.Id] = teamInstance;
        
        OnSpawned(teamInstance);
        
        teamInstance.OnSpawned();
        return teamInstance;
    }
}