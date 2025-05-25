using System.Diagnostics;
using System.Text;
using Hints;
using LabExtended.Core;
using Mirror;

namespace LabExtended.API.Hints;

/// <summary>
/// Represents the current hint state.
/// </summary>
public class HintState
{
    /// <summary>
    /// Whether or not elements have been removed in this frame.
    /// </summary>
    public bool FrameRemoved = false;

    /// <summary>
    /// The update delay.
    /// </summary>
    public float Interval = 0f;
    
    /// <summary>
    /// The lowest desired update interval.
    /// </summary>
    public float LowestInterval = 0f;
    
    /// <summary>
    /// The frame counter.
    /// </summary>
    public ulong FrameCounter = 0;

    /// <summary>
    /// Whether or not to force the next frame, ignoring the set frame delay.
    /// </summary>
    public bool ForceSend = false;

    /// <summary>
    /// Whether or not any hint data was appended.
    /// </summary>
    public bool AnyAppended = false;
    
    /// <summary>
    /// Whether or not any element overrides parsing.
    /// </summary>
    public bool AnyOverrideParse = false;

    /// <summary>
    /// Whether or not any element overrides others.
    /// </summary>
    public bool AnyOverrideAll = false;

    /// <summary>
    /// The hint overlay builder.
    /// </summary>
    public StringBuilder Builder = new();

    /// <summary>
    /// The hint overlay writer.
    /// </summary>
    public NetworkWriter Writer = new();

    /// <summary>
    /// Frame delay stopwatch.
    /// </summary>
    public Stopwatch Watch = new();
    
    /// <summary>
    /// List of hint parameters.
    /// </summary>
    public List<HintParameter> Parameters = new();

    /// <summary>
    /// List of elements to remove next frame.
    /// </summary>
    public List<HintElement> Remove = new();

    /// <summary>
    /// Whether or not the hint should be updated next frame.
    /// </summary>
    public bool ShouldUpdate
    {
        get
        {
            if (ExPlayer.Count < 1)
                return false;

            if (ForceSend)
                return true;

            if (Interval > 0f && Watch.ElapsedMilliseconds < Interval)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Resets player-only properties.
    /// </summary>
    public void ResetPlayer()
    {
        AnyAppended = false;
        
        AnyOverrideAll = false;
        AnyOverrideParse = false;
        
        Writer.Reset();
        Builder.Clear();
        Parameters.Clear();
    }

    /// <summary>
    /// Resets frame-only properties.
    /// </summary>
    public void ResetFrame()
    {
        FrameRemoved = false;
        FrameCounter++;
        
        Remove.Clear();
        Watch.Restart();

        Interval = ApiLoader.ApiConfig.HintSection.UpdateInterval;
        LowestInterval = 0f;
    }
}