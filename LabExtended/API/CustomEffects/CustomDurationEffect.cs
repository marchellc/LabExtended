using LabExtended.Attributes;
using UnityEngine;

namespace LabExtended.API.CustomEffects;

/// <summary>
/// A subtype of UpdatingCustomEffect which adds a duration property.
/// </summary>
[LoaderIgnore]
public abstract class CustomDurationEffect : CustomTickingEffect
{
    /// <summary>
    /// Gets or sets the remaining duration (in seconds).
    /// </summary>
    public float RemainingDuration { get; set; } = 0f;

    /// <summary>
    /// Gets the effect's default duration.
    /// </summary>
    /// <returns>The effect's duration.</returns>
    public abstract float GetDuration();
    
    /// <summary>
    /// Called once the remaining duration reaches zero.
    /// </summary>
    /// <returns>Seconds to add to remaining duration.</returns>
    public virtual float CheckDuration() => 0f;

    /// <inheritdoc cref="CustomTickingEffect.Tick"/>
    public override void Tick()
    {
        base.Tick();

        if (!IsActive)
            return;

        RemainingDuration -= Time.deltaTime;

        if (RemainingDuration <= 0f)
        {
            var addDuration = CheckDuration();

            if (addDuration > 0f)
            {
                RemainingDuration += addDuration;
                return;
            }
            
            RemainingDuration = 0f;
            
            IsActive = false;
            
            RemoveEffects();
        }
    }
    
    internal override void OnApplyEffects()
    {
        RemainingDuration = GetDuration();
        IsActive = RemainingDuration > 0f;
        
        if (IsActive)
            base.OnApplyEffects();
    }

    internal override void OnRemoveEffects()
    {
        RemainingDuration = 0f;
        base.OnRemoveEffects();
    }
}