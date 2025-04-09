using LabExtended.API.CustomEffects.SubEffects;
using LabExtended.Attributes;
using LabExtended.Events;
using PlayerRoles;

using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API.CustomEffects.Rocket;

/// <summary>
/// An effect which sends the target user to space upon activation.
/// </summary>
public class RocketEffect : UpdatingCustomEffect
{
    /// <summary>
    /// Enables the rocket effect on the target player, effectively sending them to space.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <returns>true if the effect was enabled</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool Apply(ExPlayer target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (!target.Effects.TryGetCustomEffect<RocketEffect>(out var rocket))
            rocket = target.Effects.AddCustomEffect<RocketEffect>();
        
        if (!rocket.IsActive && target.Role.IsAlive)
            rocket.Enable();

        return rocket.IsActive;
    }

    /// <summary>
    /// Disables the rocket effect on the target player.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <returns>true if the effect was disabled</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool Remove(ExPlayer target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (!target.Effects.TryGetCustomEffect<RocketEffect>(out var rocket))
            rocket = target.Effects.AddCustomEffect<RocketEffect>();

        if (!rocket.IsActive) 
            return false;
        
        rocket.Disable();
        return true;
    }
    
    /// <summary>
    /// Gets or sets the maximum Y axis value. The player will die upon reaching this.
    /// </summary>
    public float YMax { get; set; } = 100f;

    /// <summary>
    /// Gets or sets the Y axis step up per each frame.
    /// </summary>
    public float YStep { get; set; } = 5f;

    /// <summary>
    /// Whether or not the player should die on the next frame.
    /// </summary>
    public bool ShouldDie
    {
        get
        {
            if (!IsActive || !Player || !Player.Role.IsAlive)
                return false;

            if (YMax > 0f && Player.Position.Position.y >= YMax)
                return true;

            return false;
        }
    }
    
    /// <summary>
    /// Gets the player's new position with applied Y axis step up.
    /// </summary>
    public Vector3 NewPosition
    {
        get
        {
            var position = Player.Position.Position;

            position.y += YStep;
            return position;
        }
    }
    
    /// <inheritdoc cref="CustomEffect.RoleChanged"/>
    public override bool RoleChanged(RoleTypeId newRole) 
        => newRole.IsAlive();

    /// <inheritdoc cref="UpdatingCustomEffect.Update"/>
    public override void Update()
    {
        base.Update();

        if (!IsActive || !Player || !Player.Role.IsAlive)
            return;

        if (ShouldDie)
        {
            Disable();
            
            Player.Kill("Flown too close to the sun.");
            return;
        }

        Player.Position.Set(NewPosition);
    }

    // Adds the effect to every player without enabling it.
    private static void OnVerified(ExPlayer player)
        => player?.Effects?.AddCustomEffect<RocketEffect>();

    [LoaderInitialize(1)]
    private static void OnInit()
        => InternalEvents.OnPlayerVerified += OnVerified;
}