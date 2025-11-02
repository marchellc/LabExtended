using System.Text;

using LabExtended.Extensions;

using NorthwoodLib.Pools;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.Commands;

/// <summary>
/// A command subtype that allows continuable execution.
/// </summary>
public abstract class ContinuableCommandBase : CommandBase
{
    /// <summary>
    /// Gets the command's previous context.
    /// </summary>
    public CommandContext PreviousContext { get; internal set; }
    
    /// <summary>
    /// The remaining time till timeout (in seconds).
    /// </summary>
    public float RemainingTime { get; internal set; }
    
    /// <summary>
    /// Gets called once a command is continued.
    /// </summary>
    public abstract void OnContinued();

    /// <summary>
    /// Gets called once the command times out.
    /// </summary>
    public virtual void OnTimedOut() { }
    
    /// <summary>
    /// Gets called once per frame.
    /// </summary>
    public virtual void OnUpdate() { }

    /// <summary>
    /// Responds to the player with a continuation.
    /// </summary>
    /// <param name="content">The content of the reply.</param>
    public void Continue(object content)
        => Response = new(true, true, false, null!, content?.ToString() ?? string.Empty);

    /// <summary>
    /// Responds to the player with a continuation.
    /// </summary>
    /// <param name="contentBuilder">The method used to build the content of the reply.</param>
    public void Continue(Action<StringBuilder> contentBuilder)
    {
        if (contentBuilder is null)
            throw new ArgumentNullException(nameof(contentBuilder));

        Response = new(true, true, false, null!, StringBuilderPool.Shared.BuildString(contentBuilder));
    }
}