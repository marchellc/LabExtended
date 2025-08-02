using LabExtended.Events;
using LabExtended.Events.Player;

using Mirror;

using PlayerRoles;
using PlayerRoles.Visibility;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using UnityEngine;

namespace LabExtended.API.RoleSync;

/// <summary>
/// Manages role synchronization.
/// </summary>
public static class RoleManager
{
    /// <summary>
    /// Sets the player's role as dirty for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="receiver">The target receiver.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SetRoleDirty(this ExPlayer player, ExPlayer receiver)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (receiver is null)
            throw new ArgumentNullException(nameof(receiver));

        player.SentRoles?.Remove(receiver.NetworkId);
    }

    internal static void Internal_ProcessPlayer(ExPlayer player, ExPlayer receiver, RoleTypeId? roleOverride = null, bool? isGhosted = null)
    {
        if (player?.Role is null || player?.SentRoles is null || player.IsUnverified)
            return;

        if (receiver?.Role is null || receiver.IsUnverified || !receiver.IsPlayer)
            return;

        var roleToSend = roleOverride ?? Internal_GetCurrentRole(player, receiver, player.Role, isGhosted);

        if (roleToSend is RoleTypeId.None)
            return;
            
        if (player.SentRoles != null)
            player.SentRoles[receiver.NetworkId] = roleToSend;
            
        receiver.Send(new RoleSyncInfo(player.ReferenceHub, roleToSend, receiver.ReferenceHub));
            
        ExPlayerEvents.OnSynchronizedRole(new(player, receiver, roleToSend));
    }

    internal static RoleTypeId Internal_GetCurrentRole(ExPlayer player, ExPlayer receiver, RoleTypeId role, bool? isGhosted = null)
    {
        var isInvisible = isGhosted ?? false;
        var gameplayDataPerms = receiver.HasPermission(PlayerPermissions.GameplayData);
        
        if (player.Role.Is<IObfuscatedRole>(out var obfuscatedRole))
            role = obfuscatedRole.GetRoleForUser(receiver.ReferenceHub);
        
        if (!isGhosted.HasValue && receiver.Role.Is<ICustomVisibilityRole>(out var customVisibilityRole))
            isInvisible = !customVisibilityRole.VisibilityController.ValidateVisibility(player.ReferenceHub);
        
        var maxDistance = player.Team is Team.SCPs
            ? FpcServerPositionDistributor.SCPVisibilityDistance
            : FpcServerPositionDistributor.HumanVisibilityDistance;
        
        var curDistance = receiver.Role.Is<Scp079Role>(out var scp079Role)
            ? Vector3.Distance(scp079Role.CameraPosition, player.Position)
            : Vector3.Distance(receiver.Position, player.Position);

        var isWithinDistance = curDistance <= maxDistance;

        if (role.GetTeam() is not Team.SCPs && !player.ReferenceHub.IsCommunicatingGlobally())
        {
            if (role != RoleTypeId.Spectator && (isInvisible || (!isWithinDistance && !gameplayDataPerms)))
                role = RoleTypeId.Spectator;

            // Some interesting way of confusing cheaters
            if (role is RoleTypeId.Spectator && FpcServerPositionDistributor.IsDistributionActive(role))
                role = RoleTypeId.Overwatch;
        }

        if (player.Role.FakedList.HasGlobalValue)
            role = player.Role.FakedList.GlobalValue;
        else if (player.Role.FakedList.TryGetValue(receiver, out var fakedRole))
            role = fakedRole;
            
        if (!receiver.Role.IsAlive && !player.Toggles.IsVisibleInSpectatorList)
            role = RoleTypeId.Spectator;
        
        var synchronizingArgs = new PlayerSynchronizingRoleEventArgs(player, receiver, role);

        if (!ExPlayerEvents.OnSynchronizingRole(synchronizingArgs))
            return RoleTypeId.None;

        return synchronizingArgs.Role;
    }

    internal static void Internal_SendRole(ExPlayer player, RoleTypeId role)
    {
        ExPlayer.AllPlayers.ForEach(receiver => Internal_ProcessPlayer(player, receiver, role));
    }

    internal static void Internal_ProcessJoin(ExPlayer receiver, NetworkWriter writer)
    {
        writer.WriteUShort((ushort)ExPlayer.AllPlayers.Count);

        ExPlayer.AllPlayers.ForEach(player =>
        {
            var role = Internal_GetCurrentRole(player, receiver, player.Role);

            if (role is RoleTypeId.None)
                return;
            
            if (player.SentRoles != null)
                player.SentRoles[receiver.NetworkId] = role;
            
            new RoleSyncInfo(player.ReferenceHub, role, receiver.ReferenceHub).Write(writer);
            
            ExPlayerEvents.OnSynchronizedRole(new(player, receiver, role));
        });
    }
}