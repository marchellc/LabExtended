using LabExtended.Attributes;
using LabExtended.Utilities.Update;

using UnityEngine;

namespace LabExtended.API.Custom.Effects;

/// <summary>
/// A subtype of CustomEffect which adds ticking abilities.
/// </summary>
[LoaderIgnore]
public class CustomTickingEffect : CustomPlayerEffect
{
    private float delayTime = 0f;
    
    /// <summary>
    /// Custom delay between each tick.
    /// </summary>
    public virtual float Delay { get; } = 0f;
    
    /// <summary>
    /// Called once a frame.
    /// </summary>
    public virtual void Tick() { }

    /// <inheritdoc cref="CustomPlayerEffect.Start"/>
    public override void Start()
    {
        base.Start();
        PlayerUpdateHelper.OnUpdate += OnUpdate;
    }

    /// <inheritdoc cref="CustomPlayerEffect.Stop"/>
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
            delayTime -= Time.deltaTime;

            if (delayTime > 0f)
                return;
            
            delayTime = Delay;
        }

        Tick();
    }
}