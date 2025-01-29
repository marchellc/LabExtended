using LabExtended.Utilities;

using UnityEngine;

namespace LabExtended.API.Toys.Primitives;

public class PrimitiveDynamicImage : IDisposable
{
    private int prevFrameIndex = -2;
    
    private int frameIndex = -2;
    private int framesPerSecond = 60;

    private float targetFrameDelay = 1f / 60;
    private float currentFrameDelay = 0f;
    
    public List<UnityEngine.Color?[,]> Frames { get; private set; }
    
    public PrimitiveImageToy Toy { get; private set; }

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
    public int FrameCount => Frames.Count;
    
    public int NextFrameIndex => frameIndex + 1;
    
    public bool IsFinished => !IsPlaying || frameIndex + 1 >= Frames.Count;
    public bool IsPlaying => Toy != null && Frames != null && frameIndex != -2;
    
    public bool IsPaused { get; set; }
    
    public bool ClearOnFinish { get; set; }
    public bool DestroyOnFinish { get; set; }
    
    public UnityEngine.Color?[,] CurrentFrame => Frames[FrameIndex];
    public UnityEngine.Color?[,] NextFrame => Frames[NextFrameIndex];
    
    public TimeSpan Duration => Frames != null ? TimeSpan.FromSeconds(FrameCount / FramesPerSecond) : TimeSpan.Zero;
    public TimeSpan Remaining => Frames != null && IsPlaying ? TimeSpan.FromSeconds((FrameCount - (FrameIndex + 1)) / FramesPerSecond) : TimeSpan.Zero;

    public PrimitiveDynamicImage(PrimitiveImageToy toy)
    {
        if (toy is null)
            throw new ArgumentNullException(nameof(toy));
        
        Toy = toy;

        PlayerUpdateHelper.OnUpdate += OnUpdate;
    }
    
    public void Play(List<UnityEngine.Color?[,]> frames, int? frameRate = null)
    {
        if (frames is null)
            throw new ArgumentNullException(nameof(frames));
        
        if (frames.Count == 0)
            throw new ArgumentException("Frames cannot be empty", nameof(frames));
        
        Frames = frames;
        
        if (frameRate.HasValue && frameRate.Value > 0)
            FramesPerSecond = frameRate.Value;

        frameIndex = -1;
    }

    public void Dispose()
    {
        PlayerUpdateHelper.OnUpdate -= OnUpdate;
        
        if (Toy != null)
        {
            Toy.Dispose();
            Toy = null;
        }
    }
    
    private void OnUpdate()
    {
        if (!IsPlaying)
            return;
        
        currentFrameDelay -= Time.deltaTime;

        if (currentFrameDelay > 0f)
            return;

        currentFrameDelay = targetFrameDelay;

        if (!IsPaused)
        {
            frameIndex++;

            if (frameIndex >= Frames.Count)
            {
                OnFinished();
                return;
            }
        }

        if (prevFrameIndex != frameIndex)
        {
            prevFrameIndex = frameIndex;
            Toy.SetFrame(Frames[frameIndex]);
        }
    }
    
    private void OnFinished()
    {
        frameIndex = prevFrameIndex = -2;
        currentFrameDelay = 0f;

        if (DestroyOnFinish)
        {
            Dispose();
            return;
        }

        if (ClearOnFinish && Toy != null)
            Toy.Clear();
    }
    
    public override string ToString()
        => $"PrimitiveDynamicImage (Height={Toy?.Height ?? -1}; Width={Toy?.Width ?? -1}; Frames={Frames?.Count} ?? -1; TargetDelay={targetFrameDelay}; CurrentDelay={currentFrameDelay}; Fps={FramesPerSecond}; CurrentIndex={frameIndex}; Duration={Duration}; Remaining={Remaining}; IsPaused={IsPaused})";
}