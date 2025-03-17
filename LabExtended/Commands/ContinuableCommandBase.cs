namespace LabExtended.Commands;

/// <summary>
/// A command subtype that allows continuable execution.
/// </summary>
public abstract class ContinuableCommandBase : CommandBase
{
    /// <summary>
    /// Gets the list of pending continuations.
    /// </summary>
    public Dictionary<uint, ContinuableCommandBase> History { get; } = new();

    /// <summary>
    /// Gets the previously invoked command instance.
    /// </summary>
    public ContinuableCommandBase? Previous { get; internal set; }
    
    /// <summary>
    /// Gets called once a command is continued.
    /// </summary>
    public abstract void OnContinued();
}