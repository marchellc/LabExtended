using NetworkManagerUtils.Dummies;

namespace LabExtended.API.RemoteAdmin.Actions;

/// <summary>
/// Base class for Remote Admin actions.
/// </summary>
public class RemoteAdminAction : IDisposable
{
    /// <summary>
    /// Gets the action's name.
    /// </summary>
    public virtual string Name { get; } = string.Empty;
    
    /// <summary>
    /// Gets the parent player.
    /// </summary>
    public ExPlayer Player { get; internal set; }
    
    /// <summary>
    /// Gets the parent module.
    /// </summary>
    public RemoteAdminActionModule Module { get; internal set; }

    /// <summary>
    /// Gets called once the action is added.
    /// </summary>
    public virtual void Initialize()
    {
        
    }
    
    /// <summary>
    /// Gets called once the action button is pressed.
    /// </summary>
    public virtual void Invoke()
    {
        
    }
    
    /// <summary>
    /// Gets called once the parent player leaves.
    /// </summary>
    public virtual void Dispose()
    {
        
    }

    internal DummyAction ToDummyAction()
    {
        return new(Name, Invoke);
    }
}