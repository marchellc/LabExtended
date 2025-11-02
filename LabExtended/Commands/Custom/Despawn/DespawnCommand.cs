using LabExtended.API;
using LabExtended.Utilities;
using LabExtended.Extensions;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using Mirror;

using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace LabExtended.Commands.Custom.Despawn;

/// <summary>
/// Commands that help with despawning.
/// </summary>
[Attributes.Command("despawn", "Despawns an object.")]
public class DespawnCommand : CommandBase, IRemoteAdminCommand
{
    /// <summary>
    /// Despawns the object that the specified player is currently looking at by performing a raycast from their camera
    /// position.
    /// </summary>
    /// <remarks>This command can only be executed from the Remote Admin panel and cannot be used by the
    /// server. If the raycast hits a player, that player is killed. If it hits a networked object, the object is
    /// destroyed. If the raycast does not hit a valid target, an error message is returned.</remarks>
    /// <param name="originPlayer">The player whose camera is used as the origin for the raycast. If null, the command sender is used.</param>
    /// <param name="distance">The maximum distance, in units, for the raycast to detect objects. Must be positive.</param>
    [CommandOverload("Despawns an object that you're currently looking at.", null)]
    public void RaycastOverload(
        [CommandParameter("Player", "The player of which camera will be used")] ExPlayer? originPlayer = null, 
        [CommandParameter("Distance", "The maximum hit distance")] float distance = 50f)
    {
        var origin = originPlayer ?? Sender;

        if (origin.IsServer)
        {
            Fail("This command can be used only from the Remote Admin panel");
            return;
        }
        
        if (!origin.Rotation.CastRay(distance, PhysicsUtils.VisibleMask | PhysicsUtils.PlayerCollisionMask, out var hit))
        {
            Fail("Raycast failed.");
            return;
        }

        if (hit.TryFindComponent<ReferenceHub>(out var referenceHub))
        {
            if (!ExPlayer.TryGet(referenceHub, out var player))
            {
                Fail("Could not get target player instance.");
                return;
            }

            if (player == Sender)
            {
                Fail("You hit yourself.");
                return;
            }

            player.IsGodModeEnabled = false;
            player.Kill($"Killed by {Sender.Nickname}");
            
            Ok($"Killed player \"{player.Nickname} ({player.UserId})\".");
        }
        else if (hit.TryFindComponent<NetworkIdentity>(out var networkIdentity))
        {
            Ok($"Destroyed network object \"{networkIdentity.name} ({networkIdentity.netId})\"");
            
            NetworkServer.Destroy(networkIdentity.gameObject);
        }
        else
        {
            Fail($"Target object cannot be destroyed ({hit.collider.name} - {hit.collider.tag})");
        }
    }

    /// <summary>
    /// Despawns a network object with the specified network ID.
    /// </summary>
    /// <param name="id">The unique identifier of the network object to despawn.</param>
    [CommandOverload("id", "Despawns a network object by it's ID.", null)]
    public void NetworkIdOverload(
        [CommandParameter("ID", "ID of the network object.")] uint id)
    {
        if (!NetworkServer.spawned.TryGetValue(id, out var networkObject))
        {
            Fail($"Could not find network object with ID {id}");
            return;
        }
        
        NetworkServer.Destroy(networkObject.gameObject);
        
        Ok($"Destroyed network object with ID {id} ({networkObject.name})");
    }
}