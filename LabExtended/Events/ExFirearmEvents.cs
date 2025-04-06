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

    /// <summary>
    /// Invokes the <see cref="RayCast"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnRayCast(FirearmRayCastEventArgs args)
        => RayCast.InvokeBooleanEvent(args);
}