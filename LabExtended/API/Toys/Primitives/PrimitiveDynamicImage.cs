using System.Diagnostics;

using LabExtended.Utilities.Update;

namespace LabExtended.API.Toys.Primitives;

public class PrimitiveDynamicImage : IDisposable
{
    private int prevFrameIndex = -1;
    
    private int frameIndex = 0;
    private int framesPerSecond = 24;

    private float targetFrameDelay = 1000f / 24;
    
    public List<UnityEngine.Color?[,]> Frames { get; private set; }
    
    public PrimitiveImageToy Toy { get; private set; }
    public Stopwatch Watch { get; private set; }

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
    public int FrameCount => Frames.Count;
    
    public bool IsPlaying => Watch.IsRunning;
    
    public bool IsPaused { get; set; }
    public bool IsLooping { get; set; }
    
    public bool ClearOnFinish { get; set; }
    public bool DestroyOnFinish { get; set; }
    
    public TimeSpan Duration => Frames != null ? TimeSpan.FromSeconds(FrameCount / FramesPerSecond) : TimeSpan.Zero;
    public TimeSpan Remaining => Frames != null && IsPlaying ? TimeSpan.FromSeconds((FrameCount - (FrameIndex + 1)) / FramesPerSecond) : TimeSpan.Zero;

    public PrimitiveDynamicImage(PrimitiveImageToy toy)
    {
        if (toy is null)
            throw new ArgumentNullException(nameof(toy));
        
        Toy = toy;
        Watch = new Stopwatch();

        PlayerUpdateHelper.OnUpdate += OnUpdate;
    }
    
    public void Play(List<UnityEngine.Color?[,]> frames, int? frameRate = null)
    {
        if (frames is null)
            throw new ArgumentNullException(nameof(frames));
        
        if (frames.Count == 0)
            throw new ArgumentException("Frames cannot be empty", nameof(frames));
        
        Reset();
        
        if (frameRate.HasValue && frameRate.Value > 0)
            FramesPerSecond = frameRate.Value;
        else
            FramesPerSecond = 24;

        Frames = frames;
        
        Watch.Reset();
        Watch.Start();
    }

    public void Reset()
    {
        if (Watch.IsRunning)
            Watch.Reset();
        
        Frames?.Clear();
        Frames = null;

        frameIndex = 0;
        prevFrameIndex = -1;
    }

    public void Dispose()
    {
        Reset();
        
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

        if (Watch.ElapsedMilliseconds < targetFrameDelay)
            return;

        Watch.Restart();

        if (frameIndex >= Frames.Count)
        {
            if (IsLooping)
            {
                frameIndex = 0;
            }
            else
            {
                OnFinished();
                return;
            }
        }

        if (prevFrameIndex != frameIndex)
        {
            prevFrameIndex = frameIndex;
            Toy.SetFrame(Frames[IsPaused ? frameIndex : frameIndex++]);
        }
    }

    private void OnFinished()
    {
        Reset();

        if (DestroyOnFinish)
        {
            Dispose();
            return;
        }

        if (ClearOnFinish && Toy != null)
            Toy.Clear();
    }
    
    public override string ToString()
        => $"PrimitiveDynamicImage (Height={Toy?.Height ?? -1}; Width={Toy?.Width ?? -1}; Frames={Frames?.Count ?? -1}; TargetDelay={targetFrameDelay}; CurrentDelay={(Watch.IsRunning ? Watch.ElapsedMilliseconds : -1)}; Fps={FramesPerSecond}; CurrentIndex={frameIndex}; Duration={Duration}; Remaining={Remaining}; IsPaused={IsPaused})";
}