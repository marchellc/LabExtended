using LabExtended.API.CustomEffects.SubEffects;
using LabExtended.Attributes;
using LabExtended.Core.Networking.Manipulation;
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
    /// Gets or sets the target object.
    /// </summary>
    public NetworkObjectManipulator? Target
    {
        get => field;
        set
        {
            field?.Dispose();
            field = value;

            if (value is null)
                return;

            if (value.Target.SupportsParenting)
            {
                value.Target.LocalPosition = Vector3.forward * 2f;
                value.Target.ChangeParent(Player.CameraTransform);
            }
        }
    }
    
    /// <inheritdoc cref="CustomEffect.RoleChanged"/>
    public override bool RoleChanged(RoleTypeId newRole)
        => newRole.IsAlive();

    /// <inheritdoc cref="CustomEffect.ApplyEffects"/>
    public override void ApplyEffects()
    {
        base.ApplyEffects();

        if (!Physics.Raycast(Player.CameraTransform.position, Player.CameraTransform.forward, out var hit, 30f,
                Physics.AllLayers)
            || !NetworkObjectManipulator.FromRaycast(hit, out var target))
        {
            Disable();
            return;
        }
        
        Target?.Dispose();
        Target = target;
    }

    /// <inheritdoc cref="CustomEffect.RemoveEffects"/>
    public override void RemoveEffects()
    {
        base.RemoveEffects();
        
        Target?.Dispose();
        Target = null;
    }

    /// <inheritdoc cref="UpdatingCustomEffect.Update"/>
    public override void Update()
    {
        base.Update();

        if (Target is { Target.IsAlive: true })
        {
            Target.Update();
            
            if (Target.Target.SupportsParenting)
            {
                Target.Target.LocalRotation = Quaternion.LookRotation(Player.CameraTransform.position, Vector3.up);
            }
            else
            {
                Target.Target.ChangeProperties(Player.CameraTransform.forward + (Vector3.forward * 2f), null, 
                    Quaternion.LookRotation(Player.CameraTransform.position, Vector3.up));
            }
        }
    }
    
    private static void OnVerified(ExPlayer player)
        => player?.Effects?.AddCustomEffect<GrabEffect>();

    [LoaderInitialize(1)]
    private static void OnInit()
        => InternalEvents.OnPlayerVerified += OnVerified;
}