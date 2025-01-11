using LabExtended.Extensions;

namespace LabExtended.Utilities.Unity;

public class PlayerUpdateReference
{
    public float RemainingTime { get; internal set; } = 0f;
    public float DelayTime { get; set; } = 0f;

    public bool IsEnabled { get; internal set; }
    
    public Action OnUpdate { get; internal set; }
    public Action TargetUpdate { get; internal set; }

    public void Disable()
    {
        if (!IsEnabled)
            return;

        IsEnabled = false;
        
        PlayerUpdateHelper.OnUpdate -= OnUpdate;
    }

    public void Enable()
    {
        if (IsEnabled)
            return;

        PlayerUpdateHelper.OnUpdate += OnUpdate;
        
        IsEnabled = true;
    }

    public void Toggle()
    {
        if (IsEnabled)
            Disable();
        else
            Enable();
    }

    public override string ToString()
        => $"PlayerUpdate [{TargetUpdate?.Method?.GetMemberName() ?? "null target method"}] (IsEnabled={IsEnabled}, DelayTime={DelayTime}, RemainingTime={RemainingTime})";
}