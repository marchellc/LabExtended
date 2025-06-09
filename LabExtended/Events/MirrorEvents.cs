using LabExtended.Events.Mirror;
using LabExtended.Extensions;

using Mirror;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// Contains events relating to the Mirror networking library.
/// </summary>
public static class MirrorEvents
{
    /// <summary>
    /// Called when a <see cref="NetworkBehaviour"/> gets destroyed.
    /// </summary>
    public static event Action<NetworkIdentity>? Destroying;

    /// <summary>
    /// Called when a new <see cref="NetworkBehaviour"/> spawns.
    /// </summary>
    public static event Action<NetworkIdentity>? Spawning;
    
    /// <inheritdoc cref="MirrorAddingObserverEventArgs"/>
    public static event Action<MirrorAddingObserverEventArgs>? AddingObserver; 

    /// <summary>
    /// Invokes the <see cref="Destroying"/> event.
    /// </summary>
    /// <param name="identity">The identity that is being destroyed.</param>
    public static void OnDestroying(NetworkIdentity identity)
        => Destroying.InvokeSafe(identity);

    /// <summary>
    /// Invokes the <see cref="Spawning"/> event.
    /// </summary>
    /// <param name="identity">The identity that is being spawned.</param>
    public static void OnSpawning(NetworkIdentity identity)
        => Spawning.InvokeSafe(identity);
    
    /// <summary>
    /// Invokes the <see cref="AddingObserver"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnAddingObserver(MirrorAddingObserverEventArgs args)
        => AddingObserver.InvokeBooleanEvent(args);
}