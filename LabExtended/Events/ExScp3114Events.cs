using LabExtended.Events.Scp3114;
using LabExtended.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// Events related to SCP-3114.
/// </summary>
public static class ExScp3114Events
{
    /// <inheritdoc cref="Scp3114StranglingEventArgs"/>
    public static event Action<Scp3114StranglingEventArgs>? Strangling;

    /// <summary>
    /// Invokes the <see cref="Strangling"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnStrangling(Scp3114StranglingEventArgs args)
        => Strangling.InvokeBooleanEvent(args);
}