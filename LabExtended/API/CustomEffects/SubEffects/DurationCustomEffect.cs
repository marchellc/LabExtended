using UnityEngine;

namespace LabExtended.API.CustomEffects.SubEffects;

/// <summary>
/// A subtype of UpdatingCustomEffect which adds a duration property.
/// </summary>
public abstract class DurationCustomEffect : UpdatingCustomEffect
{
    /// <summary>
    /// Gets or sets the remaining duration (in seconds).
    /// </summary>
    public float Remaining { get; set; } = 0f;

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

    /// <inheritdoc cref="UpdatingCustomEffect.Update"/>
    public override void Update()
    {
        base.Update();

        if (!IsActive)
            return;

        Remaining -= Time.deltaTime;

        if (Remaining <= 0f)
        {
            var addDuration = CheckDuration();

            if (addDuration > 0f)
            {
                Remaining += addDuration;
                return;
            }
            
            Remaining = 0f;
            
            IsActive = false;
            
            RemoveEffects();
        }
    }
    
    internal override void OnApplyEffects()
    {
        Remaining = GetDuration();
        IsActive = Remaining > 0f;
        
        if (IsActive)
            base.OnApplyEffects();
    }

    internal override void OnRemoveEffects()
    {
        Remaining = 0f;
        base.OnRemoveEffects();
    }
}