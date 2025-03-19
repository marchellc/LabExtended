using System.Text;

using LabExtended.API;
using LabExtended.Events;
using LabExtended.Attributes;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Commands;

/// <summary>
/// A command subtype that allows continuable execution.
/// </summary>
public abstract class ContinuableCommandBase : CommandBase
{
    /// <summary>
    /// Gets the list of pending continuations.
    /// </summary>
    public static Dictionary<uint, ContinuableCommandBase> History { get; } = new();

    /// <summary>
    /// Gets the previously invoked command instance.
    /// </summary>
    public ContinuableCommandBase? Previous { get; internal set; }
    
    /// <summary>
    /// Gets called once a command is continued.
    /// </summary>
    public abstract void OnContinued();

    /// <summary>
    /// Responds to the player with a continuation.
    /// </summary>
    /// <param name="content">The content of the reply.</param>
    public void Continue(object content)
        => Response = new(true, true, content?.ToString() ?? string.Empty);

    /// <summary>
    /// Responds to the player with a continuation.
    /// </summary>
    /// <param name="contentBuilder">The method used to build the content of the reply.</param>
    public void Continue(Action<StringBuilder> contentBuilder)
    {
        if (contentBuilder is null)
            throw new ArgumentNullException(nameof(contentBuilder));

        Response = new(true, true, StringBuilderPool.Shared.BuildString(contentBuilder));
    }

    private static void OnPlayerLeft(ExPlayer player)
        => History.Remove(player.NetworkId);

    [LoaderInitialize(1)]
    private static void OnInit()
        => InternalEvents.OnPlayerLeft += OnPlayerLeft;
}