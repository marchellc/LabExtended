using LabExtended.Utilities.Update;

using UnityEngine;

namespace LabExtended.API.CustomEffects.SubEffects;

/// <summary>
/// A subtype of CustomEffect which adds ticking abilities.
/// </summary>
public class UpdatingCustomEffect : CustomEffect
{
    private float _remainingTime = 0f;
    
    /// <summary>
    /// The custom delay between each tick.
    /// </summary>
    public virtual float Delay { get; } = 0f;
    
    /// <summary>
    /// Called once a frame.
    /// </summary>
    public virtual void Update() { }

    /// <inheritdoc cref="CustomEffect.Start"/>
    public override void Start()
    {
        base.Start();
        PlayerUpdateHelper.OnUpdate += OnUpdate;
    }

    /// <inheritdoc cref="CustomEffect.Stop"/>
    public override void Stop()
    {
        base.Stop();
        PlayerUpdateHelper.OnUpdate -= OnUpdate;
    }

    private void OnUpdate()
    {
        if (!IsActive)
            return;

        if (Delay > 0f)
        {
            _remainingTime -= Time.deltaTime;

            if (_remainingTime > 0f)
                return;
            
            _remainingTime = Delay;
        }

        Update();
    }
}