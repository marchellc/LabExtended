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
public abstract class CustomTeamHandler<TInstance> : Internal_CustomTeamHandlerBase
    where TInstance : CustomTeamInstance
{
    private int idClock = 0;

    /// <summary>
    /// Gets the type of the team instance class.
    /// </summary>
    public Type Type { get; } = typeof(TInstance);

    /// <summary>
    /// Gets a list of all spawned instances.
    /// </summary>
    public Dictionary<int, TInstance> Instances { get; } = new();

    /// <summary>
    /// Checks if a specific player is spawnable to be included in the next team.
    /// </summary>
    /// <param name="player">The player to spawn.</param>
    /// <returns>true if the player can be spawned</returns>
    public abstract bool IsSpawnable(ExPlayer player);

    /// <summary>
    /// Selects a player's role.
    /// </summary>
    /// <param name="player">The player to select the role for.</param>
    /// <param name="selectedRoles">A list of already selected roles (values are the respective roles, can be of type <see cref="CustomRoleData"/> or <see cref="RoleTypeId"/>).</param>
    /// <returns>The selected role to spawn as, can return <see cref="CustomRoleData"/> to spawn as a custom role or <see cref="RoleTypeId"/> to spawn as a base-game role (or null to exclude the player from the spawn).</returns>
    public abstract object SelectRole(ExPlayer player, Dictionary<ExPlayer, object> selectedRoles);
    
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
    /// <param name="customSpawnBounds">The spawn position bounds.</param>
    /// <param name="throwIfNotEnoughPlayers">Whether or not to throw an exception if there aren't enough players to match <paramref name="maxPlayerCount"/></param>
    /// <param name="additionalChecks">Delegate used to check potential players, called after <see cref="IsSpawnable"/>.</param>
    /// <returns>The spawned team instance (if spawned, null if there weren't enough players).</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="Exception"></exception>
    public TInstance? Spawn(int maxPlayerCount, Bounds? customSpawnBounds, bool throwIfNotEnoughPlayers = false, Predicate<ExPlayer>? additionalChecks = null)
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

        if (Activator.CreateInstance(Type) is not TInstance teamInstance)
        {
            ListPool<ExPlayer>.Shared.Return(spawnablePlayers);

            throw new Exception($"Could not instantiate {typeof(TInstance).Name}");
        }

        var selectedRoles = DictionaryPool<ExPlayer, object>.Shared.Rent();

        for (var i = 0; i < spawnablePlayers.Count; i++)
        {
            var player = spawnablePlayers[i];
            var role = SelectRole(player, selectedRoles);
            
            if (role is null)
                continue;

            if (role is RoleTypeId || role is CustomRoleData)
                selectedRoles[player] = role;
            else
                throw new Exception($"Unknown return type from SelectRole(): {role.GetType().FullName}");
        }
        
        ListPool<ExPlayer>.Shared.Return(spawnablePlayers);

        teamInstance.Id = idClock++;
        teamInstance.Handler = this;
        teamInstance.SpawnBounds = customSpawnBounds;
        teamInstance.SpawnTime = Time.realtimeSinceStartup;

        foreach (var pair in selectedRoles)
        {
            if (pair.Value is RoleTypeId roleType)
            {
                pair.Key.Role.Set(roleType, RoleChangeReason.Respawn,
                    customSpawnBounds.HasValue ? RoleSpawnFlags.AssignInventory : RoleSpawnFlags.All);
            }
            else if (pair.Value is CustomRoleData customRoleData)
            {
                pair.Key.SetCustomRole(customRoleData.Type, !customSpawnBounds.HasValue);
            }

            if (customSpawnBounds.HasValue)
                pair.Key.Position.Position = customSpawnBounds.Value.GetRandom(false);

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