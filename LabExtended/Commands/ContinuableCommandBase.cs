using System.Text;

using LabExtended.API;
using LabExtended.Events;
using LabExtended.Attributes;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

using UnityEngine;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.Commands;

/// <summary>
/// A command subtype that allows continuable execution.
/// </summary>
public abstract class ContinuableCommandBase : CommandBase
{
    internal float remainingTime = 0f;
    
    internal bool updateAssigned;
    internal bool hasExpired;
    
    /// <summary>
    /// Gets the list of pending continuations.
    /// </summary>
    public static Dictionary<uint, ContinuableCommandBase> History { get; } = new();
    
    /// <summary>
    /// Gets the command's previous context.
    /// </summary>
    public CommandContext PreviousContext { get; internal set; }
    
    /// <summary>
    /// The remaining time till timeout (in seconds).
    /// </summary>
    public float RemainingTime => remainingTime;

    /// <summary>
    /// Whether or not the command has timed out.
    /// </summary>
    public bool HasTimedOut => hasExpired;
    
    /// <summary>
    /// Gets called once a command is continued.
    /// </summary>
    public abstract void OnContinued();

    /// <summary>
    /// Gets called once the command times out.
    /// </summary>
    public virtual void OnTimedOut() { }

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

    internal void Reset()
    {
        hasExpired = false;
    }

    internal void Update()
    {
        if (!hasExpired)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                hasExpired = true;
                
                OnTimedOut();

                if (History.TryGetKey(this, out var netId))
                    History.Remove(netId);
            }
        }
    }
    
    private static void OnPlayerLeft(ExPlayer player)
        => History.Remove(player.NetworkId);

    [LoaderInitialize(1)]
    private static void OnInit()
        => InternalEvents.OnPlayerLeft += OnPlayerLeft;
}