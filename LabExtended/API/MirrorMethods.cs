using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Attributes;
using LabExtended.Extensions;

using Mirror;

using System.Reflection.Emit;

using UnityEngine;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API;

/// <summary>
/// Helper methods targeting the Mirror library.
/// </summary>
public static class MirrorMethods
{
    /// <summary>
    /// Gets the maximum length of a string.
    /// </summary>
    public const int MaxStringLength = 65534;
    
    /// <summary>
    /// Gets full names of all RPCs.
    /// <remarks>Keys are formatted as the declaring type of the property and then the name of the property
    /// (MyType.MyProperty)</remarks>
    /// </summary>
    public static Dictionary<string, string> RpcNames { get; } = new();
    
    /// <summary>
    /// Gets hashes of all RPCs.
    /// <remarks>Keys are formatted as the declaring type of the property and then the name of the property
    /// (MyType.MyProperty)</remarks>
    /// </summary>
    public static Dictionary<string, int> RpcHashes { get; } = new();

    /// <summary>
    /// Gets dirty bits of all network properties
    /// <remarks>Keys are formatted as the declaring type of the property and then the name of the property
    /// (MyType.MyProperty)</remarks>
    /// </summary>
    public static Dictionary<string, ulong> DirtyBits { get; } = new();

    /// <summary>
    /// Gets Mirror-generated network writer extensions.
    /// </summary>
    public static Dictionary<Type, Func<object, object[], object>> Writers { get; } = new();

    /// <summary>
    /// Gets the <see cref="NetworkServer.SendSpawnMessage"/> delegate.
    /// </summary>
    public static Action<NetworkIdentity, NetworkConnection> SendSpawnMessage { get; private set; }

    /// <summary>
    /// Attempts to get a specific behaviour component of a network identity.
    /// </summary>
    /// <param name="identityId">The ID of the identity.</param>
    /// <param name="behaviour">The resolved behaviour.</param>
    /// <typeparam name="T">The behaviour type to find.</typeparam>
    /// <returns>true if the identity and behaviour were found</returns>
    public static bool TryGetBehaviour<T>(uint identityId, out T behaviour) where T : NetworkBehaviour
    {
        if (!NetworkServer.spawned.TryGetValue(identityId, out var identity))
        {
            behaviour = null;
            return false;
        }

        for (var i = 0; i < identity.NetworkBehaviours.Length; i++)
        {
            if (identity.NetworkBehaviours[i] is not T target)
                continue;

            behaviour = target;
            return true;
        }
        
        behaviour = null;
        return false;
    }

    /// <summary>
    /// Attempts to retrieve a writer delegate from <see cref="Writers"/>.
    /// </summary>
    /// <param name="type">The target type</param>
    /// <param name="writer">The found delegate</param>
    /// <returns>true if the delegate was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetWriter(this Type type, out Func<object, object[], object> writer)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        
        return Writers.TryGetValue(type, out writer);
    }

    /// <summary>
    /// Attempts to retrieve the hash of a RPC from <see cref="RpcNames"/>.
    /// </summary>
    /// <param name="fullName">The name of the RPC (DeclaringType.MethodName - case sensitive)</param>
    /// <param name="rpcHash">The found hash of the remote RPC</param>
    /// <returns>true if the RPC was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetRpcHash(string fullName, out int rpcHash)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentNullException(nameof(fullName));
        
        return RpcHashes.TryGetValue(fullName, out rpcHash);
    }

    /// <summary>
    /// Attempts to retrieve the hash of a RPC from <see cref="RpcNames"/>.
    /// </summary>
    /// <param name="typeName">Name of the declaring type (case sensitive)</param>
    /// <param name="methodName">Name of the method (case sensitive)</param>
    /// <param name="rpcHash">The found hash of the remote RPC.</param>
    /// <returns>true if the RPC was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetRpcHash(string typeName, string methodName, out int rpcHash)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            throw new ArgumentNullException(nameof(typeName));
        
        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentNullException(nameof(methodName));
        
        return RpcHashes.TryGetValue(string.Concat(typeName, ".", methodName), out rpcHash);
    }

    /// <summary>
    /// Attempts to retrieve the hash of a RPC from <see cref="RpcNames"/>.
    /// </summary>
    /// <param name="type">The declaring type</param>
    /// <param name="methodName">Name of the method (case sensitive)</param>
    /// <param name="rpcHash">The found hash of the remote RPC.</param>
    /// <returns>true if the RPC was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetRpcHash(this Type type, string methodName, out int rpcHash)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        
        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentNullException(nameof(methodName));
        
        return RpcHashes.TryGetValue(string.Concat(type.Name, ".", methodName), out rpcHash);
    }
    
    /// <summary>
    /// Attempts to retrieve the full name of a RPC from <see cref="RpcNames"/>.
    /// </summary>
    /// <param name="fullName">The name of the RPC (DeclaringType.MethodName - case sensitive)</param>
    /// <param name="rpcName">The found name of the remote RPC</param>
    /// <returns>true if the RPC was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetRpcName(string fullName, out string rpcName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentNullException(nameof(fullName));
        
        return RpcNames.TryGetValue(fullName, out rpcName);
    }

    /// <summary>
    /// Attempts to retrieve the full name of a RPC from <see cref="RpcNames"/>.
    /// </summary>
    /// <param name="typeName">Name of the declaring type (case sensitive)</param>
    /// <param name="methodName">Name of the method (case sensitive)</param>
    /// <param name="rpcName">The found name of the remote RPC.</param>
    /// <returns>true if the RPC was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetRpcName(string typeName, string methodName, out string rpcName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            throw new ArgumentNullException(nameof(typeName));
        
        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentNullException(nameof(methodName));
        
        return RpcNames.TryGetValue(string.Concat(typeName, ".", methodName), out rpcName);
    }

    /// <summary>
    /// Attempts to retrieve the full name of a RPC from <see cref="RpcNames"/>.
    /// </summary>
    /// <param name="type">The declaring type</param>
    /// <param name="methodName">Name of the method (case sensitive)</param>
    /// <param name="rpcName">The found name of the remote RPC.</param>
    /// <returns>true if the RPC was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetRpcName(this Type type, string methodName, out string rpcName)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        
        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentNullException(nameof(methodName));
        
        return RpcNames.TryGetValue(string.Concat(type.Name, ".", methodName), out rpcName);
    }

    /// <summary>
    /// Attempts to retrieve a dirty bit of a network property.
    /// </summary>
    /// <param name="propertyName">Name of the network property (formatted as DeclaringTypeName.PropertyName - case sensitive)</param>
    /// <param name="dirtyBit">The found dirty bit.</param>
    /// <returns>true if the dirty bit was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetDirtyBit(string propertyName, out ulong dirtyBit)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        return DirtyBits.TryGetValue(propertyName, out dirtyBit);
    }

    /// <summary>
    /// Attempts to retrieve a dirty bit of a network property.
    /// </summary>
    /// <param name="declaringTypeName">The name of the declaring type (case sensitive)</param>
    /// <param name="propertyName">The name of the network property (case sensitive)</param>
    /// <param name="dirtyBit">The found dirty bit</param>
    /// <returns>true if the dirty bit was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetDirtyBit(string declaringTypeName, string propertyName, out ulong dirtyBit)
    {
        if (string.IsNullOrWhiteSpace(declaringTypeName))
            throw new ArgumentNullException(nameof(declaringTypeName));
        
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        return DirtyBits.TryGetValue(string.Concat(declaringTypeName, ".", propertyName), out dirtyBit);
    }

    /// <summary>
    /// Attempts to retrieve a dirty bit of a network property.
    /// </summary>
    /// <param name="behaviourType">The declaring network behaviour type</param>
    /// <param name="propertyName">The name of the network property (case sensitive)</param>
    /// <param name="dirtyBit">The found dirty bit</param>
    /// <returns>true if the dirty bit was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetDirtyBit(this Type behaviourType, string propertyName, out ulong dirtyBit)
    {
        if (behaviourType is null)
            throw new ArgumentNullException(nameof(behaviourType));
        
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        return DirtyBits.TryGetValue(string.Concat(behaviourType.Name, propertyName), out dirtyBit);
    }

    /// <summary>
    /// Sets a network property as dirty.
    /// </summary>
    /// <param name="behaviour">The target network behaviour</param>
    /// <param name="dirtyBit">The property dirty bit</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SetDirtyBit(this NetworkBehaviour behaviour, ulong dirtyBit)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));

        behaviour.syncVarDirtyBits |= dirtyBit;
    }

    /// <summary>
    /// Sets a network property as dirty.
    /// </summary>
    /// <param name="behaviour">The target network behaviour</param>
    /// <param name="propertyName">The property to set as dirty (case sensitive)</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SetDirtyBit(this NetworkBehaviour behaviour, string propertyName)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));

        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        if (!DirtyBits.TryGetValue(string.Concat(behaviour.GetType().Name, ".", propertyName), out var dirtyBit))
            throw new ArgumentException(
                $"Property {propertyName} (in behaviour {behaviour.GetType().Name}) does not have a" +
                $"registered dirty bit", nameof(propertyName));

        behaviour.syncVarDirtyBits |= dirtyBit;
    }
    
    /// <summary>
    /// Removes a dirty bit from a network behaviour.
    /// </summary>
    /// <param name="behaviour">The target network behaviour</param>
    /// <param name="dirtyBit">The property dirty bit</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void RemoveDirtyBit(this NetworkBehaviour behaviour, ulong dirtyBit)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));

        behaviour.syncVarDirtyBits &= ~dirtyBit;
    }

    /// <summary>
    /// Removes a dirty bit from a network behaviour
    /// </summary>
    /// <param name="behaviour">The target network behaviour</param>
    /// <param name="propertyName">The property to remove the dirty bit of (case sensitive)</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void RemoveDirtyBit(this NetworkBehaviour behaviour, string propertyName)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));

        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        if (!DirtyBits.TryGetValue(string.Concat(behaviour.GetType().Name, ".", propertyName), out var dirtyBit))
            throw new ArgumentException(
                $"Property {propertyName} (in behaviour {behaviour.GetType().Name}) does not have a" +
                $"registered dirty bit", nameof(propertyName));

        behaviour.syncVarDirtyBits &= ~dirtyBit;
    }

    /// <summary>
    /// Adds all players as observers for a specific identity.
    /// </summary>
    /// <param name="identity">The target identity.</param>
    /// <param name="predicate">The optional filtering predicate.</param>
    public static void AddAllObservers(this NetworkIdentity identity, Predicate<ExPlayer>? predicate = null)
    {
        ExPlayer.Players.ForEach(ply =>
        {
            if (!ply.IsVerified || ply.ClientConnection is null || !ply.ClientConnection.isReady)
                return;

            if (predicate != null && !predicate(ply))
                return;

            identity.AddObserver(ply.ClientConnection);
        });
    }

    /// <summary>
    /// Sends a custom <see cref="SpawnMessage"/>.
    /// </summary>
    /// <param name="identity">The targeted network identity</param>
    /// <param name="predicate">Receiver filter</param>
    /// <param name="customPos">Custom identity position</param>
    /// <param name="customScale">Custom identity scale</param>
    /// <param name="customRot">Custom identity rotation</param>
    /// <param name="payload">Custom identity spawn payload</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SendCustomSpawnMessage(this NetworkIdentity identity, Func<ExPlayer, bool>? predicate = null,
        Vector3? customPos = null, Vector3? customScale = null, Quaternion? customRot = null,
        ArraySegment<byte>? payload = null)
    {
        if (identity is null)
            throw new ArgumentNullException(nameof(identity));
        
        if (predicate != null)
            WriteToWhere(predicate, x => x.Write(identity.GetSpawnMessage(customPos, customScale, customRot, payload)));
        else
            WriteToMany(ExPlayer.Players, x => x.Write(identity.GetSpawnMessage(customPos, customScale, customRot, payload)));
    }

    /// <summary>
    /// Gets a <see cref="SpawnMessage"/> for a specific identity.
    /// </summary>
    /// <param name="identity">The target network identity</param>
    /// <param name="customPos">Custom identity position</param>
    /// <param name="customScale">Custom identity scale</param>
    /// <param name="customRot">Custom identity rotation</param>
    /// <param name="payload">Custom spawn payload</param>
    /// <returns>The created <see cref="SpawnMessage"/> instance</returns>
    public static SpawnMessage GetSpawnMessage(this NetworkIdentity identity, Vector3? customPos = null,
        Vector3? customScale = null, Quaternion? customRot = null, ArraySegment<byte>? payload = null)
    {
        var msg = new SpawnMessage
        {
            assetId = identity.assetId,

            isLocalPlayer = identity.isLocalPlayer,
            isOwner = identity.isOwned,

            netId = identity.netId,

            sceneId = identity.sceneId,

            position = customPos ?? identity.transform.position,
            rotation = customRot ?? identity.transform.rotation,

            scale = customScale ?? identity.transform.localScale
        };

        if (payload.HasValue)
            msg.payload = payload.Value;

        return msg;
    }

    /// <summary>
    /// Finds the index of a network behaviour's type.
    /// </summary>
    /// <param name="identity">The target identity</param>
    /// <param name="behaviourType">The behaviour type to find</param>
    /// <returns>the index the type was found at or -1 if not found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static int FindBehaviourIndex(this NetworkIdentity identity, Type behaviourType)
    {
        if (identity is null)
            throw new ArgumentNullException(nameof(identity));

        if (identity.NetworkBehaviours?.Length < 1)
            return -1;

        for (var i = 0; i < identity.NetworkBehaviours.Length; i++)
        {
            if (identity.NetworkBehaviours[i].GetType() != behaviourType)
                continue;

            return i;
        }

        return -1;
    }

    /// <summary>
    /// Edits the properties of a network identity.
    /// </summary>
    /// <param name="identity">The identity to edit</param>
    /// <param name="edit">The method used to edit the identity</param>
    /// <param name="predicate">The receiver filter</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static void EditIdentity(this NetworkIdentity identity, Action<NetworkIdentity> edit, Predicate<ExPlayer>? predicate = null)
    {
        if (identity is null)
            throw new ArgumentNullException(nameof(identity));

        if (edit is null)
            throw new ArgumentNullException(nameof(edit));

        if (identity.netId == 0)
            throw new Exception("Attempted to edit a NetworkIdentity that is either despawned or is a scene object.");

        edit.InvokeSafe(identity, true);

        var destroyMsg = new ObjectDestroyMessage() { netId = identity.netId };
        var spawnMsg = identity.GetSpawnMessage();

        ExPlayer.Players.ForEach(p =>
        {
            if (predicate != null && !predicate(p))
                return;

            p.Connection.Send(destroyMsg);
            p.Connection.Send(spawnMsg);
        });
    }

    #region Spawn For
    /// <summary>
    /// Spawns the target behaviour for selected connection.
    /// </summary>
    /// <param name="behaviour">The target behaviour.</param>
    /// <param name="target">The selected connection.</param>
    /// <param name="ownerConnection">Optional identity owner connection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SpawnFor(this NetworkBehaviour behaviour, NetworkConnection target,
        NetworkConnection? ownerConnection = null)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));
        
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (!target.isReady)
            throw new ArgumentException("Target connection cannot receive data", nameof(target));
        
        if (target is not NetworkConnectionToClient connectionToClient)
            return;
        
        InitIdentityServerSide(behaviour.netIdentity, ownerConnection);
        
        behaviour.netIdentity.AddObserver(connectionToClient);
    }
    
    /// <summary>
    /// Spawns the target behaviour for selected player.
    /// </summary>
    /// <param name="behaviour">The target behaviour.</param>
    /// <param name="target">The selected player.</param>
    /// <param name="ownerConnection">Optional identity owner connection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SpawnFor(this NetworkBehaviour behaviour, ExPlayer target,
        NetworkConnection? ownerConnection = null)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));
        
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        if (target.Connection is not NetworkConnectionToClient connectionToClient)
            return;
        
        InitIdentityServerSide(behaviour.netIdentity, ownerConnection);
        
        behaviour.netIdentity.AddObserver(connectionToClient);
    }

    /// <summary>
    /// Spawns the target behaviour for selected connections.
    /// </summary>
    /// <param name="behaviour">The target behaviour.</param>
    /// <param name="connections">The selected connections.</param>
    /// <param name="ownerConnection">Optional identity owner connection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SpawnFor(this NetworkBehaviour behaviour, IEnumerable<NetworkConnection> connections, NetworkConnection? ownerConnection = null)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));
        
        if (connections is null)
            throw new ArgumentNullException(nameof(connections));
        
        InitIdentityServerSide(behaviour.netIdentity, ownerConnection);

        foreach (var connection in connections)
        {
            if (connection is not NetworkConnectionToClient connectionToClient)
                continue;
            
            behaviour.netIdentity.AddObserver(connectionToClient);
        }
    }

    /// <summary>
    /// Spawns the target behaviour for selected players.
    /// </summary>
    /// <param name="behaviour">The target behaviour.</param>
    /// <param name="players">The selected players.</param>
    /// <param name="ownerConnection">Optional identity owner connection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SpawnFor(this NetworkBehaviour behaviour, IEnumerable<ExPlayer> players, NetworkConnection? ownerConnection = null)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));
        
        if (players is null)
            throw new ArgumentNullException(nameof(players));
        
        InitIdentityServerSide(behaviour.netIdentity, ownerConnection);

        foreach (var player in players)
        {
            if (player.Connection is not NetworkConnectionToClient connectionToClient)
                continue;
            
            behaviour.netIdentity.AddObserver(connectionToClient);
        }
    }
    
    /// <summary>
    /// Spawns the target behaviour for selected players.
    /// </summary>
    /// <param name="behaviour">The target behaviour.</param>
    /// <param name="predicate">Player filter.</param>
    /// <param name="ownerConnection">Optional identity owner connection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SpawnFor(this NetworkBehaviour behaviour, Func<ExPlayer, bool> predicate, NetworkConnection? ownerConnection = null)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));
        
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        InitIdentityServerSide(behaviour.netIdentity, ownerConnection);

        ExPlayer.Players.ForEach(p =>
        {
            if (!predicate(p))
                return;
                
            if (p.Connection is not NetworkConnectionToClient connectionToClient)
                return;
            
            behaviour.netIdentity.AddObserver(connectionToClient);
        });
    }
    #endregion

    #region Destroy For
    /// <summary>
    /// Destroys the behaviour for the selected connection.
    /// </summary>
    /// <param name="behaviour">The target behaviour.</param>
    /// <param name="target">The target connection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void DestroyFor(this NetworkBehaviour behaviour, NetworkConnection target)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));
        
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        if (target is NetworkConnectionToClient connectionToClient)
            InternalDestroyFor(behaviour.netIdentity, connectionToClient);
        
        target.Send(new ObjectDestroyMessage { netId = behaviour.netId });
    }

    /// <summary>
    /// Destroys the behaviour for the selected connections.
    /// </summary>
    /// <param name="behaviour">The target behaviour.</param>
    /// <param name="connections">The target connections.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void DestroyFor(this NetworkBehaviour behaviour, IEnumerable<NetworkConnection> connections)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));
        
        if (connections is null)
            throw new ArgumentNullException(nameof(connections));

        var msg = new ObjectDestroyMessage { netId = behaviour.netId };

        connections.ForEach(c =>
        {
            if (c is NetworkConnectionToClient connectionToClient)
                InternalDestroyFor(behaviour.netIdentity, connectionToClient);

            c.Send(msg);
        });
    }
    
    /// <summary>
    /// Destroys the behaviour for the selected players.
    /// </summary>
    /// <param name="behaviour">The target behaviour.</param>
    /// <param name="players">The target players.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void DestroyFor(this NetworkBehaviour behaviour, IEnumerable<ExPlayer> players)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));
        
        if (players is null)
            throw new ArgumentNullException(nameof(players));

        var msg = new ObjectDestroyMessage { netId = behaviour.netId };

        players.ForEach(c =>
        {
            if (c.ClientConnection is null)
                return;
            
            InternalDestroyFor(behaviour.netIdentity, c.ClientConnection);

            c.Send(msg);
        });
    }
    
    /// <summary>
    /// Destroys the behaviour for the selected players.
    /// </summary>
    /// <param name="behaviour">The target behaviour.</param>
    /// <param name="predicate">Player filter</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void DestroyFor(this NetworkBehaviour behaviour, Func<ExPlayer, bool> predicate)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));
        
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var msg = new ObjectDestroyMessage { netId = behaviour.netId };

        ExPlayer.Players.ForEach(p =>
        {
            if (p.ClientConnection is null || !predicate(p))
                return;
            
            InternalDestroyFor(behaviour.netIdentity, p.ClientConnection);
            
            p.Send(msg);
        });
    }

    private static void InternalDestroyFor(NetworkIdentity identity, NetworkConnectionToClient conn)
    {
        identity.RemoveObserver(conn);
        
        conn.RemoveFromObserving(identity, false);
    }
    #endregion
    
    /// <summary>
    /// Creates a new <see cref="RpcMessage"/> for the specified behaviour.
    /// </summary>
    /// <param name="behaviour">The target behaviour</param>
    /// <param name="rpcHash">Hash of the RPC method</param>
    /// <param name="dataWriter">Optional data writer delegate</param>
    /// <returns>The created <see cref="RpcMessage"/> instance</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static RpcMessage GetRpcMessage(this NetworkBehaviour behaviour, int rpcHash,
        Action<NetworkWriter>? dataWriter = null)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));

        var message = new RpcMessage();
        
        message.netId = behaviour.netId;
        message.functionHash = (ushort)rpcHash;
        message.componentIndex = behaviour.ComponentIndex;

        if (dataWriter != null)
            message.payload = Write(dataWriter);

        return message;
    }

    /// <summary>
    /// Creates a new <see cref="RpcMessage"/> for the specified behaviour.
    /// <remarks>It's recommended to cache the hash of the target RPC and
    /// call <see cref="GetRpcMessage(Mirror.NetworkBehaviour,int,System.Action{Mirror.NetworkWriter}?)"/> directly
    /// as this method looks the hash up each time it is called.</remarks>
    /// </summary>
    /// <param name="behaviour">The target behaviour</param>
    /// <param name="methodName"></param>
    /// <param name="dataWriter">Optional data writer delegate</param>
    /// <returns>The created <see cref="RpcMessage"/> instance</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static RpcMessage GetRpcMessage(this NetworkBehaviour behaviour, string methodName, Action<NetworkWriter>? dataWriter = null)
    {
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));

        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentNullException(nameof(methodName));

        if (!TryGetRpcHash(behaviour.GetType(), methodName, out var hash))
            throw new Exception($"Could not find hash of RPC {methodName} (in type {behaviour.GetType().Name})");
        
        return GetRpcMessage(behaviour, hash, dataWriter);
    }

    /// <summary>
    /// Executes the target delegate on a writer and returns it's data.
    /// </summary>
    /// <param name="writer">The writer delegate.</param>
    /// <returns>The written data.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ArraySegment<byte> Write(Action<NetworkWriter> writer)
    {
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));

        using var pooled = NetworkWriterPool.Get();
        
        writer(pooled);
        return pooled.ToArraySegment();
    }

    /// <summary>
    /// Writes custom sync var data to the writer.
    /// </summary>
    /// <param name="writer">The target writer</param>
    /// <param name="behaviour">The target behaviour instance</param>
    /// <param name="propertyName">The target property name (case sensitive)</param>
    /// <param name="customValue">The custom value</param>
    /// <param name="valueWriter">The custom value writer delegate</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void WriteCustomSyncVar(this NetworkWriter writer, NetworkBehaviour behaviour, string propertyName,
        object customValue, Action<NetworkWriter, object>? valueWriter = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentNullException(nameof(propertyName));
        
        if (!TryGetDirtyBit(behaviour.GetType(), propertyName, out var bit))
            throw new ArgumentException($"Unknown network property: {behaviour.GetType().Name}.{propertyName}");
        
        WriteCustomSyncVar(writer, behaviour, bit, customValue, valueWriter);
    }
    
    /// <summary>
    /// Writes custom sync var data to the writer.
    /// </summary>
    /// <param name="writer">The target writer</param>
    /// <param name="behaviour">The target behaviour instance</param>
    /// <param name="propertyName">The target property name (case sensitive)</param>
    /// <param name="customValue">The custom value</param>
    /// <param name="valueWriter">The custom value writer delegate</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void WriteCustomSyncVar<T>(this NetworkWriter writer, NetworkBehaviour behaviour, string propertyName,
        T customValue, Action<NetworkWriter, T>? valueWriter = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentNullException(nameof(propertyName));
        
        if (!TryGetDirtyBit(behaviour.GetType(), propertyName, out var bit))
            throw new ArgumentException($"Unknown network property: {behaviour.GetType().Name}.{propertyName}");
        
        WriteCustomSyncVar(writer, behaviour, bit, customValue, valueWriter);
    }

    /// <summary>
    /// Writes custom sync var data to the writer.
    /// </summary>
    /// <param name="writer">The target writer</param>
    /// <param name="behaviour">The target behaviour instance</param>
    /// <param name="dirtyBit">The property dirty bit</param>
    /// <param name="customValue">The custom value</param>
    /// <param name="valueWriter">The custom value writer delegate</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void WriteCustomSyncVar(this NetworkWriter writer, NetworkBehaviour behaviour, ulong dirtyBit, 
        object customValue, Action<NetworkWriter, object>? valueWriter = null)
    {
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));
        
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));
        
        if (customValue is null)
            throw new ArgumentNullException(nameof(customValue));

        if (valueWriter is null )
        {
            if (!Writers.TryGetValue(customValue.GetType(), out var definedValueWriter))
                throw new Exception($"Type {customValue.GetType().FullName} does not have a defined writer");

            valueWriter = (x, y) => definedValueWriter(null, [x, y]);
        }
        
        WriteCustomSyncVar(writer, behaviour.netId, dirtyBit, behaviour.ComponentIndex, customValue, valueWriter);
    }
    
    /// <summary>
    /// Writes custom sync var data to the writer.
    /// </summary>
    /// <param name="writer">The target writer</param>
    /// <param name="behaviour">The target behaviour instance</param>
    /// <param name="dirtyBit">The property dirty bit</param>
    /// <param name="customValue">The custom value</param>
    /// <param name="valueWriter">The custom value writer delegate</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void WriteCustomSyncVar<T>(this NetworkWriter writer, NetworkBehaviour behaviour, ulong dirtyBit, 
        T customValue, Action<NetworkWriter, T>? valueWriter = null)
    {
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));
        
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));
        
        if (customValue is null)
            throw new ArgumentNullException(nameof(customValue));

        if (valueWriter is null)
        {
            if (!Writers.TryGetValue(customValue.GetType(), out var definedValueWriter))
                throw new Exception($"Type {customValue.GetType().FullName} does not have a defined writer");

            valueWriter = (x, y) => definedValueWriter(null, [x, y]);
        }
        
        WriteCustomSyncVar(writer, behaviour.netId, dirtyBit, behaviour.ComponentIndex, customValue, 
            (x, _) => valueWriter(x, customValue));
    }

    /// <summary>
    /// Writes a custom sync var into the writer.
    /// </summary>
    /// <param name="writer">The target writer</param>
    /// <param name="netId">The target identity ID</param>
    /// <param name="dirtyBit">The target property dirty bit</param>
    /// <param name="behaviourIndex">The target behaviour index</param>
    /// <param name="customValue">The custom value</param>
    /// <param name="customValueWriter">The writer for the custom value</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void WriteCustomSyncVar(this NetworkWriter writer, uint netId, ulong dirtyBit,
        int behaviourIndex, object customValue, Action<NetworkWriter, object> customValueWriter)
    {
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));
        
        if (customValue is null)
            throw new ArgumentNullException(nameof(customValue));
        
        if (customValueWriter is null)
            throw new ArgumentNullException(nameof(customValueWriter));
        
        using var segmentWriter = NetworkWriterPool.Get();

        if (segmentWriter.Position != 0)
            segmentWriter.Reset();

        var num = 0UL | 1UL << (behaviourIndex & 31);

        Compression.CompressVarUInt(segmentWriter, num);

        var pos1 = segmentWriter.Position;

        segmentWriter.WriteByte(0);

        var pos2 = segmentWriter.Position;

        segmentWriter.WriteULong(dirtyBit);

        customValueWriter(writer, customValue);

        var pos3 = segmentWriter.Position;

        segmentWriter.Position = pos1;

        var b = (byte)((pos3 - pos2) & 255);

        segmentWriter.WriteByte(b);
        segmentWriter.Position = pos3;

        writer.WriteMessageId<EntityStateMessage>();
        writer.WriteUInt(netId);
        writer.WriteArraySegmentAndSize(segmentWriter.ToArraySegment());
    }

    /// <summary>
    /// Executes the target delegate on a writer and sends it's data to the connection.
    /// </summary>
    /// <param name="connection">Connection to send the data to</param>
    /// <param name="writer">The writer delegate</param>
    public static void WriteTo(this NetworkConnection connection, Action<NetworkWriter> writer)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));
        
        if (!connection.isReady)
            throw new ArgumentException($"The target connection cannot receive data", nameof(connection));
        
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));
        
        connection.Send(Write(writer));
    }

    /// <summary>
    /// Executes the target delegate on a writer and sends it's data to the connections.
    /// </summary>
    /// <param name="connections">Connections to send the data to</param>
    /// <param name="writer">The writer delegate</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void WriteToMany(this IEnumerable<NetworkConnection> connections, Action<NetworkWriter> writer)
    {
        if (connections is null)
            throw new ArgumentNullException(nameof(connections));
        
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));

        var data = Write(writer);

        foreach (var connection in connections)
            connection.Send(data);
    }

    /// <summary>
    /// Executes the target delegate on a writer and sends it's data to the players.
    /// </summary>
    /// <param name="players">Players to send the data to</param>
    /// <param name="writer">The writer delegate</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void WriteToMany(this IEnumerable<ExPlayer> players, Action<NetworkWriter> writer)
    {
        if (players is null)
            throw new ArgumentNullException(nameof(players));
        
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));

        var data = Write(writer);

        foreach (var player in players)
            player.Send(data);
    }

    /// <summary>
    /// Executes the target delegate on a writer and sends it's data to the selected players
    /// </summary>
    /// <param name="predicate">Data receiver filter</param>
    /// <param name="writer">The writer delegate</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void WriteToWhere(Func<ExPlayer, bool> predicate, Action<NetworkWriter> writer)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));

        var data = Write(writer);

        ExPlayer.Players.ForEach(p =>
        {
            if (!predicate(p))
                return;
            
            p.Send(data);
        });
    }

    /// <summary>
    /// Writes ID of the message type.
    /// </summary>
    /// <param name="writer">The writer to write the ID to.</param>
    /// <typeparam name="T">The type of message to write the ID of</typeparam>
    public static void WriteMessageId<T>(this NetworkWriter writer) where T : struct, NetworkMessage
        => writer.WriteUShort(NetworkMessageId<T>.Id);

    /// <summary>
    /// Writes a message to the writer.
    /// </summary>
    /// <param name="writer">The writer delegate</param>
    /// <typeparam name="T">The type of message to write</typeparam>
    public static ArraySegment<byte> WriteMessage<T>(Action<NetworkWriter> writer) where T : struct, NetworkMessage
    {
        using var pooled = NetworkWriterPool.Get();
        
        pooled.WriteUShort(NetworkMessageId<T>.Id);
        
        writer(pooled);
        return pooled.ToArraySegment();
    }

    private static void InitIdentityServerSide(NetworkIdentity identity, NetworkConnection? ownerConnection)
    {
        if (identity is null)
            throw new ArgumentNullException(nameof(identity));
        
        if (NetworkServer.spawned.ContainsKey(identity.netId))
            throw new Exception($"Identity {identity.netId} is already spawned");

        identity.connectionToClient = (NetworkConnectionToClient)ownerConnection;
        
        if (ownerConnection is LocalConnectionToClient)
            identity.isOwned = true;
        
        if (identity is { isServer: false, netId: 0 })
        {
            identity.isLocalPlayer = NetworkClient.localPlayer == identity;
            identity.isClient = NetworkClient.active;
            identity.isServer = true;
            
            identity.netId = NetworkIdentity.GetNextNetworkId();
            
            NetworkServer.spawned[identity.netId] = identity;
            
            identity.OnStartServer();
        }
    }

    // TODO: There's a weird bug that causes the Mono runtime to randomly crash at random points in this method.
    [LoaderInitialize(1)]
    private static void OnInit()
    {
        try
        {
            SendSpawnMessage =
                typeof(NetworkServer).FindMethod(x => x.Name == "SendSpawnMessage")
                        .CreateDelegate(typeof(Action<NetworkIdentity, NetworkConnection>)) as
                    Action<NetworkIdentity, NetworkConnection>;

            var assembly = typeof(ServerConsole).Assembly;
            var types = assembly.GetTypes();

            for (var i = 0; i < types.Length; i++)
            {
                try
                {
                    var type = types[i];

                    var methods = type.GetAllMethods();
                    var properties = type.GetAllProperties();

                    var isSerializer = type.Name.EndsWith("Serializer");

                    for (var x = 0; x < methods.Length; x++)
                    {
                        var method = methods[x];

                        if (isSerializer && method.ReturnType == typeof(void) && method.Name.StartsWith("Write"))
                        {
                            var parameters = method.GetAllParameters();
                            if (parameters.Length != 2) continue;

                            var serializedType = parameters
                                .FirstOrDefault(y => y.ParameterType != typeof(NetworkWriter))
                                ?.ParameterType;

                            if (serializedType is null) continue;
                            if (Writers.ContainsKey(serializedType)) continue;
                            
                            var invoker = FastReflection.ForMethod(method);

                            Writers.Add(serializedType, invoker);
                        }
                        else if (method.HasAttribute<ClientRpcAttribute>() || method.HasAttribute<TargetRpcAttribute>())
                        {
                            var name = $"{method.ReflectedType.Name}.{method.Name}";

                            if (RpcNames.ContainsKey(name))
                                continue;

                            var body = method.GetMethodBody();
                            if (body is null) continue;

                            var codes = body.GetILAsByteArray();
                            if (codes?.Length < 1) continue;

                            var full = method.Module.ResolveString(BitConverter.ToInt32(codes,
                                codes.IndexOf((byte)OpCodes.Ldstr.Value) + 1));
                            var hashIndex = codes.IndexOf((byte)OpCodes.Ldc_I4.Value) + 1;
                            var hash = codes[hashIndex] | (codes[hashIndex + 1] << 8) | (codes[hashIndex + 2] << 16) |
                                       (codes[hashIndex + 3] << 24);

                            RpcNames.Add(name, full);
                            RpcHashes.Add(name, hash);
                        }
                    }

                    for (var y = 0; y < properties.Length; y++)
                    {
                        var prop = properties[y];
                        if (!prop.Name.StartsWith("Network")) continue;

                        var name = $"{prop.ReflectedType.Name}.{prop.Name}";
                        if (DirtyBits.ContainsKey(name)) continue;

                        var setter = prop.GetSetMethod(true);
                        if (setter is null) continue;

                        var body = setter.GetMethodBody();
                        if (body is null) continue;

                        var il = body.GetILAsByteArray();
                        if (il?.Length < 1) continue;
                        
                        var bit = il[il.LastIndexOf((byte)OpCodes.Ldc_I8.Value) + 1];

                        DirtyBits.Add(name, bit);
                    }
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Mirror Methods", ex);
                }
            }
            
            var writerExtensions = typeof(NetworkWriterExtensions).GetAllMethods();

            for (var i = 0; i < writerExtensions.Length; i++)
            {
                try
                {
                    var method = writerExtensions[i];

                    if (method.IsGenericMethod) continue;
                    if (method.HasAttribute<ObsoleteAttribute>()) continue;

                    var parameters = method.GetAllParameters();
                    if (parameters.Length != 2) continue;

                    var type = parameters.FirstOrDefault(x => x.ParameterType != typeof(NetworkWriter))?.ParameterType;
                    if (type is null) continue;
                    
                    var invoker = FastReflection.ForMethod(method);

                    Writers.Add(type, invoker);
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Mirror Methods", ex);
                }
            }
            
            var generatedType = assembly.GetType("Mirror.GeneratedNetworkCode");
            var generatedMethods = generatedType.GetAllMethods();
            
            for (var i = 0; i < generatedMethods.Length; i++)
            {
                try
                {
                    var method = generatedMethods[i];

                    if (method.IsGenericMethod) continue;
                    if (method.ReturnType != typeof(void)) continue;

                    var parameters = method.GetAllParameters();
                    if (parameters.Length != 2) continue;

                    var type = parameters.FirstOrDefault(x => x.ParameterType != typeof(NetworkWriter))?.ParameterType;

                    if (type is null) continue;
                    if (Writers.ContainsKey(type)) continue;

                    var invoker = FastReflection.ForMethod(method);

                    Writers.Add(type, invoker);
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Mirror Methods", ex);
                }
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("Mirror Methods", ex);
        }
    }
}