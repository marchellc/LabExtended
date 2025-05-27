using LabExtended.Events.Scp939;
using LabExtended.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// SCP-939 related events.
/// </summary>
public static class ExScp939Events
{
    /// <inheritdoc cref="Scp939PlacingMimicryEventArgs"/>
    public static event Action<Scp939PlacingMimicryEventArgs>? PlacingMimicry; 
    
    /// <inheritdoc cref="Scp939RemovingMimicryEventArgs"/>
    public static event Action<Scp939RemovingMimicryEventArgs>? RemovingMimicry; 

    /// <summary>
    /// Invokes the <see cref="PlacingMimicry"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnPlacingMimicry(Scp939PlacingMimicryEventArgs args)
        => PlacingMimicry.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="RemovingMimicry"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnRemovingMimicry(Scp939RemovingMimicryEventArgs args)
        => RemovingMimicry.InvokeBooleanEvent(args);
}