using LabExtended.Events.Scp0492;
using LabExtended.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// Events related to SCP-049-2.
/// </summary>
public static class ExScp0492Events
{
    /// <inheritdoc cref="Scp0492ConsumingRagdollEventArgs"/>
    public static event Action<Scp0492ConsumingRagdollEventArgs>? ConsumingRagdoll;

    /// <summary>
    /// Invokes the <see cref="ConsumingRagdoll"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnConsumingRagdoll(Scp0492ConsumingRagdollEventArgs args)
        => ConsumingRagdoll.InvokeBooleanEvent(args);
}