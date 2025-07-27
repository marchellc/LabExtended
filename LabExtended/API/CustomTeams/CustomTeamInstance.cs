using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.API.CustomRoles;
using LabExtended.Extensions;

using PlayerRoles;

using UnityEngine;

namespace LabExtended.API.CustomTeams;

/// <summary>
/// A generic version of <see cref="CustomTeamInstance"/>.
/// </summary>
/// <typeparam name="THandler">The type of the parent team handler.</typeparam>
public class CustomTeamInstance<THandler> : CustomTeamInstance 
    where THandler : CustomTeamHandler
{
    /// <summary>
    /// Gets the parent team handler cast to <see cref="THandler"/>.
    /// </summary>
    public new THandler Handler => ((THandler)base.Handler);
}

/// <summary>
/// A spawned instance of a team.
/// </summary>
public abstract class CustomTeamInstance
{
    /// <summary>
    /// Gets the ID of the instance. IDs are unique per-team, not globally. IDs reset on server restart, not on round restart.
    /// </summary>
    public int Id { get; internal set; }

    /// <summary>
    /// Gets the time (in seconds) of the instance spawning - uses <see cref="Time.realtimeSinceStartup"/>
    /// </summary>
    public float SpawnTime { get; internal set; }

    /// <summary>
    /// Gets the amounts of seconds passed since the team spawned.
    /// </summary>
    public float PassedTime => Time.realtimeSinceStartup - SpawnTime;
    
    /// <summary>
    /// Whether or not this wave was spawned by the timer.
    /// </summary>
    public bool IsByTimer { get; internal set; }

    /// <summary>
    /// Gets the parent team handler.
    /// </summary>
    public CustomTeamHandler Handler { get; internal set; }

    /// <summary>
    /// Gets a list of players spawned in this instance - contains only players which are still alive.
    /// </summary>
    public List<ExPlayer> AlivePlayers { get; private set; } = new(10);

    /// <summary>
    /// Gets a list of ALL (except disconnected) players spawned in this instance (also includes player which were added later).
    /// </summary>
    public List<ExPlayer> OriginalPlayers { get; private set; } = new(10);

    /// <summary>
    /// Removes a member.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    /// <param name="setRole">The role to set to the player.</param>
    /// <returns>true if the member was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool RemoveMember(ExPlayer player, RoleTypeId? setRole = RoleTypeId.Spectator)
    {
        if (player?.ReferenceHub == null)
            throw new ArgumentNullException(nameof(player));

        if (!AlivePlayers.Contains(player) && !OriginalPlayers.Contains(player))
            return false;
        
        AlivePlayers.Remove(player);
        OriginalPlayers.Remove(player);
        
        OnRemoved(player);
        
        player.Role.CustomTeam = null;
        
        if (setRole.HasValue)
            player.Role.Set(setRole.Value);

        if (!string.IsNullOrEmpty(Handler.Name))
        {
            player.CustomInfo = string.Empty;
            player.InfoArea &= ~PlayerInfoArea.CustomInfo;
        }

        return true;
    }

    /// <summary>
    /// Adds a member to the team.
    /// </summary>
    /// <param name="player">The player to add to the team.</param>
    /// <param name="role">The role to set for the player.</param>
    /// <param name="keepPosition">Whether or not to keep the player's current position.</param>
    /// <param name="reviveIfDead">Whether or not to revive the player if they were part of the initial spawn and died.</param>
    /// <returns>true if the player was added</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public bool AddMember(ExPlayer player, object role, bool keepPosition = false, bool reviveIfDead = false)
    {
        if (role is null)
            throw new ArgumentNullException(nameof(role));

        if (player?.ReferenceHub == null)
            throw new ArgumentNullException(nameof(player));

        if (role is not RoleTypeId && role is not CustomRoleData)
            throw new ArgumentException("You can use a RoleTypeId enum or a CustomRoleData instance only.",
                nameof(role));

        if (player.Role.CustomTeam != null && player.Role.CustomTeam.Id != Id &&
            !player.Role.CustomTeam.RemoveMember(player, null))
            throw new Exception($"Player {player.UserId} is already a part of another team and could not be removed!");

        if (OriginalPlayers.Contains(player) && (AlivePlayers.Contains(player) || !reviveIfDead))
            return false;

        var position = Handler.SelectPosition(player);
        
        player.Role.CustomTeam = this;

        if (role is RoleTypeId roleType)
        {
            player.Role.Set(roleType, RoleChangeReason.Respawn,
                (!keepPosition && !position.HasValue) ? RoleSpawnFlags.All : RoleSpawnFlags.AssignInventory);
        }
        else if (role is CustomRoleData customRoleData)
        {
            player.SetCustomRole(customRoleData.Type, !keepPosition && !position.HasValue);
        }

        if (!keepPosition && position.HasValue)
            player.Position.Position = position.Value;

        if (!string.IsNullOrWhiteSpace(Handler.Name))
        {
            player.CustomInfo = Handler.Name;

            if ((player.InfoArea & PlayerInfoArea.CustomInfo) != PlayerInfoArea.CustomInfo)
                player.InfoArea |= PlayerInfoArea.CustomInfo;
        }

        AlivePlayers.AddUnique(player);
        OriginalPlayers.AddUnique(player);

        OnAdded(player);
        return true;
    }

    /// <summary>
    /// Checks if this team includes (or included) a specific player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="onlyAlive">Whether or not to only search through a list of alive players.</param>
    /// <returns></returns>
    public bool IsMember(ExPlayer player, bool onlyAlive = false)
    {
        if (player?.ReferenceHub == null)
            return false;
        
        return onlyAlive ? AlivePlayers.Contains(player) : OriginalPlayers.Contains(player);
    }
    
    /// <summary>
    /// Gets called once the instance is spawned.
    /// </summary>
    public virtual void OnSpawned() { }
    
    /// <summary>
    /// Gets called once the instance is destroyed.
    /// </summary>
    public virtual void OnDestroy() { }
    
    /// <summary>
    /// Gets called when a member is removed from the team.
    /// </summary>
    /// <param name="player">The player who was removed.</param>
    public virtual void OnRemoved(ExPlayer player) { }
    
    /// <summary>
    /// Gets called when a member is added to the team.
    /// </summary>
    /// <param name="player">The added player.</param>
    public virtual void OnAdded(ExPlayer player) { }
    
    /// <summary>
    /// Gets called when a member of this team leaves the server.
    /// </summary>
    /// <param name="player">The player who left the server.</param>
    public virtual void OnLeft(ExPlayer player) { }
    
    /// <summary>
    /// Gets called when a member of this team attacks another player.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnAttacking(PlayerHurtingEventArgs args) { }
    
    /// <summary>
    /// Gets called after a member of this team deals damage to another player.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnAttacked(PlayerHurtEventArgs args) { }
    
    /// <summary>
    /// Gets called when a member of this team kills another player (before being set to spectator).
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnKilling(PlayerDyingEventArgs args) { }
    
    /// <summary>
    /// Gets called after a member of this team kills another player (after being set to spectator).
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnKilled(PlayerDeathEventArgs args) { }
    
    /// <summary>
    /// Gets called when a member of this team receives damage.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnHurting(PlayerHurtingEventArgs args) { }
    
    /// <summary>
    /// Gets called after a member of this team receives damage.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnHurt(PlayerHurtEventArgs args) { }
    
    /// <summary>
    /// Gets called before a member of this team dies.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnDying(PlayerDyingEventArgs args) { }
    
    /// <summary>
    /// Gets called after a member of this team dies.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnDied(PlayerDeathEventArgs args) { }
    
    /// <summary>
    /// Gets called when a player's role is about to be changed.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnChangingRole(PlayerChangingRoleEventArgs args) { }
    
    /// <summary>
    /// Gets called when a player's role is changed.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnChangedRole(PlayerChangedRoleEventArgs args) { }
}