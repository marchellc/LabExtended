using LabExtended.Core;
using LabExtended.Extensions;

using LabExtended.Events.Mirror;

using Mirror;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// Contains events relating to the Mirror networking library.
/// </summary>
public static class MirrorEvents
{
    /// <summary>
    /// Represents the method that handles serialization events for a NetworkBehaviour, providing access to the
    /// behaviour and the serialized data writer.
    /// </summary>
    /// <param name="behaviour">The NetworkBehaviour instance being serialized. Cannot be null.</param>
    /// <param name="serializedData">The NetworkWriter used to write the serialized data for the behaviour. The handler should write any custom data
    /// to this writer as needed.</param>
    public delegate void BehaviourSerializingEventHandler(NetworkBehaviour behaviour, NetworkWriter serializedData, ref bool serializeBehaviour);

    /// <summary>
    /// Represents the method that handles the event raised when a NetworkBehaviour is serialized.
    /// </summary>
    /// <param name="behaviour">The NetworkBehaviour instance that is being serialized.</param>
    /// <param name="serializedData">A NetworkWriter containing the serialized data of the behaviour.</param>
    public delegate void BehaviourSerializedEventHandler(NetworkBehaviour behaviour, NetworkWriter serializedData);

    /// <summary>
    /// Called before a <see cref="NetworkIdentity"/> gets destroyed.
    /// </summary>
    public static event Action<NetworkIdentity, NetworkServer.DestroyMode>? Destroying;

    /// <summary>
    /// Called after a <see cref="NetworkIdentity"/> gets destroyed.
    /// </summary>
    public static event Action<NetworkIdentity, NetworkServer.DestroyMode>? Destroyed;

    /// <summary>
    /// Called before a new <see cref="NetworkIdentity"/> spawns.
    /// </summary>
    public static event Action<NetworkIdentity>? Spawning;

    /// <summary>
    /// Called after a new <see cref="NetworkIdentity"/> spawns.
    /// </summary>
    public static event Action<NetworkIdentity>? Spawned;
    
    /// <inheritdoc cref="MirrorAddingObserverEventArgs"/>
    public static event Action<MirrorAddingObserverEventArgs>? AddingObserver;

    /// <inheritdoc cref="MirrorAddedObserverEventArgs"/>
    public static event Action<MirrorAddedObserverEventArgs>? AddedObserver;

    /// <summary>
    /// Gets called before serialized data of a behaviour is written to the identity writer.
    /// </summary>
    public static event BehaviourSerializingEventHandler? BehaviourSerializing;

    /// <summary>
    /// Gets called after serialized data of a behaviour is written to the identity writer.
    /// </summary>
    public static event BehaviourSerializedEventHandler? BehaviourSerialized;

    /// <summary>
    /// Invokes the <see cref="Destroying"/> event.
    /// </summary>
    /// <param name="identity">The identity that is being destroyed.</param>
    public static void OnDestroying(NetworkIdentity identity, NetworkServer.DestroyMode mode)
        => Destroying.InvokeSafe(identity, mode);

    /// <summary>
    /// Invokes the <see cref="Destroyed"/> event.
    /// </summary>
    /// <param name="identity">The identity that is being destroyed.</param>
    public static void OnDestroyed(NetworkIdentity identity, NetworkServer.DestroyMode mode)
        => Destroyed.InvokeSafe(identity, mode);

    /// <summary>
    /// Invokes the <see cref="Spawning"/> event.
    /// </summary>
    /// <param name="identity">The identity that is being spawned.</param>
    public static void OnSpawning(NetworkIdentity identity)
        => Spawning.InvokeSafe(identity);

    /// <summary>
    /// Invokes the <see cref="Spawned"/> event.
    /// </summary>
    /// <param name="identity">The identity that is being spawned.</param>
    public static void OnSpawned(NetworkIdentity identity)
        => Spawning.InvokeSafe(identity);

    /// <summary>
    /// Invokes the <see cref="AddingObserver"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnAddingObserver(MirrorAddingObserverEventArgs args)
        => AddingObserver.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="AddedObserver"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static void OnAddedObserver(MirrorAddedObserverEventArgs args)
        => AddedObserver.InvokeEvent(args);

    /// <summary>
    /// Invokes the <see cref="BehaviourSerializing"/> event.
    /// </summary>
    /// <param name="behaviour">The behaviour being serialized.</param>
    /// <param name="writer">The target network writer.</param>
    public static bool OnBehaviourSerializing(NetworkBehaviour behaviour, NetworkWriter writer)
    {
        try
        {
            var serializeBehaviour = true;

            BehaviourSerializing?.Invoke(behaviour, writer, ref serializeBehaviour);
            return serializeBehaviour;
        }
        catch (Exception ex)
        {
            ApiLog.Error("OnBehaviourSerializing", ex);
            return true;
        }
    }

    /// <summary>
    /// Invokes the <see cref="BehaviourSerialized"/> event.
    /// </summary>
    /// <param name="behaviour">The behaviour being serialized.</param>
    /// <param name="writer">The target network writer.</param>
    public static void OnBehaviourSerialized(NetworkBehaviour behaviour, NetworkWriter writer)
    {
        try
        {
            BehaviourSerialized?.Invoke(behaviour, writer);
        }
        catch (Exception ex)
        {
            ApiLog.Error("OnBehaviourSerializing", ex);
        }
    }
}