using LabExtended.API;

using LabExtended.Events;
using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Events.Player;

using Mirror;

using PlayerRoles;
using PlayerRoles.SpawnData;
using PlayerRoles.Visibility;
using PlayerRoles.Subroutines;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using UnityEngine;

namespace LabExtended.Utilities;

/// <summary>
/// Custom role synchronization.
/// </summary>
public static class RoleSync
{
    /// <summary>
    /// Gets the maximum distance between players for their SCP role to be synced.
    /// </summary>
    public const float ScpMaxDistance = 110f;

    /// <summary>
    /// Gets the maximum distance between players for their human role to be synced.
    /// </summary>
    public const float HumanMaxDistance = 50f;

    /// <summary>
    /// Whether or not the custom role sync should be enabled.
    /// </summary>
    public static bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Whether or not the Overwatch spoof should be enabled.
    /// <remarks>When enabled, players currently playing as Spectator have a chance of being reported as Overwatch to non-staff players.</remarks>
    /// </summary>
    public static bool IsOverwatchSpoofEnabled { get; set; } = true;

    /// <summary>
    /// Gets the role which should be sent to a specific player (with a chance of applying the Overwatch spoof).
    /// </summary>
    /// <param name="player">The player who owns the role.</param>
    /// <param name="receiver">The player receiving the role.</param>
    /// <returns>The role which should be sent.</returns>
    public static RoleTypeId GetSpoofedRoleToSend(ExPlayer player, ExPlayer receiver)
    {
        var role = GetRoleToSend(player, receiver);

        if (IsOverwatchSpoofEnabled && FpcServerPositionDistributor.IsDistributionActive(role))
            role = RoleTypeId.Overwatch;

        return role;
    }

    /// <summary>
    /// Gets the role which should be sent to a specific player.
    /// </summary>
    /// <param name="player">The player who owns the role.</param>
    /// <param name="receiver">The player receiving the role.</param>
    /// <returns>The role which should be sent.</returns>
    public static RoleTypeId GetRoleToSend(ExPlayer player, ExPlayer receiver)
    {
        if (player is null || receiver is null)
            return RoleTypeId.None;

        var role = player.Role.Type;

        if (player.Role.Is<IObfuscatedRole>(out var obfuscatedRole))
            role = obfuscatedRole.GetRoleForUser(receiver.ReferenceHub);

        if (player.Role.FakedList.HasGlobalValue)
            role = player.Role.FakedList.GlobalValue;

        if (player.Role.FakedList.TryGetValue(receiver, out var fakedValue))
            role = fakedValue;

        if (!player.ReferenceHub.IsCommunicatingGlobally())
        {
            var invisible = false;
        
            var permissions = PermissionsHandler.IsPermitted(receiver.ReferenceHub.serverRoles.Permissions,
                PlayerPermissions.GameplayData);
        
            var distance = player.Role.Is<Scp079Role>(out var scpRole)
                ? Vector3.Distance(scpRole.CameraPosition, receiver.Transform.position)
                : Vector3.Distance(player.Transform.position, receiver.Transform.position);
        
            var inRange = player.Role.IsScp
                ? distance <= ScpMaxDistance
                : distance <= HumanMaxDistance;
            
            if (receiver.Role.Is<ICustomVisibilityRole>(out var visibilityRole))
                invisible = !visibilityRole.VisibilityController.ValidateVisibility(player.ReferenceHub);

            if (invisible && !inRange && !permissions && receiver.Role.Type != RoleTypeId.Spectator)
                role = RoleTypeId.Spectator;
        }

        return role;
    }

    internal static void Internal_Resync(ExPlayer player)
    {
        using (var writer = NetworkWriterPool.Get())
        {
            ExPlayer.Players.ForEach(receiver =>
            {
                writer.Reset();

                if (receiver.ReferenceHub != null
                    && receiver.IsOnlineAndVerified)
                {
                    var role = GetRoleToSend(player, receiver);
                    var fakeRole = FpcServerPositionDistributor.InvokeRoleSyncEvent(player.ReferenceHub, receiver.ReferenceHub, role, writer);

                    if (fakeRole.HasValue)
                        role = fakeRole.Value;

                    if (!player.SentRoles.TryGetValue(receiver.NetworkId, out var sentRole)
                        || sentRole != role)
                    {
                        var synchronizingArgs = new PlayerSynchronizingRoleEventArgs(player, receiver, role, writer);

                        if (!ExPlayerEvents.OnSynchronizingRole(synchronizingArgs))
                            return;

                        role = synchronizingArgs.Role;

                        player.SentRoles[receiver.NetworkId] = role;

                        receiver.Send(new RoleSyncInfo(player.ReferenceHub, role, receiver.ReferenceHub, writer));

                        ExPlayerEvents.OnSynchronizedRole(new(player, receiver, role));
                    }
                }
            });
        }
    }

    internal static void Internal_CheckDirtyRole(ExPlayer player, ExPlayer receiver)
    {
        using (var writer = NetworkWriterPool.Get())
        {
            var role = GetRoleToSend(player, receiver);
            var fakedRole = FpcServerPositionDistributor.InvokeRoleSyncEvent(player.ReferenceHub, receiver.ReferenceHub, role, writer);

            if (fakedRole.HasValue)
                role = fakedRole.Value;

            if (!player.SentRoles.TryGetValue(receiver.NetworkId, out var sentRole) || sentRole != role)
            {
                var synchronizingArgs = new PlayerSynchronizingRoleEventArgs(player, receiver, role, writer);

                if (!ExPlayerEvents.OnSynchronizingRole(synchronizingArgs))
                    return;

                role = synchronizingArgs.Role;

                player.SentRoles[receiver.NetworkId] = role;

                if (IsOverwatchSpoofEnabled && FpcServerPositionDistributor.IsDistributionActive(role))
                    role = RoleTypeId.Overwatch;

                receiver.Send(new RoleSyncInfo(player.ReferenceHub, role, receiver.ReferenceHub, writer));

                if (role == player.Role.Type && player.Role.Is<ISubroutinedRole>(out var subroutinedRole))
                {
                    foreach (var subroutine in subroutinedRole.SubroutineModule.AllSubroutines)
                    {
                        receiver.Send(new SubroutineMessage(subroutine, true));
                    }
                }

                ExPlayerEvents.OnSynchronizedRole(new(player, receiver, role));
            }
        }
    }
    
    private static void Internal_Verified(ExPlayer player)
    {
        player.Connection.WriteTo(writer =>
        {
            using (var spoofWriter = NetworkWriterPool.Get())
            {
                var count = 0;
                var position = 0;

                writer.WriteMessageId<RoleSyncInfoPack>();

                position = writer.Position;

                writer.Position += 2; // Skip UShort count

                ExPlayer.AllPlayers.ForEach(p =>
                {
                    if (p?.ReferenceHub != null)
                    {
                        spoofWriter.Reset();

                        var role = GetRoleToSend(p, player);
                        var fakeRole = FpcServerPositionDistributor.InvokeRoleSyncEvent(p.ReferenceHub, player.ReferenceHub, role, spoofWriter);

                        if (fakeRole.HasValue)
                            role = fakeRole.Value;

                        var synchronizingArgs = new PlayerSynchronizingRoleEventArgs(p, player, role, spoofWriter);

                        if (!ExPlayerEvents.OnSynchronizingRole(synchronizingArgs))
                            return;

                        role = synchronizingArgs.Role;

                        writer.WriteUInt(p.NetworkId);
                        writer.WriteRoleType(role);

                        if (spoofWriter.Position > 0)
                        {
                            for (var x = 0; x < spoofWriter.Position; x++)
                            {
                                writer.WriteByte(spoofWriter.buffer[x]);
                            }
                        }

                        if (role == p.Role.Type)
                        {
                            if (p.Role.Is<IPublicSpawnDataWriter>(out var publicSpawnDataWriter))
                            {
                                publicSpawnDataWriter.WritePublicSpawnData(writer);
                            }

                            if (p == player && p.Role.Is<IPrivateSpawnDataWriter>(out var privateSpawnDataWriter))
                            {
                                privateSpawnDataWriter.WritePrivateSpawnData(writer);
                            }
                        }
                        else if (role.TryGetPrefab(out var prefab))
                        {
                            if (prefab is IPublicSpawnDataWriter publicSpawnDataWriter)
                            {
                                publicSpawnDataWriter.WritePublicSpawnData(writer);
                            }

                            if (p == player && prefab is IPrivateSpawnDataWriter privateSpawnDataWriter)
                            {
                                privateSpawnDataWriter.WritePrivateSpawnData(writer);
                            }
                        }

                        p.SentRoles[player.NetworkId] = role;

                        ExPlayerEvents.OnSynchronizedRole(new(p, player, role));

                        count++;
                    }
                });

                var curPosition = writer.Position;

                writer.Position = position;
                writer.WriteUShort((ushort)count);
                writer.Position = curPosition;
            }
        });
    }

    [LoaderInitialize(1)]
    private static void Internal_Init()
    {
        if (!IsEnabled)
            return;
        
        InternalEvents.OnPlayerVerified += Internal_Verified;
        
        ReferenceHub.OnPlayerAdded -= PlayerRolesNetUtils.HandleSpawnedPlayer;
    }
}