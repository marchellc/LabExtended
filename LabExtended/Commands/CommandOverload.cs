namespace LabExtended.Commands;

using Parameters;

/// <summary>
/// Represents a method of a command.
/// </summary>
public class CommandOverload
{
    /// <summary>
    /// Whether or not this overload is a coroutine.
    /// </summary>
    public bool IsCoroutine { get; }
    
    /// <summary>
    /// Gets the amount of required parameters.
    /// </summary>
    public int RequiredParameters { get; internal set; }
    
    /// <summary>
    /// Gets all parameters from this overload.
    /// </summary>
    public List<CommandParameter> Parameters { get; } = new();
    
    /// <summary>
    /// Gets the compiled method delegate.
    /// </summary>
    public Func<object, object[], object>? Method { get; internal set; }

    /// <summary>
    /// Creates a new <see cref="CommandOverload"/> instance.
    /// </summary>
    public CommandOverload(bool isCoroutine)
    {
        IsCoroutine = isCoroutine;
    }
}