using LabExtended.API;
using LabExtended.Extensions;
using LabExtended.Utilities;

using Mirror;

using System.Reflection.Emit;

using UnityEngine;

namespace LabExtended.Core.Networking
{
    public static class MirrorMethods
    {
        public static volatile HashSet<Tuple<Type, Func<object, object[], object>>> Writers;
        public static volatile HashSet<Tuple<string, string>> RpcFullNames;
        public static volatile HashSet<Tuple<string, ulong>> DirtyBits;

        public static volatile Action<NetworkIdentity, NetworkConnection> SendSpawnMessageDelegate;

        static MirrorMethods()
        {
            try
            {
                Writers = new();
                DirtyBits = new();
                RpcFullNames = new();

                SendSpawnMessageDelegate =
                    typeof(NetworkServer).FindMethod(x => x.Name == "SendSpawnMessage")
                            .CreateDelegate(typeof(Action<NetworkIdentity, NetworkConnection>)) as
                        Action<NetworkIdentity, NetworkConnection>;

                var assembly = typeof(ServerConsole).Assembly;
                var types = assembly.GetTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    var type = types[i];

                    var methods = type.GetAllMethods();
                    var properties = type.GetAllProperties();

                    var isSerializer = type.Name.EndsWith("Serializer");

                    for (int x = 0; x < methods.Length; x++)
                    {
                        var method = methods[x];

                        if (isSerializer && method.ReturnType == typeof(void) && method.Name.StartsWith("Write"))
                        {
                            var parameters = method.GetAllParameters();

                            if (parameters.Length != 2)
                                continue;

                            var serializedType = parameters.FirstOrDefault(x => x.ParameterType != typeof(NetworkWriter))?.ParameterType;

                            if (serializedType is null)
                                continue;

                            var invoker = FastReflection.ForMethod(method);

                            Writers.Add(new Tuple<Type, Func<object, object[], object>>(serializedType, invoker));
                        }
                        else if (method.HasAttribute<ClientRpcAttribute>() || method.HasAttribute<TargetRpcAttribute>())
                        {
                            var name = $"{method.ReflectedType.Name}.{method.Name}";

                            if (RpcFullNames.Any(x => x.Item1 == name))
                                continue;

                            var body = method.GetMethodBody();

                            if (body is null)
                                continue;

                            var codes = body.GetILAsByteArray();

                            if (codes.Length < 1)
                                continue;

                            var full = method.Module.ResolveString(BitConverter.ToInt32(codes, codes.IndexOf((byte)OpCodes.Ldstr.Value) + 1));

                            RpcFullNames.Add(new Tuple<string, string>(name, full));
                        }
                    }

                    for (int y = 0; y < properties.Length; y++)
                    {
                        var prop = properties[y];

                        if (!prop.Name.StartsWith("Network"))
                            continue;

                        var name = $"{prop.ReflectedType.Name}.{prop.Name}";

                        if (DirtyBits.Any(x => x.Item1 == name))
                            continue;

                        var setter = prop.GetSetMethod(true);

                        if (setter is null)
                            continue;

                        var body = setter.GetMethodBody();

                        if (body is null)
                            continue;

                        var il = body.GetILAsByteArray();

                        if (il.Length < 1)
                            continue;

                        var bit = il[il.LastIndexOf((byte)OpCodes.Ldc_I8.Value) + 1];

                        DirtyBits.Add(new Tuple<string, ulong>(name, bit));
                    }
                }

                var writerExtensions = typeof(NetworkWriterExtensions).GetAllMethods();

                for (int i = 0; i < writerExtensions.Length; i++)
                {
                    var method = writerExtensions[i];

                    if (method.IsGenericMethod)
                        continue;

                    if (method.HasAttribute<ObsoleteAttribute>())
                        continue;

                    var parameters = method.GetAllParameters();

                    if (parameters.Length != 2)
                        continue;

                    var type = parameters.FirstOrDefault(x => x.ParameterType != typeof(NetworkWriter))?.ParameterType;

                    if (type is null)
                        continue;

                    var invoker = FastReflection.ForMethod(method);

                    Writers.Add(new Tuple<Type, Func<object, object[], object>>(type, invoker));
                }

                var generatedType = assembly.GetType("Mirror.GeneratedNetworkCode");
                var generatedMethods = generatedType.GetAllMethods();

                for (int i = 0; i < generatedMethods.Length; i++)
                {
                    var method = generatedMethods[i];

                    if (method.IsGenericMethod)
                        continue;

                    if (method.ReturnType != typeof(void))
                        continue;

                    var parameters = method.GetAllParameters();

                    if (parameters.Length != 2)
                        continue;

                    var type = parameters.FirstOrDefault(x => x.ParameterType != typeof(NetworkWriter))?.ParameterType;

                    if (type is null)
                        continue;

                    var invoker = FastReflection.ForMethod(method);

                    Writers.Add(new Tuple<Type, Func<object, object[], object>>(type, invoker));
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Mirror Methods", $"An exception occured while loading MirroMethods:\n{ex.ToColoredString()}");
            }
        }

        public static Func<object, object[], object> GetWriterDelegate(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (!Writers.TryGetFirst(x => x.Item1 == type, out var writer))
                throw new Exception($"No writers for type {type.FullName}");

            return writer.Item2;
        }

        public static string GetRpcName(string methodName)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentNullException(methodName);

            if (!methodName.Contains("."))
                throw new ArgumentException($"Must contain the declaring type name, ex. MyType.RpcMethod", nameof(methodName));

            if (!RpcFullNames.TryGetFirst(x => x.Item1 == methodName, out var fullName))
                throw new Exception($"Unknown RPC: {methodName}");

            return fullName.Item2;
        }

        public static ulong GetSyncVarBit(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(propertyName);

            if (!propertyName.Contains("."))
                throw new ArgumentException($"Must contain the declaring type name, ex. MyType.NetworkProperty", nameof(propertyName));

            if (!DirtyBits.TryGetFirst(x => x.Item1 == propertyName, out var bit))
                throw new Exception($"Unknown network property: {propertyName}");

            return bit.Item2;
        }

        public static void SetDirtyBit(this NetworkBehaviour behaviour, ulong bit)
            => behaviour?.SetSyncVarDirtyBit(bit);

        public static void SendSpawnMessage(this NetworkIdentity? identity, NetworkConnection connection)
            => SendSpawnMessageDelegate(identity, connection);

        public static void SendSpawnMessage(this NetworkIdentity identity, Func<ExPlayer, bool> predicate = null, Vector3? customPos = null, Vector3? customScale = null, Quaternion? customRot = null, ArraySegment<byte>? payload = null)
        {
            if (identity is null)
                throw new ArgumentNullException(nameof(identity));

            var msg = identity.GetSpawnMessage(customPos, customScale, customRot, payload);
            var writer = NetworkWriterPool.Get();

            writer.Write(msg);

            var msgData = ReturnWriterAndData(writer);

            for (int i = 0; i < ExPlayer.Players.Count; i++)
            {
                var player = ExPlayer.Players[i];

                if (predicate != null && !predicate.InvokeSafe(player))
                    continue;

                player.Send(msgData);
            }
        }

        public static SpawnMessage GetSpawnMessage(this NetworkIdentity identity, Vector3? customPos = null, Vector3? customScale = null, Quaternion? customRot = null, ArraySegment<byte>? payload = null)
        {
            var msg = new SpawnMessage
            {
                assetId = identity.assetId,

                isLocalPlayer = identity.isLocalPlayer,
                isOwner = identity.isOwned,

                netId = identity.netId,

                sceneId = identity.sceneId,

                position = customPos.HasValue ? customPos.Value : identity.transform.position,
                rotation = customRot.HasValue ? customRot.Value : identity.transform.rotation,

                scale = customScale.HasValue ? customScale.Value : identity.transform.localScale
            };

            if (payload.HasValue)
                msg.payload = payload.Value;

            return msg;
        }

        public static int FindBehaviourIndex(this NetworkIdentity identity, Type behaviourType)
        {
            if (identity is null)
                throw new ArgumentNullException(nameof(identity));

            if (identity.NetworkBehaviours is null)
                throw new Exception($"This NetworkIdentity has not been initialized yet.");

            if (identity.NetworkBehaviours.Length < 1)
                return -1;

            for (int i = 0; i < identity.NetworkBehaviours.Length; i++)
            {
                if (identity.NetworkBehaviours[i].GetType() != behaviourType)
                    continue;

                return i;
            }

            return -1;
        }

        public static void EditIdentity(this NetworkIdentity identity, Action<NetworkIdentity> edit, Predicate<ExPlayer> predicate = null)
        {
            if (identity is null)
                throw new ArgumentNullException(nameof(identity));

            if (edit is null)
                throw new ArgumentNullException(nameof(edit));

            if (identity.netId == 0)
                throw new Exception($"Attempted to edit a NetworkIdentity that is either despawned or is a scene object.");

            edit.InvokeSafe(identity, true);

            var destroyMsg = new ObjectDestroyMessage() { netId = identity.netId };
            var spawnMsg = identity.GetSpawnMessage();

            foreach (var player in ExPlayer.Players)
            {
                if (predicate != null && !predicate(player))
                    continue;

                player.Connection.Send(destroyMsg);
                player.Connection.Send(spawnMsg);
            }
        }

        public static void SendRpc(this NetworkBehaviour behaviour, string rpcName, int rpcHash, Action<NetworkWriter> customWriter, int channelId = 0, bool includeOwner = true, bool checkObservers = true, IEnumerable<ExPlayer> customObservers = null)
        {
            if (behaviour is null)
                throw new ArgumentNullException(nameof(behaviour));

            if (string.IsNullOrWhiteSpace(rpcName))
                throw new ArgumentNullException(nameof(rpcName));

            if (checkObservers && behaviour.netIdentity.observers.Count < 1)
                return;

            if (customWriter != null)
            {
                var writer = NetworkWriterPool.Get();

                customWriter.InvokeSafe(writer, true);

                var data = ReturnWriterAndData(writer);

                SendRpc(behaviour, rpcName, rpcHash, data, channelId, includeOwner, checkObservers, customObservers);
            }
            else
            {
                SendRpc(behaviour, rpcName, rpcHash, (ArraySegment<byte>?)null, channelId, includeOwner, checkObservers, customObservers);
            }
        }

        public static void SendRpc(this NetworkBehaviour behaviour, string rpcName, int rpcHash, Action<NetworkWriter> customWriter, int channelId = 0, bool includeOwner = true, bool checkObservers = true, IEnumerable<NetworkConnectionToClient> customObservers = null)
        {
            if (behaviour is null)
                throw new ArgumentNullException(nameof(behaviour));

            if (string.IsNullOrWhiteSpace(rpcName))
                throw new ArgumentNullException(nameof(rpcName));

            if (checkObservers && behaviour.netIdentity.observers.Count < 1)
                return;

            if (customWriter != null)
            {
                var writer = NetworkWriterPool.Get();

                customWriter.InvokeSafe(writer, true);

                var data = ReturnWriterAndData(writer);

                SendRpc(behaviour, rpcName, rpcHash, data, channelId, includeOwner, checkObservers, customObservers);
            }
            else
            {
                SendRpc(behaviour, rpcName, rpcHash, (ArraySegment<byte>?)null, channelId, includeOwner, checkObservers, customObservers);
            }
        }

        public static void SendRpc(this NetworkBehaviour behaviour, string rpcName, int rpcHash, NetworkWriter writer, 
            int channelId = 0, bool includeOwner = true, bool checkObservers = true, IEnumerable<ExPlayer> customObservers = null)
        {
            if (behaviour is null) throw new ArgumentNullException(nameof(behaviour));
            if (string.IsNullOrWhiteSpace(rpcName)) throw new ArgumentNullException(nameof(rpcName));

            if (writer != null)
            {
                var data = writer.ToArraySegment();
                
                SendRpc(behaviour, rpcName, rpcHash, data, channelId, includeOwner, checkObservers, customObservers);
            }
            else
            {
                SendRpc(behaviour, rpcName, rpcHash, (ArraySegment<byte>?)null, channelId, includeOwner, checkObservers, customObservers);
            }
        }

        public static void SendRpc(this NetworkBehaviour behaviour, string rpcName, int rpcHash, NetworkWriter writer, 
            int channelId = 0, bool includeOwner = true, bool checkObservers = true, IEnumerable<NetworkConnectionToClient> customObservers = null)
        {
            if (behaviour is null) throw new ArgumentNullException(nameof(behaviour));
            if (string.IsNullOrWhiteSpace(rpcName)) throw new ArgumentNullException(nameof(rpcName));

            if (writer != null)
            {
                var data = writer.ToArraySegment();
                
                SendRpc(behaviour, rpcName, rpcHash, data, channelId, includeOwner, checkObservers, customObservers);
            }
            else
            {
                SendRpc(behaviour, rpcName, rpcHash, (ArraySegment<byte>?)null, channelId, includeOwner, checkObservers, customObservers);
            }
        }

        public static void SendRpc(this NetworkBehaviour behaviour, string rpcName, int rpcHash, ArraySegment<byte>? data,
            int channelId = 0, bool includeOwner = true, bool checkObservers = true, IEnumerable<ExPlayer> customObservers = null)
        {
            if (behaviour is null) throw new ArgumentNullException(nameof(behaviour));
            if (string.IsNullOrWhiteSpace(rpcName)) throw new ArgumentNullException(nameof(rpcName));
            
            ApiLog.Debug("Mirror API", $"SendRpc &6{behaviour.name}&r &3{rpcName}&r (&6{rpcHash}&r)");

            var msg = new RpcMessage()
            {
                netId = behaviour.netId,
                componentIndex = behaviour.ComponentIndex,

                functionHash = (ushort)rpcHash
            };

            if (data.HasValue)
                msg.payload = data.Value;

            if (customObservers != null)
            {
                ApiLog.Debug("Mirror API", $"Sending to customObservers");
                
                foreach (var ply in customObservers)
                {
                    ApiLog.Debug("Mirror API", $"Checking out observer &1{ply.Nickname}&r (&6{ply.UserId}&r)");

                    if (checkObservers && !behaviour.netIdentity.observers.ContainsValue(ply.ClientConnection))
                    {
                        ApiLog.Debug("Mirror API", $"checkObservers is true and target is not included in observers");
                        continue;
                    }

                    if (!includeOwner && ply.Connection == behaviour.netIdentity.connectionToClient)
                    {
                        ApiLog.Debug("Mirror API", $"includeOwner is false and target connection is owner");
                        continue;
                    }

                    if (!ply.Connection.isReady)
                    {
                        ApiLog.Debug("Mirror API", $"target connection is not ready");    
                        continue;
                    }

                    ply.Connection.Send(msg, channelId);
                    
                    ApiLog.Debug("Mirror API", $"Sent RPC payload");    
                }
            }
            else
            {
                ApiLog.Debug("Mirror API", $"Sending to default observers ({behaviour.netIdentity.observers.Count})");    
                
                foreach (var conn in behaviour.netIdentity.observers.Values)
                {
                    ApiLog.Debug("Mirror API", $"Checking out connection id={conn.connectionId} ({conn})");    
                    
                    if (!includeOwner && conn == behaviour.netIdentity.connectionToClient)
                    {
                        ApiLog.Debug("Mirror API", $"includeOwner is false and target connection is owner");
                        continue;
                    }

                    if (!conn.isReady)
                    {
                        ApiLog.Debug("Mirror API", $"Target connection is not ready");    
                        continue;
                    }

                    conn.Send(msg, channelId);
                    
                    ApiLog.Debug("Mirror API", $"Sent RPC payload to connection");    
                }
            }
        }

        public static void SendRpc(this NetworkBehaviour behaviour, string rpcName, int rpcHash, ArraySegment<byte>? data, 
            int channelId = 0, bool includeOwner = true, bool checkObservers = true, IEnumerable<NetworkConnectionToClient> customObservers = null)
        {
            if (behaviour is null) throw new ArgumentNullException(nameof(behaviour));
            if (string.IsNullOrWhiteSpace(rpcName)) throw new ArgumentNullException(nameof(rpcName));

            var msg = new RpcMessage()
            {
                netId = behaviour.netId,
                componentIndex = behaviour.ComponentIndex,

                functionHash = (ushort)rpcHash
            };

            if (data.HasValue)
                msg.payload = data.Value;

            foreach (var conn in (customObservers ?? behaviour.netIdentity.observers.Values))
            {
                if (customObservers != null && checkObservers && !behaviour.netIdentity.observers.ContainsValue(conn)) continue;
                if (!includeOwner && conn == behaviour.netIdentity.connectionToClient) continue;
                if (!conn.isReady) continue;

                conn.Send(msg, channelId);
            }
        }

        public static void SendCustomSyncVar(this NetworkConnectionToClient connection, NetworkBehaviour behaviour, string propertyName, object customValue)
        {
            if (connection is null)
                throw new ArgumentNullException(nameof(connection));

            if (behaviour is null)
                throw new ArgumentNullException(nameof(behaviour));

            if (customValue is null)
                throw new ArgumentNullException(nameof(customValue));

            var type = customValue.GetType();

            if (!Writers.TryGetFirst(x => x.Item1 == type, out var syncWriter))
                throw new Exception($"No writers for type {type.FullName} were found");

            var bitMask = GetSyncVarBit(propertyName);

            var varWriter = NetworkWriterPool.Get();
            var segmentWriter = NetworkWriterPool.Get();

            WriteCustomSyncVars(varWriter, behaviour, bitMask, x => syncWriter.Item2(null, new object[] { x, customValue }), segmentWriter);
            
            NetworkWriterPool.Return(segmentWriter);

            var data = ReturnWriterAndData(varWriter);

            connection.Send(data);
        }

        public static void SendCustomSyncVar<T>(this NetworkConnectionToClient connection, NetworkBehaviour behaviour, string propertyName, T customValue)
        {
            if (connection is null)
                throw new ArgumentNullException(nameof(connection));

            if (behaviour is null)
                throw new ArgumentNullException(nameof(behaviour));

            var bitMask = GetSyncVarBit(propertyName);

            var varWriter = NetworkWriterPool.Get();
            var segmentWriter = NetworkWriterPool.Get();

            WriteCustomSyncVars(varWriter, behaviour, bitMask, x => x.Write(customValue), segmentWriter);
            NetworkWriterPool.Return(segmentWriter);

            var data = ReturnWriterAndData(varWriter);

            connection.Send(data);
        }

        public static void SendCustomSyncVars(this NetworkConnectionToClient connection, NetworkBehaviour behaviour, ulong bitMask, Action<NetworkWriter> writeSyncVars)
        {
            if (connection is null)
                throw new ArgumentNullException(nameof(connection));

            if (behaviour is null)
                throw new ArgumentNullException(nameof(behaviour));

            var varWriter = NetworkWriterPool.Get();
            var segmentWriter = NetworkWriterPool.Get();

            WriteCustomSyncVars(varWriter, behaviour, bitMask, writeSyncVars, segmentWriter);
            
            NetworkWriterPool.Return(segmentWriter);

            var data = ReturnWriterAndData(varWriter);

            connection.Send(data);
        }

        public static void WriteCustomSyncVars(this NetworkWriter writer, NetworkBehaviour behaviour, ulong bitMask, Action<NetworkWriter> writeSyncVars, NetworkWriterPooled segmentWriter = null)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            if (behaviour is null)
                throw new ArgumentNullException(nameof(behaviour));

            var wasNull = segmentWriter is null;

            if (segmentWriter is null)
                segmentWriter = NetworkWriterPool.Get();

            if (segmentWriter.Position != 0)
                segmentWriter.Reset();

            var identity = behaviour.netIdentity;
            var num = 0UL | 1UL << (identity.NetworkBehaviours.IndexOf(behaviour) & 31);

            Compression.CompressVarUInt(segmentWriter, num);

            var pos1 = segmentWriter.Position;

            segmentWriter.WriteByte(0);

            var pos2 = segmentWriter.Position;

            segmentWriter.WriteULong(bitMask);
            writeSyncVars.InvokeSafe(writer, true);

            var pos3 = segmentWriter.Position;

            segmentWriter.Position = pos1;

            var b = (byte)((pos3 - pos2) & 255);

            segmentWriter.WriteByte(b);
            segmentWriter.Position = pos3;

            writer.WriteMessageId<EntityStateMessage>();
            writer.WriteUInt(identity.netId);
            writer.WriteArraySegmentAndSize(segmentWriter.ToArraySegment());

            if (wasNull)
                NetworkWriterPool.Return(segmentWriter);
        }

        public static void WriteToConnection(this NetworkConnection connection, Action<NetworkWriter> customWriter, int channelId = 0)
        {
            if (connection is null)
                throw new ArgumentNullException(nameof(connection));

            if (customWriter is null)
                throw new ArgumentNullException(nameof(customWriter));

            if (!connection.isReady)
                throw new Exception($"Cannot write to a connection that is not ready");

            var data = WriteToSegment(customWriter);

            if (data.Count < 2) // Network Message ID size
                throw new Exception($"Attempted to send invalid data");

            connection.Send(data, channelId);
        }

        public static ArraySegment<byte> WriteMessageToData<T>(Action<NetworkWriter> customWriter, int channelId = 0) where T : struct, NetworkMessage
        {
            if (customWriter is null)
                throw new ArgumentNullException(nameof(customWriter));

            var writer = NetworkWriterPool.Get();

            writer.WriteUShort(NetworkMessageId<T>.Id);
            customWriter.InvokeSafe(writer, true);

            return ReturnWriterAndData(writer);
        }

        public static void WriteMessageToConnection<T>(this NetworkConnection connection, Action<NetworkWriter> customWriter, int channelId = 0) where T : struct, NetworkMessage
        {
            if (connection is null)
                throw new ArgumentNullException(nameof(connection));

            if (customWriter is null)
                throw new ArgumentNullException(nameof(customWriter));

            if (!connection.isReady)
                throw new Exception($"Cannot write to a connection that is not ready");

            var writer = NetworkWriterPool.Get();

            writer.WriteUShort(NetworkMessageId<T>.Id);
            customWriter.InvokeSafe(writer, true);

            var data = ReturnWriterAndData(writer);

            connection.Send(data, channelId);
        }

        public static void WriteMessageId<T>(this NetworkWriter writer) where T : struct, NetworkMessage
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteUShort(NetworkMessageId<T>.Id);
        }

        public static void WriteMessage<T>(this NetworkWriter writer, Action<NetworkWriter> customWriter) where T : struct, NetworkMessage
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            if (customWriter is null)
                throw new ArgumentNullException(nameof(customWriter));

            writer.WriteUShort(NetworkMessageId<T>.Id);
            customWriter.InvokeSafe(writer, true);
        }

        public static ArraySegment<byte> WriteToSegment(Action<NetworkWriter> customWriter)
        {
            if (customWriter is null)
                throw new ArgumentNullException(nameof(customWriter));

            var writer = NetworkWriterPool.Get();

            customWriter.InvokeSafe(writer, true);
            return ReturnWriterAndData(writer);
        }

        public static ArraySegment<byte> ReturnWriterAndData(NetworkWriterPooled writer)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            var data = writer.ToArraySegment();

            NetworkWriterPool.Return(writer);
            return data;
        }
    }
}