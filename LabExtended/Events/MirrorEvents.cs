using LabExtended.API;

using LabExtended.Core;
using LabExtended.Extensions;

using Mirror;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// Contains events relating to the Mirror networking library.
/// </summary>
public static class MirrorEvents
{
    #region Delegates
    /// <summary>
    /// Represents the method that handles serialization events for a NetworkBehaviour, providing access to the
    /// behaviour and the serialized data writer.
    /// </summary>
    /// <param name="behaviour">The NetworkBehaviour instance being serialized. Cannot be null.</param>
    /// <param name="serializedData">The NetworkWriter used to write the serialized data for the behaviour. The handler should write any custom data
    /// to this writer as needed.</param>
    /// <param name="serializeBehaviour">Whether or not the behaviour should serialize it's properties.</param>
    public delegate void BehaviourSerializingEventHandler(NetworkBehaviour behaviour, NetworkWriter serializedData, ref bool serializeBehaviour);

    /// <summary>
    /// Represents the method that handles the event raised when a NetworkBehaviour is serialized.
    /// </summary>
    /// <param name="behaviour">The NetworkBehaviour instance that is being serialized.</param>
    /// <param name="serializedData">A NetworkWriter containing the serialized data of the behaviour.</param>
    /// <param name="clearBits">Whether or not dirty bits should be cleared.</param>
    public delegate void BehaviourSerializedEventHandler(NetworkBehaviour behaviour, NetworkWriter serializedData, ref bool clearBits);

    /// <summary>
    /// Represents a method that is called when a NetworkBehaviour's SyncVar value is about to be updated, allowing
    /// inspection or modification of the new value before it is applied.
    /// </summary>
    /// <param name="behaviour">The NetworkBehaviour instance containing the SyncVar that is being set.</param>
    /// <param name="syncVarType">The type of the SyncVar field being updated.</param>
    /// <param name="syncVarDirtyBit">The dirty bit mask associated with the SyncVar, used to track changes for network synchronization.</param>
    /// <param name="currentSyncVarValue">The current value of the SyncVar before the update.</param>
    /// <param name="newSyncVarValue">A reference to the new value to be assigned to the SyncVar. This value can be modified within the delegate to
    /// alter the value that will be set.</param>
    public delegate void BehaviourUpdatingSyncVarEventHandler(NetworkBehaviour behaviour, Type syncVarType, ulong syncVarDirtyBit, 
        object currentSyncVarValue, ref object newSyncVarValue);

    /// <summary>
    /// Represents a method that is called when a NetworkBehaviour's SyncVar value gets updated.
    /// <param name="behaviour">The NetworkBehaviour instance containing the SyncVar that is being set.</param>
    /// <param name="syncVarType">The type of the SyncVar field being updated.</param>
    /// <param name="syncVarDirtyBit">The dirty bit mask associated with the SyncVar, used to track changes for network synchronization.</param>
    /// <param name="previousSyncVarValue">The previous value of the SyncVar before the update.</param>
    /// <param name="newSyncVarValue">The new value to be assigned to the SyncVar.</param>
    public delegate void BehaviourUpdatedSyncVarEventHandler(NetworkBehaviour behaviour, Type syncVarType, ulong syncVarDirtyBit, 
        object previousSyncVarValue, object newSyncVarValue);

    /// <summary>
    /// Represents the method that handles the event raised when determining whether an observer should be added to a
    /// network identity.
    /// </summary>
    /// <remarks>Event handlers can modify the <paramref name="isAllowed"/> parameter to control whether the
    /// specified player is permitted to observe the network identity.</remarks>
    /// <param name="identity">The network identity for which the observer is being considered.</param>
    /// <param name="observer">The player being evaluated as a potential observer.</param>
    /// <param name="isAllowed">A reference to a Boolean value that indicates whether the observer is allowed. Set to <see langword="true"/> to
    /// allow the observer; otherwise, set to <see langword="false"/>.</param>
    public delegate void AddingObserverEventHandler(NetworkIdentity identity, ExPlayer observer, ref bool isAllowed);

    /// <summary>
    /// Represents the method that handles the event when an observer is added to a network identity.
    /// </summary>
    /// <param name="identity">The network identity to which the observer is being added.</param>
    /// <param name="observer">The observer that has been added to the network identity.</param>
    public delegate void AddedObserverEventHandler(NetworkIdentity identity, ExPlayer observer);

    /// <summary>
    /// Represents the method that handles the event raised when determining whether an observer should be removed from a
    /// network identity.
    /// </summary>
    /// <remarks>Event handlers can modify the <paramref name="isAllowed"/> parameter to control whether the
    /// specified player is permitted to observe the network identity.</remarks>
    /// <param name="identity">The network identity for which the observer is being removed.</param>
    /// <param name="observer">The player being removed.</param>
    /// <param name="isAllowed">A reference to a Boolean value that indicates whether the observer is allowed to be removed. Set to <see langword="true"/> to
    /// allow the observer; otherwise, set to <see langword="false"/>.</param>
    public delegate void RemovingObserverEventHandler(NetworkIdentity identity, ExPlayer observer, ref bool isAllowed);

    /// <summary>
    /// Represents the method that handles the event when an observer is removed from a network identity.
    /// </summary>
    /// <param name="identity">The network identity from which the observer is being removed.</param>
    /// <param name="observer">The observer that has been removed from the network identity.</param>
    public delegate void RemovedObserverEventHandler(NetworkIdentity identity, ExPlayer observer);

    /// <summary>
    /// Represents the method that handles the event raised before a remote procedure call (RPC) is sent to specified
    /// targets on the network.
    /// </summary>
    /// <remarks>This event allows inspection and modification of RPC data and targets before the message is
    /// sent. It can be used to implement custom filtering, logging, or security checks. Modifying <paramref
    /// name="isAllowed"/> to <see langword="false"/> will prevent the RPC from being sent.</remarks>
    /// <param name="behaviour">The network behaviour instance that is invoking the RPC.</param>
    /// <param name="rpcName">The name of the RPC method being called.</param>
    /// <param name="rpcHash">The hash value identifying the RPC method.</param>
    /// <param name="writer">The writer used to serialize the RPC payload data.</param>
    /// <param name="connectionTargets">The list of network connections to which the RPC will be sent.</param>
    /// <param name="message">A reference to the RPC message being sent. Can be modified to alter the message before transmission.</param>
    /// <param name="isAllowed">A reference to a Boolean value indicating whether the RPC is permitted to be sent. Set to <see
    /// langword="false"/> to block the RPC.</param>
    public delegate void SendingRpcEventHandler(NetworkBehaviour behaviour, string rpcName, int rpcHash, NetworkWriter writer, 
        List<NetworkConnection> connectionTargets, List<ExPlayer> playerTargets, ref RpcMessage message, ref bool isAllowed);

    /// <summary>
    /// Represents the method that handles an event triggered when a remote procedure call (RPC) is sent from a network
    /// behaviour.
    /// </summary>
    /// <remarks>This delegate allows inspection or modification of the outgoing RPC message before it is
    /// transmitted to the specified targets. It can be used to implement custom logging, filtering, or message
    /// transformation logic.</remarks>
    /// <param name="behaviour">The network behaviour instance from which the RPC is being sent.</param>
    /// <param name="rpcName">The name of the RPC method being invoked.</param>
    /// <param name="rpcHash">The hash value that uniquely identifies the RPC method.</param>
    /// <param name="writer">The writer used to serialize the RPC payload data.</param>
    /// <param name="targets">The list of target players to whom the RPC will be sent.</param>
    /// <param name="message">A reference to the RPC message that will be sent. Can be modified to alter the outgoing message.</param>
    public delegate void SentRpcEventHandler(NetworkBehaviour behaviour, string rpcName, int rpcHash, NetworkWriter writer, List<ExPlayer> targets, 
        ref RpcMessage message);
    #endregion

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
    
    /// <summary>
    /// Gets called before a new observer is added to an observing list.
    /// </summary>
    public static event AddingObserverEventHandler? AddingObserver;

    /// <summary>
    /// Gets called after a new observer is added to an observing list.
    /// </summary>
    public static event AddedObserverEventHandler? AddedObserver;

    /// <summary>
    /// Gets called before an observer is removed from an observing list.
    /// </summary>
    public static event RemovingObserverEventHandler? RemovingObserver;

    /// <summary>
    /// Gets called after an observer is removed from an observing list.
    /// </summary>
    public static event RemovedObserverEventHandler? RemovedObserver;

    /// <summary>
    /// Gets called before serialized data of a behaviour is written to the identity writer.
    /// </summary>
    public static event BehaviourSerializingEventHandler? BehaviourSerializing;

    /// <summary>
    /// Gets called after serialized data of a behaviour is written to the identity writer.
    /// </summary>
    public static event BehaviourSerializedEventHandler? BehaviourSerialized;

    /// <summary>
    /// Gets called before a behaviour's sync variable value is updated.
    /// </summary>
    public static event BehaviourUpdatingSyncVarEventHandler? UpdatingSyncVar;

    /// <summary>
    /// Gets called after a behaviour's sync variable value is updated.
    /// </summary>
    public static event BehaviourUpdatedSyncVarEventHandler? UpdatedSyncVar;

    /// <summary>
    /// Gets called before an RPC is sent to the specified targets.
    /// </summary>
    public static event SendingRpcEventHandler? SendingRpc;

    /// <summary>
    /// Gets called after an RPC is sent.
    /// </summary>
    public static event SentRpcEventHandler? SentRpc;

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
    public static bool OnAddingObserver(NetworkIdentity identity, ExPlayer observer)
    {
        var isAllowed = true;

        try
        {
            AddingObserver?.Invoke(identity, observer, ref isAllowed);
        }
        catch (Exception ex)
        {
            ApiLog.Error("OnAddingObserver", ex);
        }

        return isAllowed;
    }

    /// <summary>
    /// Invokes the <see cref="AddedObserver"/> event.
    /// </summary>
    public static void OnAddedObserver(NetworkIdentity identity, ExPlayer observer)
    {
        try
        {
            AddedObserver?.Invoke(identity, observer);
        }
        catch (Exception ex)
        {
            ApiLog.Error("OnAddedObserver", ex);
        }
    }

    /// <summary>
    /// Invokes the <see cref="RemovingObserver"/> event.
    /// </summary>
    public static bool OnRemovingObserver(NetworkIdentity identity, ExPlayer observer)
    {
        var isAllowed = true;

        try
        {
            RemovingObserver?.Invoke(identity, observer, ref isAllowed);
        }
        catch (Exception ex)
        {
            ApiLog.Error("OnRemovingObserver", ex);
        }

        return isAllowed;
    }

    /// <summary>
    /// Invokes the <see cref="RemovedObserver"/> event.
    /// </summary>
    public static void OnRemovedObserver(NetworkIdentity identity, ExPlayer observer)
    {
        try
        {
            RemovedObserver?.Invoke(identity, observer);
        }
        catch (Exception ex)
        {
            ApiLog.Error("OnRemovedObserver", ex);
        }
    }

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
    public static bool OnBehaviourSerialized(NetworkBehaviour behaviour, NetworkWriter writer)
    {
        try
        {
            var clearBits = true;

            BehaviourSerialized?.Invoke(behaviour, writer, ref clearBits);
            return clearBits;
        }
        catch (Exception ex)
        {
            ApiLog.Error("OnBehaviourSerializing", ex);
            return true;
        }
    }

    /// <summary>
    /// Invokes the <see cref="UpdatingSyncVar"/> event.
    /// </summary>
    public static void OnUpdatingSyncVar(NetworkBehaviour behaviour, Type syncVarType, ulong syncVarDirtyBit,
        object currentSyncVarValue, ref object newSyncVarValue)
    {
        try
        {
            UpdatingSyncVar?.Invoke(behaviour, syncVarType, syncVarDirtyBit, currentSyncVarValue, ref newSyncVarValue);
        }
        catch (Exception ex)
        {
            ApiLog.Error("OnUpdatingSyncVar", ex);
        }
    }

    /// <summary>
    /// Invokes the <see cref="UpdatedSyncVar"/> event.
    /// </summary>
    public static void OnUpdatedSyncVar(NetworkBehaviour behaviour, Type syncVarType, ulong syncVarDirtyBit,
        object previousSyncVarValue, object newSyncVarValue)
    {
        try
        {
            UpdatedSyncVar?.Invoke(behaviour, syncVarType, syncVarDirtyBit, previousSyncVarValue, newSyncVarValue);
        }
        catch (Exception ex)
        {
            ApiLog.Error("OnUpdatedSyncVar", ex);
        }
    }

    /// <summary>
    /// Invokes the <see cref="SendingRpc"/> event.
    /// </summary>
    public static bool OnSendingRpc(NetworkBehaviour behaviour, string rpcName, int rpcHash, NetworkWriter writer, List<NetworkConnection> connections, 
        List<ExPlayer> players, ref RpcMessage message)
    {
        var isAllowed = true;

        try
        {
            SendingRpc?.Invoke(behaviour, rpcName, rpcHash, writer, connections, players, ref message, ref isAllowed);
        }
        catch (Exception ex)
        {
            ApiLog.Error("OnSendingRpc", ex);
        }

        return isAllowed;
    }

    /// <summary>
    /// Invokes the <see cref="SentRpc"/> event.
    /// </summary>
    public static void OnSentRpc(NetworkBehaviour behaviour, string rpcName, int rpcHash, NetworkWriter writer, List<ExPlayer> targets, ref RpcMessage message)
    {
        try
        {
            SentRpc?.Invoke(behaviour, rpcName, rpcHash, writer, targets, ref message);
        }
        catch (Exception ex)
        {
            ApiLog.Error("OnSentRpc", ex);
        }
    }

    internal static bool Internal_AnySyncVarSubsribers()
        => UpdatingSyncVar is not null || UpdatedSyncVar is not null;

    internal static bool Internal_AnySerializingSubscribers()
        => BehaviourSerializing is not null || BehaviourSerialized is not null;
}