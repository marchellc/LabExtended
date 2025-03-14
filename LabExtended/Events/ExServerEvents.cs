using LabExtended.Attributes;
using LabExtended.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// Server-related events.
/// </summary>
public static class ExServerEvents
{
    // Used to prevent multiple Quitting event calls.
    private static bool shutdownCalled = false;
    
    /// <summary>
    /// Called when the server process quits.
    /// </summary>
    public static event Action? Quitting;
    
    /// <summary>
    /// Called when a new message is printed into the server console.
    /// </summary>
    public static event Action<string>? Logging; 

    /// <summary>
    /// Invokes the Quitting event.
    /// </summary>
    public static void OnQuitting()
        => Quitting.InvokeSafe();

    /// <summary>
    /// Invokes the Logging event.
    /// </summary>
    /// <param name="log">The message that is being logged.</param>
    public static void OnLogging(string log)
        => Logging.InvokeSafe(log);

    private static void OnQuit()
    {
        Shutdown.OnQuit -= OnQuit;

        if (shutdownCalled)
            return;
        
        shutdownCalled = true;
        
        OnQuitting();
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        Shutdown.OnQuit += OnQuit;
    }
}