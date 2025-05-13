using LabExtended.Events.Firearms;
using LabExtended.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// Contains events specific to firearms.
/// </summary>
public static class ExFirearmEvents
{
    /// <inheritdoc cref="FirearmRayCastEventArgs"/>
    public static event Action<FirearmRayCastEventArgs>? RayCast;
    
    /// <inheritdoc cref="FirearmProcessingEventEventArgs"/>
    public static event Action<FirearmProcessingEventEventArgs>? ProcessingEvent; 
    
    /// <inheritdoc cref="FirearmProcessedEventEventArgs"/>
    public static event Action<FirearmProcessedEventEventArgs>? ProcessedEvent;

    /// <summary>
    /// Invokes the <see cref="RayCast"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnRayCast(FirearmRayCastEventArgs args)
        => RayCast.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="ProcessingEvent"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnProcessingEvent(FirearmProcessingEventEventArgs args)
        => ProcessingEvent.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="ProcessedEvent"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnProcessedEvent(FirearmProcessedEventEventArgs args)
        => ProcessedEvent.InvokeEvent(args);
}