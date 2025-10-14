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
    /// Called before a <see cref="NetworkIdentity"/> gets destroyed.
    /// </summary>
    public static event Action<MirrorDestroyingIdentityEventArgs>? Destroying;

    /// <summary>
    /// Called after a <see cref="NetworkIdentity"/> gets destroyed.
    /// </summary>
    public static event Action<MirrorDestroyedIdentityEventArgs>? Destroyed;

    /// <summary>
    /// Called before a new <see cref="NetworkIdentity"/> spawns.
    /// </summary>
    public static event Action<MirrorIdentityBooleanEventArgs>? Spawning;

    /// <summary>
    /// Called after a new <see cref="NetworkIdentity"/> spawns.
    /// </summary>
    public static event Action<MirrorIdentityEventArgs>? Spawned;
    
    /// <summary>
    /// Gets called before a new observer is added to an observing list.
    /// </summary>
    public static event Action<MirrorAddingObserverEventArgs>? AddingObserver;

    /// <summary>
    /// Gets called after a new observer is added to an observing list.
    /// </summary>
    public static event Action<MirrorAddedObserverEventArgs>? AddedObserver;

    /// <summary>
    /// Gets called before an observer is removed from an observing list.
    /// </summary>
    public static event Action<MirrorRemovingObserverEventArgs>? RemovingObserver;

    /// <summary>
    /// Gets called after an observer is removed from an observing list.
    /// </summary>
    public static event Action<MirrorRemovedObserverEventArgs>? RemovedObserver;

    /// <summary>
    /// Gets called before an RPC is sent to the specified targets.
    /// </summary>
    public static event Action<MirrorSendingRpcEventArgs>? SendingRpc;

    /// <summary>
    /// Gets called after an RPC is sent.
    /// </summary>
    public static event Action<MirrorSentRpcEventArgs>? SentRpc;

    /// <summary>
    /// Gets called before serialized data of a behaviour is written to the identity writer.
    /// </summary>
    public static event Action<MirrorSerializingBehaviourEventArgs>? SerializingBehaviour;

    /// <summary>
    /// Gets called after serialized data of a behaviour is written to the identity writer.
    /// </summary>
    public static event Action<MirrorSerializedBehaviourEventArgs>? SerializedBehaviour;

    /// <summary>
    /// Gets called before a behaviour's sync variable value is updated.
    /// </summary>
    public static event Action<MirrorSettingSyncVarEventArgs>? SettingSyncVar;

    /// <summary>
    /// Gets called after a behaviour's sync variable value is updated.
    /// </summary>
    public static event Action<MirrorSetSyncVarEventArgs>? SetSyncVar;

    /// <summary>
    /// Invokes the <see cref="Destroying"/> event.
    /// </summary>
    public static bool OnDestroying(MirrorDestroyingIdentityEventArgs args)
        => Destroying.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="Destroyed"/> event.
    /// </summary>
    public static void OnDestroyed(MirrorDestroyedIdentityEventArgs args)
        => Destroyed.InvokeEvent(args);

    /// <summary>
    /// Invokes the <see cref="Spawning"/> event.
    /// </summary>
    public static bool OnSpawning(MirrorIdentityBooleanEventArgs args)
        => Spawning.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="Spawned"/> event.
    /// </summary>
    public static void OnSpawned(MirrorIdentityEventArgs args)
        => Spawned.InvokeEvent(args);

    /// <summary>
    /// Invokes the <see cref="AddingObserver"/> event.
    /// </summary>
    public static bool OnAddingObserver(MirrorAddingObserverEventArgs args)
        => AddingObserver.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="AddedObserver"/> event.
    /// </summary>
    public static void OnAddedObserver(MirrorAddedObserverEventArgs args)
        => AddedObserver.InvokeEvent(args);

    /// <summary>
    /// Invokes the <see cref="RemovingObserver"/> event.
    /// </summary>
    public static bool OnRemovingObserver(MirrorRemovingObserverEventArgs args)
        => RemovingObserver.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="RemovedObserver"/> event.
    /// </summary>
    public static void OnRemovedObserver(MirrorRemovedObserverEventArgs args)
        => RemovedObserver.InvokeEvent(args);

    /// <summary>
    /// Invokes the <see cref="SerializingBehaviour"/> event.
    /// </summary>
    public static bool OnSerializingBehaviour(MirrorSerializingBehaviourEventArgs args)
        => SerializingBehaviour.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="SerializedBehaviour"/> event.
    /// </summary>
    public static void OnSerializedBehaviour(MirrorSerializedBehaviourEventArgs args)
        => SerializedBehaviour.InvokeEvent(args);

    /// <summary>
    /// Invokes the <see cref="SettingSyncVar"/> event.
    /// </summary>
    public static bool OnSettingSyncVar(MirrorSettingSyncVarEventArgs args)
        => SettingSyncVar.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="SetSyncVar"/> event.
    /// </summary>
    public static void OnSetSyncVar(MirrorSetSyncVarEventArgs args)
        => SetSyncVar.InvokeEvent(args);

    /// <summary>
    /// Invokes the <see cref="SendingRpc"/> event.
    /// </summary>
    public static bool OnSendingRpc(MirrorSendingRpcEventArgs args)
        => SendingRpc.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="SentRpc"/> event.
    /// </summary>
    public static void OnSentRpc(MirrorSentRpcEventArgs args)
        => SentRpc.InvokeEvent(args);
}