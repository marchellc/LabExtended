using System.Diagnostics;

using LabExtended.API.Enums;
using LabExtended.API.Hints.Interfaces;
using LabExtended.Core;

namespace LabExtended.API.Hints.Elements.Personal;

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
    
    public PersonalImageElement(bool overridesOthers = true, HintAlign alignment = HintElement.DefaultHintAlign, 
        int pixelLineSpacing = HintElement.DefaultPixelLineSpacing, float verticalOffset = HintElement.DefaultVerticalOffset)
    {
        this.overridesOthers = overridesOthers;
        this.verticalOffset = verticalOffset;
        this.pixelLineSpacing = pixelLineSpacing;
        this.alignment = alignment;

        watch = new Stopwatch();
    }

    #region Hint Properties
    public override bool OverridesOthers => overridesOthers;

    public override bool ShouldParse => false;
    public override bool ShouldWrap => false;

    public override float VerticalOffset => verticalOffset;

    public override int PixelSpacing => pixelLineSpacing;

    public override HintAlign Alignment => alignment;
    #endregion
    
    #region Playback Properties
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
    
    public int FrameIndex => frameIndex;
    public int FrameCount => Frames?.Length ?? -1;
    
    public bool IsPlaying { get; private set; }
    
    public bool IsPaused { get; set; }
    public bool IsLooping { get; set; }
    
    public string[] Frames { get; private set; }
    
    public TimeSpan Duration => Frames != null ? TimeSpan.FromSeconds(FrameCount / FramesPerSecond) : TimeSpan.Zero;
    public TimeSpan Remaining => Frames != null && IsPlaying ? TimeSpan.FromSeconds((FrameCount - (FrameIndex + 1)) / FramesPerSecond) : TimeSpan.Zero;
    #endregion

    public void Play(string[] frames, int? frameRate = null)
    {
        if (frames is null)
            throw new ArgumentNullException(nameof(frames));

        if (frames.Length == 0)
            throw new ArgumentException("Frames cannot be empty", nameof(frames));

        Reset();

        if (frameRate.HasValue && frameRate.Value > 0)
            FramesPerSecond = frameRate.Value;
        else
            FramesPerSecond = 24;

        Frames = frames;

        IsPlaying = true;

        watch.Restart();
    }

    public float ModifyRate(float targetRate)
    {
        if (!IsPlaying || IsPaused)
            return targetRate;

        return targetFrameDelay;
    }

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

    public override bool OnDraw()
        => IsPlaying && Builder.Length > 0;
}