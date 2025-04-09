using InventorySystem.Items.Pickups;

using LabExtended.API.CustomEffects.SubEffects;
using LabExtended.Attributes;
using LabExtended.Events;

using PlayerRoles;

using UnityEngine;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API.CustomEffects.Grab;

/// <summary>
/// Used to grab players.
/// </summary>
public class GrabEffect : UpdatingCustomEffect
{
    /// <summary>
    /// Adds a player to a player's grab effect.
    /// </summary>
    /// <param name="sourcePlayer">The player that's grabbing <paramref name="targetPlayer"/></param>
    /// <param name="targetPlayer">The player that's being grabbed by <paramref name="sourcePlayer"/></param>
    /// <returns>true if the player was successfully added</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool AddPlayer(ExPlayer sourcePlayer, ExPlayer targetPlayer)
    {
        if (sourcePlayer is null)
            throw new ArgumentNullException(nameof(sourcePlayer));

        if (targetPlayer is null)
            throw new ArgumentNullException(nameof(targetPlayer));

        if (sourcePlayer == targetPlayer)
            return false;

        if (!sourcePlayer.Effects.TryGetCustomEffect<GrabEffect>(out var grabEffect))
            grabEffect = sourcePlayer.Effects.AddCustomEffect<GrabEffect>();
        
        if (!grabEffect.IsActive)
            grabEffect.Enable();

        if (!grabEffect.Targets.Contains(targetPlayer))
        {
            grabEffect.Targets.Add(targetPlayer);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes a player from a player's grab effect.
    /// </summary>
    /// <param name="sourcePlayer">The player that's grabbing <paramref name="targetPlayer"/></param>
    /// <param name="targetPlayer">The player that's being grabbed by <paramref name="sourcePlayer"/></param>
    /// <returns>true if the player was successfully removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemovePlayer(ExPlayer sourcePlayer, ExPlayer targetPlayer)
    {
        if (sourcePlayer is null)
            throw new ArgumentNullException(nameof(sourcePlayer));

        if (targetPlayer is null)
            throw new ArgumentNullException(nameof(targetPlayer));

        if (sourcePlayer == targetPlayer)
            return false;

        if (!sourcePlayer.Effects.TryGetCustomEffect<GrabEffect>(out var grabEffect))
            grabEffect = sourcePlayer.Effects.AddCustomEffect<GrabEffect>();

        return grabEffect.Targets.Remove(targetPlayer);
    }
    
    /// <summary>
    /// Gets a list of target players.
    /// </summary>
    public List<ExPlayer> Targets { get; } = new();

    /// <summary>
    /// Gets a list of target pickups.
    /// </summary>
    public List<ItemPickupBase> Pickups { get; } = new();
    
    /// <summary>
    /// Gets the object position for grabbed objects.
    /// </summary>
    public Vector3 Position { get; private set; }

    /// <inheritdoc cref="CustomEffect.RoleChanged"/>
    public override bool RoleChanged(RoleTypeId newRole)
        => newRole.IsAlive();

    /// <inheritdoc cref="CustomEffect.RemoveEffects"/>
    public override void RemoveEffects()
    {
        base.RemoveEffects();
        
        Pickups.ForEach(p =>
        {
            if (p != null)
            {
                p.transform.localPosition = Vector3.zero;
                p.transform.localRotation = Quaternion.identity;
                
                p.transform.parent = null;
            }
        });
        
        Targets.Clear();
        Pickups.Clear();
    }

    /// <inheritdoc cref="UpdatingCustomEffect.Update"/>
    public override void Update()
    {
        base.Update();

        if (!IsActive || 
            (Targets.Count < 1
            && Pickups.Count < 1))
            return;
        
        UpdateCameraPosition();

        Targets.ForEach(UpdatePlayer);
        Pickups.ForEach(UpdatePickup);
    }

    private void UpdateCameraPosition()
        => Position = Player.CameraTransform.forward * 1.3f;

    private void UpdatePlayer(ExPlayer? player)
    {
        if (player && player.ReferenceHub != null)
        {
            player.Position.Set(Position);
            player.Rotation.Set(Quaternion.LookRotation(Player.CameraTransform.forward, Player.CameraTransform.up));
        }
    }

    private void UpdatePickup(ItemPickupBase pickup)
    {
        if (pickup != null)
        {
            pickup.transform.localPosition = Position;
            pickup.transform.localRotation = Quaternion.LookRotation(Player.CameraTransform.forward, Player.CameraTransform.up);
            
            if (pickup.transform.parent is null || pickup.transform.parent != Player.CameraTransform)
                pickup.transform.SetParent(Player.CameraTransform);
        }
    }
    
    private static void OnVerified(ExPlayer player)
        => player?.Effects?.AddCustomEffect<GrabEffect>();

    [LoaderInitialize(1)]
    private static void OnInit()
        => InternalEvents.OnPlayerVerified += OnVerified;
}