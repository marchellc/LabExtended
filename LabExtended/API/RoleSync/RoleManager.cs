using LabExtended.Core;
using LabExtended.Attributes;
using LabExtended.Events;
using LabExtended.Events.Player;
using LabExtended.Utilities.Update;

using Mirror;

using PlayerRoles;

namespace LabExtended.API.RoleSync;

/// <summary>
/// Manages role synchronization.
/// </summary>
public static class RoleManager
{
    /// <summary>
    /// Whether or not roles should be sent next frame.
    /// </summary>
    public static bool SendNextFrame { get; set; }
    
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
    
    /// <summary>
    /// Sends all roles.
    /// </summary>
    public static void SendRoles()
    {
        if (SendNextFrame)
        {
            try
            {
                ExPlayer.AllPlayers.ForEach(ProcessPlayer);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Role Manager", $"Could not send roles!\n{ex}");
            }
        }

        SendNextFrame = false;
    }

    private static void ProcessPlayer(ExPlayer? player)
    {
        if (player?.Role is null || player?.SentRoles is null || player.IsUnverified)
            return;

        var isObfuscated = player.Role.Is<IObfuscatedRole>(out var obfuscatedRole);
        
        ExPlayer.AllPlayers.ForEach(receiver =>
        {
            if (receiver?.Role is null || receiver.IsUnverified || !receiver.IsPlayer)
                return;
            
            var role = player.Role.Type;

            if (isObfuscated)
                role = obfuscatedRole.GetRoleForUser(receiver.ReferenceHub);

            if (player.Role.FakedList.HasGlobalValue)
                role = player.Role.FakedList.GlobalValue;
            else if (player.Role.FakedList.TryGetValue(receiver, out var fakedRole))
                role = fakedRole;

            if (!receiver.Role.IsAlive && !player.Toggles.IsVisibleInSpectatorList)
                role = RoleTypeId.Spectator;

            var synchronizingArgs = new PlayerSynchronizingRoleEventArgs(player, receiver, role);

            if (!ExPlayerEvents.OnSynchronizingRole(synchronizingArgs))
                return;

            role = synchronizingArgs.Role;

            if (player.SentRoles.TryGetValue(receiver.NetworkId, out var sentRole) && sentRole == role)
                return;

            player.SentRoles[receiver.NetworkId] = role;
            
            receiver.Connection.Send(new RoleSyncInfo(player.ReferenceHub, role, receiver.ReferenceHub));
            
            ExPlayerEvents.OnSynchronizedRole(new(player, receiver, role));
        });
    }

    internal static void ProcessJoin(ExPlayer receiver, NetworkWriter writer)
    {
        writer.WriteUShort((ushort)ExPlayer.AllPlayers.Count);

        ExPlayer.AllPlayers.ForEach(player =>
        {
            var role = player.Role.Type;

            if (player.Role.Is<IObfuscatedRole>(out var obfuscatedRole))
                role = obfuscatedRole.GetRoleForUser(receiver.ReferenceHub);
            
            if (player.Role.FakedList.HasGlobalValue)
                role = player.Role.FakedList.GlobalValue;
            else if (player.Role.FakedList.TryGetValue(receiver, out var fakedRole))
                role = fakedRole;
            
            if (!player.Toggles.IsVisibleInSpectatorList)
                role = RoleTypeId.Spectator;

            var synchronizingArgs = new PlayerSynchronizingRoleEventArgs(player, receiver, role);

            if (!ExPlayerEvents.OnSynchronizingRole(synchronizingArgs))
                return;
            
            role = synchronizingArgs.Role;
            
            player.SentRoles?[receiver.NetworkId] = role;
            
            new RoleSyncInfo(player.ReferenceHub, role, receiver.ReferenceHub).Write(writer);
            
            ExPlayerEvents.OnSynchronizedRole(new(player, receiver, role));
        });
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        PlayerUpdateHelper.OnUpdate += SendRoles;
    }
}