namespace LabExtended.API.CustomEffects;

public class CustomEffect
{
    public ExPlayer Player { get; internal set; }
    
    public bool IsActive { get; internal set; }
    
    public virtual void Start() { }
    public virtual void Stop() { }
    
    public virtual void ApplyEffects() { }
    public virtual void RemoveEffects() { }

    internal virtual void OnApplyEffects() => ApplyEffects();
    internal virtual void OnRemoveEffects() => RemoveEffects();

    public void Enable()
    {
        IsActive = true;
        OnApplyEffects();
    }

    public void Disable()
    {
        IsActive = false;
        OnRemoveEffects();
    }
}