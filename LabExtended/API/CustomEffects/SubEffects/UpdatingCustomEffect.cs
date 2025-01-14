using LabExtended.Utilities;

using UnityEngine;

namespace LabExtended.API.CustomEffects.SubEffects;

public class UpdatingCustomEffect : CustomEffect
{
    private float _remainingTime = 0f;
    
    public virtual float Delay { get; } = 0f;
    
    public virtual void Update() { }

    public override void Start()
    {
        base.Start();
        PlayerUpdateHelper.OnUpdate += OnUpdate;
    }

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