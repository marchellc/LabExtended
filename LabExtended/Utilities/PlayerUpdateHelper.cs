using LabExtended.Attributes;
using LabExtended.Extensions;
using LabExtended.Utilities.Unity;

using UnityEngine;
using UnityEngine.PlayerLoop;

namespace LabExtended.Utilities;

public class PlayerUpdateHelper
{
    public struct CustomUpdateLoop { }

    public static event Action OnUpdate;

    public static PlayerUpdateReference Register(Action onUpdate, float? timeDelay = null)
    {
        if (onUpdate is null)
            throw new ArgumentNullException(nameof(onUpdate));

        var reference = new PlayerUpdateReference();

        reference.IsEnabled = true;

        if (timeDelay.HasValue && timeDelay.Value > 0f)
            reference.DelayTime = timeDelay.Value;

        reference.TargetUpdate = onUpdate;
        
        reference.OnUpdate = () =>
        {
            if (!reference.IsEnabled || reference.TargetUpdate is null)
            {
                reference.IsEnabled = false;
                
                OnUpdate -= reference.OnUpdate;
                return;
            }

            if (reference.DelayTime > 0f)
            {
                reference.RemainingTime -= Time.deltaTime;

                if (reference.RemainingTime > 0f)
                    return;

                reference.RemainingTime = reference.DelayTime;
            }

            reference.TargetUpdate.InvokeSafe();
        };

        OnUpdate += reference.OnUpdate;
        return reference;
    }
    
    private static void Update()
    {
        OnUpdate.InvokeSafe();
    }
    
    [LoaderInitialize(1)]
    private static void Init()
    {
        PlayerLoopHelper.ModifySystem(x =>
            x.InjectAfter<TimeUpdate.WaitForLastPresentationAndUpdateTime>(Update, typeof(CustomUpdateLoop))
                ? x
                : null);
    }
}