using System.Diagnostics;

using LabExtended.API.Images;
using LabExtended.Core;
using LabExtended.Utilities.Update;

namespace LabExtended.Images.Playback;

/// <summary>
/// The base class for a playback.
/// </summary>
public class PlaybackBase : IDisposable
{
    private int frameIndex = 0;
    private Stopwatch transitionTime = new();
    
    /// <summary>
    /// Gets the playback display.
    /// </summary>
    public IPlaybackDisplay Display { get; }
    
    /// <summary>
    /// Gets the currently played image.
    /// </summary>
    public ImageFile? CurrentFile { get; private set; }

    /// <summary>
    /// Gets the state of the playback.
    /// </summary>
    public PlaybackState State { get; private set; } = PlaybackState.Idle;
    
    /// <summary>
    /// Gets or sets the playback options.
    /// </summary>
    public PlaybackFlags Options { get; set; } = PlaybackFlags.None;

    /// <summary>
    /// Whether or not the loop option has been enabled.
    /// </summary>
    public bool IsLooping => (Options & PlaybackFlags.Loop) == PlaybackFlags.Loop;
    
    /// <summary>
    /// Whether or not the pause option has been enabled.
    /// </summary>
    public bool IsPaused => (Options & PlaybackFlags.Pause) == PlaybackFlags.Pause;

    /// <summary>
    /// Gets called when a playback is finished (includes force-stopped playback).
    /// </summary>
    public event Action? Finished;

    /// <summary>
    /// Gets called when a playback is started.
    /// </summary>
    public event Action? Started;

    /// <summary>
    /// Gets called when a new frame is displayed.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Initializes the playback instance.
    /// </summary>
    public PlaybackBase(IPlaybackDisplay display)
    {
        if (display is null)
            throw new ArgumentNullException(nameof(display));
        
        Display = display;
        
        PlayerUpdateHelper.OnUpdate += Update;
    }

    /// <summary>
    /// Plays the specified image file.
    /// </summary>
    /// <param name="image">The image file to play.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Play(ImageFile image)
    {
        if (image is null)
            throw new ArgumentNullException(nameof(image));
        
        if (State is PlaybackState.Disposed)
            throw new ObjectDisposedException(GetType().Name);
        
        Reset();
        
        CurrentFile = image;
        
        transitionTime.Restart();
        
        Started?.Invoke();
    }

    /// <summary>
    /// Stops the playback.
    /// </summary>
    public void Stop()
    {
        if (State is PlaybackState.Disposed)
            throw new ObjectDisposedException(GetType().Name);

        if (CurrentFile != null
            && State != PlaybackState.Idle)
        {
            Reset();
            
            Display.SetFrame(null);

            Finished?.Invoke();
        }
    }

    /// <summary>
    /// Enables an option.
    /// </summary>
    /// <param name="option">The option flag to enable.</param>
    public void EnableOption(PlaybackFlags option)
    {
        if (State is PlaybackState.Disposed)
            throw new ObjectDisposedException(GetType().Name);
        
        if ((Options & option) == option)
            return;
        
        Options |= option;
    }

    /// <summary>
    /// Disables an option.
    /// </summary>
    /// <param name="option">The option flag to disable.</param>
    public void DisableOption(PlaybackFlags option)
    {
        if (State is PlaybackState.Disposed)
            throw new ObjectDisposedException(GetType().Name);
        
        if ((Options & option) == option)
        {
            Options &= ~option;
        }
    }

    /// <summary>
    /// Toggles a playback option.
    /// </summary>
    /// <param name="option">The option flag to toggle.</param>
    public void ToggleOption(PlaybackFlags option)
    {
        if (State is PlaybackState.Disposed)
            throw new ObjectDisposedException(GetType().Name);
        
        if ((Options & option) == option)
        {
            Options &= ~option;
        }
        else
        {
            Options |= option;
        }
    }
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public virtual void Dispose()
    {
        if (State != PlaybackState.Disposed)
        {
            PlayerUpdateHelper.OnUpdate -= Update;

            Stop();
        }
    }

    internal virtual void Reset()
    {
        if (State is PlaybackState.Disposed)
            throw new ObjectDisposedException(GetType().Name);

        CurrentFile = null;
        
        transitionTime.Stop();
        transitionTime.Reset();

        frameIndex = 0;
        
        State = PlaybackState.Idle;
    }

    private void Update()
    {
        if (State is PlaybackState.Disposed)
            throw new ObjectDisposedException(GetType().Name);
        
        if (CurrentFile != null)
        {
            if (frameIndex != 0 && IsPaused)
                return;

            if (frameIndex != 0 && transitionTime.ElapsedMilliseconds < CurrentFile.FrameDuration)
            {
                State = PlaybackState.Waiting;
                return;
            }

            State = PlaybackState.Playing;
            
            transitionTime.Restart();
            
            Display.SetFrame(CurrentFile.Frames[frameIndex++]);
            
            Changed?.Invoke();
            
            if (frameIndex >= CurrentFile.Frames.Count)
                Finish();
        }
    }

    private void Finish()
    {
        Finished?.Invoke();

        var file = CurrentFile;
        
        Reset();
        
        Display.SetFrame(null);
        
        State = PlaybackState.Idle;
        
        if (file != null && IsLooping)
            Play(file);
    }
}