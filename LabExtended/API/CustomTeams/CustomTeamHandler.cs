using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.Attributes;
using LabExtended.Events;

using LabExtended.API.CustomRoles;

using UnityEngine;

using PlayerRoles;

namespace LabExtended.API.CustomTeams;

/// <summary>
/// Base class for <see cref="CustomTeamHandler{TInstance}"/>, exists to provide internal methods.
/// </summary>
public abstract class CustomTeamHandler
{
    /// <summary>
    /// Gets the team's name.
    /// </summary>
    public abstract string? Name { get; }

    /// <summary>
    /// Selects a spawn position for a player.
    /// </summary>
    /// <param name="player">The player to select the position for.</param>
    /// <returns>the selected position (or null to use a random position from the player's role)</returns>
    public abstract Vector3? SelectPosition(ExPlayer player);
    
    /// <summary>
    /// Selects a player's role.
    /// </summary>
    /// <param name="player">The player to select the role for.</param>
    /// <param name="selectedRoles">A list of already selected roles (values are the respective roles, can be of type <see cref="CustomRoleData"/> or <see cref="RoleTypeId"/>).</param>
    /// <returns>The selected role to spawn as, can return <see cref="CustomRoleData"/> to spawn as a custom role or <see cref="RoleTypeId"/> to spawn as a base-game role (or null to exclude the player from the spawn).</returns>
    public abstract object SelectRole(ExPlayer player, Dictionary<ExPlayer, object> selectedRoles);
    
    /// <summary>
    /// Checks if a specific player is spawnable to be included in the next team.
    /// </summary>
    /// <param name="player">The player to spawn.</param>
    /// <returns>true if the player can be spawned</returns>
    public abstract bool IsSpawnable(ExPlayer player);

    /// <summary>
    /// Gets called once the handler is unregistered.
    /// </summary>
    public virtual void OnRegistered() { }
    
    /// <summary>
    /// Gets called once the handler is registered.
    /// </summary>
    public virtual void OnUnregistered() { }

    /// <summary>
    /// Checks if friendly fire is allowed between two players of the same custom team.
    /// </summary>
    /// <param name="attacker">The attacking player.</param>
    /// <param name="target">The player being attacked.</param>
    /// <returns>true if the friendly fire should be allowed</returns>
    public virtual bool CanDamage(ExPlayer attacker, ExPlayer target) => true;

    /// <summary>
    /// Despawns all active instances.
    /// </summary>
    /// <param name="playerRole">The role to set alive players to.</param>
    public abstract void DespawnAll(RoleTypeId playerRole = RoleTypeId.Spectator);
    
    // I realize that this way of doings things may be a bit stupid, but it works.
    
    internal abstract void Internal_RemoveInstance(int id);
    internal abstract bool Internal_DespawnInstance(int id);

    internal abstract IEnumerable<CustomTeamInstance> Internal_GetInstances();

    internal abstract CustomTeamInstance Internal_SpawnInstance(int minPlayerCount, int maxPlayerCount);

    private static void Internal_OnLeft(ExPlayer player)
    {
        if (player.Role.CustomTeam != null)
        {
            player.Role.CustomTeam.OnLeft(player);
            
            player.Role.CustomTeam.AlivePlayers.Remove(player);
            player.Role.CustomTeam.OriginalPlayers.Remove(player);
            
            if (player.Role.CustomTeam.AlivePlayers.Count == 0)
            {
                if (player.Role.CustomTeam.Handler is CustomTeamHandler internalCustomTeamHandler)
                    internalCustomTeamHandler.Internal_RemoveInstance(player.Role.CustomTeam.Id);

                player.Role.CustomTeam.OnDestroy();
            }

            player.Role.CustomTeam = null;
        }
    }

    private static void Internal_OnHurting(PlayerHurtingEventArgs args)
    {
        var attacker = args.Attacker as ExPlayer;
        var player = args.Player as ExPlayer;

        if (player?.ReferenceHub == null)
            return;

        if (attacker?.ReferenceHub != null)
        {
            if (attacker == player)
                return;
                
            if (attacker.Role.CustomTeam?.Handler != null)
            {
                if (player.Role.CustomTeam?.Handler != null)
                {
                    if (player.Role.CustomTeam.Handler == attacker.Role.CustomTeam.Handler &&
                        player.Role.CustomTeam.Handler is CustomTeamHandler handler &&
                        !handler.CanDamage(attacker, player))
                    {
                        args.IsAllowed = false;
                    }
                    else
                    {
                        attacker.Role.CustomTeam.OnAttacking(args);
                        player.Role.CustomTeam.OnHurting(args);
                    }
                }
                else
                {
                    attacker.Role.CustomTeam.OnAttacking(args);
                }
            }
            else
            {
                player.Role.CustomTeam?.OnHurting(args);
            }
        }
        else
        {
            player.Role.CustomTeam?.OnHurting(args);
        }
    }

    private static void Internal_OnHurt(PlayerHurtEventArgs args)
    {
        var attacker = args.Attacker as ExPlayer;
        var player = args.Player as ExPlayer;

        if (player?.ReferenceHub == null)
            return;

        if (attacker?.ReferenceHub != null)
        {
            if (attacker == player)
                return;
                
            if (attacker.Role.CustomTeam?.Handler != null)
            {
                if (player.Role.CustomTeam?.Handler != null)
                {
                    attacker.Role.CustomTeam.OnAttacked(args);
                    player.Role.CustomTeam.OnHurt(args);
                }
                else
                {
                    attacker.Role.CustomTeam.OnAttacked(args);
                }
            }
            else
            {
                player.Role.CustomTeam?.OnHurt(args);
            }
        }
        else
        {
            player.Role.CustomTeam?.OnHurt(args);
        }
    }
    
    private static void Internal_OnDeath(PlayerDeathEventArgs args)
    {
        var attacker = args.Attacker as ExPlayer;
        var player = args.Player as ExPlayer;

        if (player?.ReferenceHub == null)
            return;

        if (attacker?.ReferenceHub != null)
        {
            if (attacker == player)
                return;
                
            if (attacker.Role.CustomTeam?.Handler != null)
            {
                if (player.Role.CustomTeam?.Handler != null)
                {
                    attacker.Role.CustomTeam.OnKilled(args);
                    player.Role.CustomTeam.OnDied(args);
                }
                else
                {
                    attacker.Role.CustomTeam.OnKilled(args);
                }
            }
            else
            {
                player.Role.CustomTeam?.OnDied(args);
            }
        }
        else
        {
            player.Role.CustomTeam?.OnDied(args);
        }
    }

    private static void Internal_OnDying(PlayerDyingEventArgs args)
    {
        var attacker = args.Attacker as ExPlayer;
        var player = args.Player as ExPlayer;

        if (player?.ReferenceHub == null)
            return;

        if (attacker?.ReferenceHub != null)
        {
            if (attacker == player)
                return;
                
            if (attacker.Role.CustomTeam?.Handler != null)
            {
                if (player.Role.CustomTeam?.Handler != null)
                {
                    if (player.Role.CustomTeam.Handler == attacker.Role.CustomTeam.Handler &&
                        player.Role.CustomTeam.Handler is CustomTeamHandler handler &&
                        !handler.CanDamage(attacker, player))
                    {
                        args.IsAllowed = false;
                    }
                    else
                    {
                        attacker.Role.CustomTeam.OnKilling(args);
                        player.Role.CustomTeam.OnDying(args);
                    }
                }
                else
                {
                    attacker.Role.CustomTeam.OnKilling(args);
                }
            }
            else
            {
                player.Role.CustomTeam?.OnDying(args);
            }
        }
        else
        {
            player.Role.CustomTeam?.OnDying(args);
        }
    }
    
    private static void Internal_OnChangingRole(PlayerChangingRoleEventArgs args)
    {
        if (args.Player is not ExPlayer player)
            return;
        
        player.Role.CustomTeam?.OnChangingRole(args);
    }

    private static void Internal_OnChangedRole(PlayerChangedRoleEventArgs args)
    {
        if (args.Player is not ExPlayer player)
            return;

        if (player.Role.CustomTeam != null)
        {
            if (player.Role.CustomTeam.Handler is CustomTeamHandler handler && !string.IsNullOrWhiteSpace(handler?.Name))
            {
                player.CustomInfo = string.Empty;
                player.InfoArea &= ~PlayerInfoArea.CustomInfo;
            }
            
            player.Role.CustomTeam.OnChangedRole(args);
            player.Role.CustomTeam.AlivePlayers.Remove(player);

            if (player.Role.CustomTeam.AlivePlayers.Count == 0)
            {
                if (player.Role.CustomTeam.Handler is CustomTeamHandler internalCustomTeamHandler)
                    internalCustomTeamHandler.Internal_RemoveInstance(player.Role.CustomTeam.Id);

                player.Role.CustomTeam.OnDestroy();
            }
        }

        player.Role.CustomTeam = null;
    }

    [LoaderInitialize(1)]
    private static void Internal_Init()
    {
        PlayerEvents.ChangingRole += Internal_OnChangingRole;
        PlayerEvents.ChangedRole += Internal_OnChangedRole;
        
        PlayerEvents.Hurting += Internal_OnHurting;
        PlayerEvents.Hurt += Internal_OnHurt;
        
        PlayerEvents.Death += Internal_OnDeath;
        PlayerEvents.Dying += Internal_OnDying;

        ExPlayerEvents.Left += Internal_OnLeft;
    }
}