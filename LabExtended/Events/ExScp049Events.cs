using LabExtended.Events.Scp049;
using LabExtended.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// SCP-049 related events.
/// </summary>
public static class ExScp049Events
{
    /// <inheritdoc cref="Scp049CancellingResurrectionEventArgs"/>
    public static event Action<Scp049CancellingResurrectionEventArgs>? CancellingResurrection;

    /// <inheritdoc cref="Scp049CancelledResurrectionEventArgs"/>
    public static event Action<Scp049CancelledResurrectionEventArgs>? CancelledResurrection;

    /// <summary>
    /// Invokes the <see cref="CancellingResurrection"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnCancellingResurrection(Scp049CancellingResurrectionEventArgs args)
        => CancellingResurrection.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="CancelledResurrection"/> event.
    /// </summary>
    public static void OnCancelledResurrection(Scp049CancelledResurrectionEventArgs args)
        => CancelledResurrection.InvokeEvent(args);
}