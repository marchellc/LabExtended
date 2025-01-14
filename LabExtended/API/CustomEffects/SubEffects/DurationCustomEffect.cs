using UnityEngine;

namespace LabExtended.API.CustomEffects.SubEffects;

public abstract class DurationCustomEffect : UpdatingCustomEffect
{
    public float Remaining { get; private set; } = 0f;

    public abstract float GetDuration();
    
    public virtual float CheckDuration() => 0f;

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
        
        base.OnApplyEffects();
    }

    internal override void OnRemoveEffects()
    {
        Remaining = 0f;
        base.OnRemoveEffects();
    }
}