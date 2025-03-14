using LabExtended.Events.Scp079;
using LabExtended.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// SCP-079 related events.
/// </summary>
public static class ExScp079Events
{
    /// <inheritdoc cref="Scp079RecontainingEventArgs"/>.
    public static event Action<Scp079RecontainingEventArgs>? Recontaining;
    
    /// <inheritdoc cref="Scp079SpawningPingEventArgs"/>
    public static event Action<Scp079SpawningPingEventArgs>? SpawningPing;

    /// <inheritdoc cref="Scp079SpawnedPingEventArgs"/>
    public static event Action<Scp079SpawnedPingEventArgs>? SpawnedPing;

    /// <summary>
    /// Invokes the <see cref="Recontaining"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnRecontaining(Scp079RecontainingEventArgs args)
        => Recontaining.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SpawningPing"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnSpawningPing(Scp079SpawningPingEventArgs args)
        => SpawningPing.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SpawnedPing"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSpawnedPing(Scp079SpawnedPingEventArgs args)
        => SpawnedPing.InvokeEvent(args);
}