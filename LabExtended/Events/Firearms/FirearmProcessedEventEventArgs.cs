using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;

using LabExtended.API;

namespace LabExtended.Events.Firearms;

/// <summary>
/// Gets called when a firearm finishes processing an event.
/// </summary>
public class FirearmProcessedEventEventArgs : EventArgs
{
    /// <summary>
    /// Gets the owner of the firearm which is processing the event.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the firearm which is processing the event.
    /// </summary>
    public Firearm Firearm { get; }
    
    /// <summary>
    /// Gets the event which is being processed.
    /// </summary>
    public FirearmEvent Event { get; }
    
    /// <summary>
    /// Gets the event's invocation data.
    /// </summary>
    public EventInvocationDetails Details { get; }
    
    /// <summary>
    /// Gets the target module.
    /// </summary>
    public ModuleBase Module { get; }
    
    /// <summary>
    /// Gets the target method.
    /// </summary>
    public string Method { get; }
    
    /// <summary>
    /// Gets the exception that occured during the event's invocation.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Creates a new <see cref="FirearmProcessedEventEventArgs"/> event.
    /// </summary>
    /// <param name="player">The owner of the firearm</param>
    /// <param name="firearm">The target firearm</param>
    /// <param name="firearmEvent">The firearm event</param>
    /// <param name="details">Event details</param>
    /// <param name="module">The target module</param>
    /// <param name="method">The target method name</param>
    /// <param name="exception">The occured exception</param>
    public FirearmProcessedEventEventArgs(ExPlayer player, Firearm firearm, FirearmEvent firearmEvent,
        EventInvocationDetails details, ModuleBase module, string method, Exception? exception)
    {
        Player = player;
        Firearm = firearm;
        Event = firearmEvent;
        Details = details;
        Module = module;
        Method = method;
        Exception = exception;
    }
}