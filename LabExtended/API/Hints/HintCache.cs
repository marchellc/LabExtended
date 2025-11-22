using System.Diagnostics;

using LabExtended.Core.Pooling;
using LabExtended.Extensions;
using LabExtended.Utilities;

// ReSharper disable CompareOfFloatsByEqualityOperator

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.Hints;

/// <summary>
/// A cache for storing temporary hints.
/// </summary>
public class HintCache : PoolObject
{
    /// <summary>
    /// Gets or sets the player that this cache belongs to.
    /// </summary>
    public ExPlayer Player { get; set; }

    /// <summary>
    /// Whether or not an empty hint was sent.
    /// </summary>
    public bool WasClearedAfterEmpty { get; set; }

    /// <summary>
    /// Whether or not the player's hints are paused.
    /// </summary>
    public bool IsPaused { get; set; }
    
    /// <summary>
    /// Whether or not the temporary hint is already parsed.
    /// </summary>
    public bool IsParsed { get; set; }

    /// <summary>
    /// Gets or sets the player's aspect ratio.
    /// </summary>
    public float AspectRatio { get; set; } = 0f;
    
    /// <summary>
    /// Gets or sets the player's offset when using the Left alignment.
    /// </summary>
    public float LeftOffset { get; set; } = 0f;

    /// <summary>
    /// Gets a list of hint messages used as a queue.
    /// </summary>
    public List<(string Content, ushort Duration)> Queue { get; } = new(byte.MaxValue);
    
    /// <summary>
    /// Gets a list of temporary parsed hints.
    /// </summary>
    public List<HintData> TempData { get; } = new(byte.MaxValue);

    /// <summary>
    /// Gets the stopwatch used to detected the end of the temporary hint.
    /// </summary>
    public Stopwatch Stopwatch { get; } = new();

    /// <summary>
    /// Gets or sets the currently displayed message.
    /// </summary>
    public (string Content, ushort Duration)? CurrentMessage { get; set; }

    /// <summary>
    /// Removes the currently displayed hint.
    /// </summary>
    public void RemoveCurrent()
    {
        CurrentMessage = null;

        IsParsed = false;

        if (Stopwatch.IsRunning)
            Stopwatch.Stop();
    }

    /// <summary>
    /// Refreshes the aspect ratio of the player's screen.
    /// </summary>
    /// <returns>true if the aspect ratio was changed</returns>
    public bool RefreshRatio()
    {
        if (AspectRatio != Player.ScreenAspectRatio)
        {
            AspectRatio = Player.ScreenAspectRatio;
            LeftOffset = (int)Math.Round(45.3448f * AspectRatio - 51.527f);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Updates the remaining duration of the currently displayed hint.
    /// </summary>
    /// <returns>true if the hint has expired</returns>
    public bool UpdateTime()
    {
        if (CurrentMessage is null)
            return false;

        if (Stopwatch.Elapsed.Seconds >= CurrentMessage.Value.Duration)
        {
            RemoveCurrent();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Dequeues a new hint message.
    /// </summary>
    /// <returns>true if a new message was dequeued</returns>
    public bool NextMessage()
    {
        RemoveCurrent();

        if (Queue.Count < 1)
            return false;

        CurrentMessage = Queue.RemoveAndTake(0);

        Stopwatch.Restart();
        return true;
    }

    /// <summary>
    /// Parses the current hint.
    /// </summary>
    public void ParseTemp()
    {
        if (IsParsed)
            return;

        if (CurrentMessage is null)
            return;

        var msg = CurrentMessage.Value.Content;

        TempData.Clear();

        msg = msg.Replace("\r\n", "\n")
            .Replace("\\n", "\n")
            .Replace("<br>", "\n")
            .TrimEnd();

        HintUtils.TrimStartNewLines(ref msg, out _);
        HintUtils.GetMessages(msg, TempData, HintController.TemporaryHintVerticalOffset,
            HintController.TemporaryHintAutoWrap, HintController.TemporaryHintPixelSpacing);

        IsParsed = true;
    }

    /// <inheritdoc cref="PoolObject.OnReturned"/>
    public override void OnReturned()
    {
        base.OnReturned();

        RemoveCurrent();

        Queue.Clear();
        TempData.Clear();

        IsPaused = false;

        WasClearedAfterEmpty = false;

        AspectRatio = 0f;
        LeftOffset = 0f;

        Player = null;
    }
}
