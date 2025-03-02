using LabExtended.Attributes;
using LabExtended.Extensions;

namespace LabExtended.Events;

public static class ServerEvents
{
    private static bool shutdownCalled = false;
    
    public static event Action Quitting;

    public static void OnQuitting()
        => Quitting.InvokeSafe();

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