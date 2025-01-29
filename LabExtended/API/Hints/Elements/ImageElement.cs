using LabExtended.API.Enums;

using UnityEngine;

namespace LabExtended.API.Hints.Elements;

public class ImageElement : HintElement
{
    private int frameIndex = -2;
    private int framesPerSecond = 24;

    private float targetFrameDelay = 1f / 24;
    private float currentFrameDelay = 0f;

    private float remainingDuration = -1f;
    
    public string[] Frames { get; private set; }

    public int FramesPerSecond
    {
        get => framesPerSecond;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
                                
            framesPerSecond = value;
            targetFrameDelay = 1f / value;
        }
    }

    public float FrameDelay => targetFrameDelay;
    
    public int FrameIndex => frameIndex;
    public int FrameCount => Frames.Length;
    
    public int NextFrameIndex => frameIndex + 1;
    
    public bool IsFinished => !IsPlaying || frameIndex + 1 >= Frames.Length;
    public bool IsPlaying => Frames != null && frameIndex != -2;
    
    public bool IsPaused { get; set; }
    
    public string CurrentFrame => Frames[FrameIndex];
    public string NextFrame => Frames[NextFrameIndex];
    
    public TimeSpan Duration => Frames != null ? TimeSpan.FromSeconds(FrameCount / FramesPerSecond) : TimeSpan.Zero;
    public TimeSpan Remaining => Frames != null && IsPlaying ? TimeSpan.FromSeconds((FrameCount - (FrameIndex + 1)) / FramesPerSecond) : TimeSpan.Zero;
    
    public Func<ExPlayer, bool> Predicate { get; set; }

    public HintAlign Alignment { get; set; } = HintElement.DefaultHintAlign;
    public float VerticalOffset { get; set; } = HintElement.DefaultVerticalOffset;
    public int PixelSpacing { get; set; } = HintElement.DefaultPixelLineSpacing;

    public override HintAlign GetAlignment(ExPlayer player) => Alignment;
    public override float GetVerticalOffset(ExPlayer player) => VerticalOffset;
    public override int GetPixelSpacing(ExPlayer player) => PixelSpacing;

    public void Play(string[] frames, int? frameRate = null, int? duration = null)
    {
        if (frames is null)
            throw new ArgumentNullException(nameof(frames));
        
        if (frames.Length == 0)
            throw new ArgumentException("Frames cannot be empty", nameof(frames));
        
        Frames = frames;
        
        if (frameRate.HasValue && frameRate.Value > 0)
            FramesPerSecond = frameRate.Value;
        else
            FramesPerSecond = 24;

        if (duration.HasValue && duration.Value > 0)
            remainingDuration = duration.Value;
        else
            remainingDuration = -1f;

        frameIndex = -1;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (!IsPlaying)
            return;
        
        currentFrameDelay -= Time.deltaTime;

        if (currentFrameDelay > 0f)
            return;

        currentFrameDelay = targetFrameDelay;
        
        if (!IsPaused)
        {
            frameIndex++;

            if (frameIndex >= Frames.Length)
            {
                OnFinished();
                return;
            }
        }
        
        if (remainingDuration != -1f)
        {
            remainingDuration -= Time.deltaTime;

            if (remainingDuration <= 0f)
            {
                OnFinished();
                return;
            }
        }

        Builder.AppendLine(Frames[frameIndex]);
    }

    public override bool OnDraw(ExPlayer player)
    {
        if (Predicate != null && !Predicate(player))
            return false;

        return IsPlaying;
    }

    private void OnFinished()
    {
        frameIndex = -2;
        currentFrameDelay = 0f;
        remainingDuration = -1f;
    }

    public override string ToString()
        => $"ImageElement (Frames={Frames?.Length} ?? -1; TargetDelay={targetFrameDelay}; CurrentDelay={currentFrameDelay}; Fps={FramesPerSecond}; CurrentIndex={frameIndex}; Duration={Duration}; Remaining={Remaining}; IsPaused={IsPaused})";
}