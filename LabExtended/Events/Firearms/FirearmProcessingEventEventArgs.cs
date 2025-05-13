using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;

using LabExtended.API;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace LabExtended.Events.Firearms;

/// <summary>
/// Gets called when a firearm starts processing an event.
/// </summary>
public class FirearmProcessingEventEventArgs : BooleanEventArgs
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
    /// Gets the name of the target method.
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// Creates a new <see cref="FirearmProcessingEventEventArgs"/> event.
    /// </summary>
    /// <param name="player">The owner of the firearm</param>
    /// <param name="firearm">The target firearm</param>
    /// <param name="firearmEvent">The firearm event</param>
    /// <param name="details">Event details</param>
    public FirearmProcessingEventEventArgs(ExPlayer player, Firearm firearm, FirearmEvent firearmEvent,
        EventInvocationDetails details)
    {
        Player = player;
        Firearm = firearm;
        Event = firearmEvent;
        Details = details;

        Method = firearmEvent.Action.GetPersistentMethodName(0);
        Module = firearmEvent.Action.GetPersistentTarget(0) as ModuleBase;
    }
}