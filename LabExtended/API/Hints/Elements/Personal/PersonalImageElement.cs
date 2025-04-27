using System.Diagnostics;

using LabExtended.API.Enums;
using LabExtended.API.Hints.Interfaces;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API.Hints.Elements.Personal;

/// <summary>
/// A personal hint element used to display images.
/// </summary>
public class PersonalImageElement : PersonalHintElement, IHintRateModifier
{
    private int pixelLineSpacing;
    
    private int frameIndex = 0;
    private int framesPerSecond = 24;

    private bool overridesOthers;
    
    private float verticalOffset;
    private float targetFrameDelay = 1000f / 24;
    
    private HintAlign alignment;
    private Stopwatch watch;
    
    /// <summary>
    /// Creates a new <see cref="PersonalImageElement"/> instance.
    /// </summary>
    /// <param name="overridesOthers">Whether or not this element should override other.</param>
    /// <param name="alignment">The element's text alignment.</param>
    /// <param name="pixelLineSpacing">The element's pixel line spacing.</param>
    /// <param name="verticalOffset">The element's vertical offset.</param>
    public PersonalImageElement(bool overridesOthers = true, HintAlign alignment = DefaultHintAlign, 
        int pixelLineSpacing = DefaultPixelLineSpacing, float verticalOffset = DefaultVerticalOffset)
    {
        this.overridesOthers = overridesOthers;
        this.verticalOffset = verticalOffset;
        this.pixelLineSpacing = pixelLineSpacing;
        this.alignment = alignment;

        watch = new Stopwatch();
    }
    
    /// <inheritdoc cref="HintElement.OverridesOthers"/>
    public override bool OverridesOthers => overridesOthers;

    /// <inheritdoc cref="HintElement.ShouldParse"/>
    public override bool ShouldParse => false;
    
    /// <inheritdoc cref="HintElement.ShouldWrap"/>
    public override bool ShouldWrap => false;

    /// <inheritdoc cref="PersonalHintElement.VerticalOffset"/>
    public override float VerticalOffset => verticalOffset;

    /// <inheritdoc cref="PersonalHintElement.PixelSpacing"/>
    public override int PixelSpacing => pixelLineSpacing;

    /// <inheritdoc cref="PersonalHintElement.Alignment"/>
    public override HintAlign Alignment => alignment;
    
    /// <summary>
    /// Gets or sets the speed of the playback (in frames per second).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int FramesPerSecond
    {
        get => framesPerSecond;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
                                
            framesPerSecond = value;
            targetFrameDelay = 1000f / value;
        }
    }
    
    /// <summary>
    /// Gets the index of the current frame.
    /// </summary>
    public int FrameIndex => frameIndex;
    
    /// <summary>
    /// Gets the amount of all frames.
    /// </summary>
    public int FrameCount => Frames?.Length ?? -1;
    
    /// <summary>
    /// Whether or not an image is currently being displayed.
    /// </summary>
    public bool IsPlaying { get; private set; }
    
    /// <summary>
    /// Whether or not the playback is paused.
    /// </summary>
    public bool IsPaused { get; set; }
    
    /// <summary>
    /// Whether or not the playback is looping.
    /// </summary>
    public bool IsLooping { get; set; }
    
    /// <summary>
    /// Gets an array of compiled image frames.
    /// </summary>
    public string[] Frames { get; private set; }
    
    /// <summary>
    /// Gets the total duration of the image.
    /// </summary>
    public TimeSpan Duration => Frames != null ? TimeSpan.FromSeconds(FrameCount / FramesPerSecond) : TimeSpan.Zero;
    
    /// <summary>
    /// Gets the remaining duration of the image.
    /// </summary>
    public TimeSpan Remaining => Frames != null && IsPlaying ? TimeSpan.FromSeconds((FrameCount - (FrameIndex + 1)) / FramesPerSecond) : TimeSpan.Zero;

    /// <summary>
    /// Starts playing the specified frames.
    /// </summary>
    /// <param name="frames">The frames to play.</param>
    /// <param name="frameRate">The frame rate to play at.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void Play(string[] frames, int? frameRate = null)
    {
        if (frames is null)
            throw new ArgumentNullException(nameof(frames));

        if (frames.Length == 0)
            throw new ArgumentException("Frames cannot be empty", nameof(frames));

        Reset();

        if (frameRate is > 0)
            FramesPerSecond = frameRate.Value;
        else
            FramesPerSecond = 24;

        Frames = frames;

        IsPlaying = true;

        watch.Restart();
    }

    /// <summary>
    /// Resets the playback.
    /// </summary>
    public void Reset()
    {
        IsPlaying = false;

        Frames = null;
        
        if (watch.IsRunning)
        {
            watch.Stop();
            watch.Reset();
        }

        frameIndex = 0;
    }
    
    /// <inheritdoc cref="IHintRateModifier.GetDesiredDelay"/>
    public float GetDesiredDelay(float targetRate)
    {
        if (!IsPlaying || IsPaused)
            return targetRate;

        return targetFrameDelay;
    }

    /// <inheritdoc cref="HintElement.OnUpdate"/>
    public override void OnUpdate()
    {
        if (!IsPlaying)
            return;

        if (!IsPaused && watch.ElapsedMilliseconds >= targetFrameDelay)
        {
            watch.Restart();
            
            frameIndex++;

            if (frameIndex >= Frames.Length)
            {
                if (IsLooping)
                {
                    frameIndex = 0;
                }
                else
                {
                    Reset();
                    return;
                }
            }
        }

        if (frameIndex >= 0 && frameIndex < Frames.Length)
            Builder.AppendLine(Frames[frameIndex]);
    }

    /// <inheritdoc cref="PersonalHintElement.OnDraw()"/>
    public override bool OnDraw()
        => IsPlaying && Builder.Length > 0;
}