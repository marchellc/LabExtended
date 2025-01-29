using LabExtended.API.Enums;
using LabExtended.Core;
using UnityEngine;

namespace LabExtended.API.Hints.Elements;

public class PersonalImageElement : PersonalElement
{
    private int frameIndex = 0;
    private int framesPerSecond = 24;

    private float targetFrameDelay = 0f;
    private float currentFrameDelay = 0f;
    
    public string[] Frames { get; private set; }

    public int FramesPerSecond
    {
        get => framesPerSecond;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
                                
            framesPerSecond = value;
            targetFrameDelay = (1f / value) * 100;
        }
    }

    public float FrameDelay => targetFrameDelay;

    public int CustomFrameDelay { get; set; } = 0;
    
    public int FrameIndex => frameIndex;
    public int FrameCount => Frames.Length;
    
    public int NextFrameIndex => frameIndex + 1;
    
    public bool IsFinished => !IsPlaying || frameIndex + 1 >= Frames.Length;
    
    public bool IsPlaying { get; private set; }
    public bool IsPaused { get; set; }
    
    public string CurrentFrame => Frames[FrameIndex];
    public string NextFrame => Frames[NextFrameIndex];
    
    public TimeSpan Duration => Frames != null ? TimeSpan.FromSeconds(FrameCount / FramesPerSecond) : TimeSpan.Zero;
    public TimeSpan Remaining => Frames != null && IsPlaying ? TimeSpan.FromSeconds((FrameCount - (FrameIndex + 1)) / FramesPerSecond) : TimeSpan.Zero;
    
    public Func<ExPlayer, bool> Predicate { get; set; }
    
    public override bool ShouldWrap => false;
    public override bool ShouldCache => false;
    public override bool ShouldParse => false;

    public override bool OverridesOthers => true;

    public new HintAlign Alignment { get; set; } = HintElement.DefaultHintAlign;
    public new float VerticalOffset { get; set; } = HintElement.DefaultVerticalOffset;
    public new int PixelSpacing { get; set; } = HintElement.DefaultPixelLineSpacing;

    public override HintAlign GetAlignment(ExPlayer player) => Alignment;
    public override float GetVerticalOffset(ExPlayer player) => VerticalOffset;
    public override int GetPixelSpacing(ExPlayer player) => PixelSpacing;

    public void Play(string[] frames, int? frameRate = null)
    {
        if (frames is null)
            throw new ArgumentNullException(nameof(frames));
        
        if (frames.Length == 0)
            throw new ArgumentException("Frames cannot be empty", nameof(frames));
        
        OnFinished();
        
        Frames = frames;
        
        if (frameRate.HasValue && frameRate.Value > 0)
            FramesPerSecond = frameRate.Value;
        else
            FramesPerSecond = 24;

        IsPaused = false;
        IsPlaying = true;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (!IsPlaying)
            return;
        
        currentFrameDelay -= Time.deltaTime;

        if (currentFrameDelay > 0f)
            return;

        currentFrameDelay = targetFrameDelay + CustomFrameDelay;

        Builder.AppendLine(Frames[frameIndex]);

        if (!IsPaused)
        {
            frameIndex++;

            if (frameIndex >= Frames.Length)
                OnFinished();
        }
    }

    public override bool OnDraw()
    {
        if (Predicate != null && !Predicate(Player))
            return false;

        return true;
    }

    private void OnFinished()
    {
        IsPlaying = false;
        
        frameIndex = 0;
        currentFrameDelay = 0f;
    }

    public override string ToString()
        => $"PersonalImageElement (Player={Player?.Name ?? "null"}; Frames={Frames?.Length ?? -1}; IsPlaying={IsPlaying}; TargetDelay={targetFrameDelay}; CurrentDelay={currentFrameDelay}; Fps={FramesPerSecond}; CurrentIndex={frameIndex}; Duration={Duration}; Remaining={Remaining}; IsPaused={IsPaused})";
}