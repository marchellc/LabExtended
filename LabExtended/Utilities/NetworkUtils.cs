using Common.Extensions;

using LabExtended.API;
using LabExtended.API.Collections.Locked;

using Mirror;

using System.Reflection;
using System.Reflection.Emit;

namespace LabExtended.Utilities
{
    /// <summary>
    /// A class for mirror extensions.
    /// </summary>
    public static class NetworkUtils
    {
        internal static readonly LockedDictionary<Type, MethodInfo> _writerExtensions = new LockedDictionary<Type, MethodInfo>();
        internal static readonly LockedDictionary<string, ulong> _syncVars = new LockedDictionary<string, ulong>();
        internal static readonly LockedDictionary<string, string> _rpcNames = new LockedDictionary<string, string>();

        public static Action<NetworkIdentity, NetworkConnection> SendSpawnMessage { get; } = NetworkServer.SendSpawnMessage;
        public static Action<NetworkBehaviour, ulong> SetSyncVarDirtyBit { get; } = (behaviour, syncVar) => behaviour.SetSyncVarDirtyBit(syncVar);

        #region Loading
        internal static void LoadMirror()
        {
            LoadSyncVars();
            LoadRpcs();
            LoadExtensions();
        }

        private static void LoadSyncVars()
        {
            foreach (var type in typeof(ServerConsole).Assembly.GetTypes())
            {
                foreach (var property in type.GetAllProperties())
                {
                    if (!property.Name.StartsWith("Network"))
                        continue;

                    var setter = property.GetSetMethod(true);
                    var setterBody = setter?.GetMethodBody();

                    if (setterBody is null)
                        continue;

                    var propName = $"{property.ReflectedType.Name}.{property.Name}";
                    var setterCodes = setterBody.GetILAsByteArray();

                    _syncVars[propName] = setterCodes[setterCodes.LastIndexOf((byte)OpCodes.Ldc_I8.Value) + 1];
                }
            }
        }

        private static void LoadRpcs()
        {
            foreach (var type in typeof(ServerConsole).Assembly.GetTypes())
            {
                foreach (var method in type.GetAllMethods())
                {
                    if (!method.HasAttribute<ClientRpcAttribute>() && !method.HasAttribute<TargetRpcAttribute>())
                        continue;

                    var body = method.GetMethodBody();

                    if (body is null)
                        continue;

                    var codes = body.GetILAsByteArray();

                    _rpcNames[$"{method.ReflectedType.Name}.{method.Name}"] = method.Module.ResolveString(BitConverter.ToInt32(codes, codes.IndexOf((byte)OpCodes.Ldstr.Value) + 1));
                }
            }
        }

        private static void LoadExtensions()
        {
            var assembly = typeof(ServerConsole).Assembly;
            var generated = assembly.GetType("Mirror.GeneratedNetworkCode");

            foreach (var method in typeof(NetworkWriterExtensions).GetAllMethods().Where(x => !x.IsGenericMethod && x.GetCustomAttribute(typeof(ObsoleteAttribute)) == null && (x.GetParameters()?.Length == 2)))
                _writerExtensions[method.Parameters().First(x => x.ParameterType != typeof(NetworkWriter)).ParameterType] = method;

            foreach (var method in generated.GetAllMethods().Where(x => !x.IsGenericMethod && (x.GetParameters()?.Length == 2) && (x.ReturnType == typeof(void))))
                _writerExtensions[method.GetParameters().First(x => x.ParameterType != typeof(NetworkWriter)).ParameterType] = method;

            foreach (var serializer in assembly.GetTypes())
            {
                if (!serializer.Name.EndsWith("Serializer"))
                    continue;

                foreach (var method in serializer.GetAllMethods())
                {
                    if (method.ReturnType != typeof(void) || !method.Name.StartsWith("Write"))
                        continue;

                    _writerExtensions[method.Parameters().First(x => x.ParameterType != typeof(NetworkWriter)).ParameterType] = method;
                }
            }
        }
        #endregion

        public static int GetComponentIndex(this NetworkIdentity identity, Type type)
            => Array.FindIndex(identity.NetworkBehaviours, (x) => x.GetType() == type);

        public static void EditNetworkObject(this NetworkIdentity identity, Action<NetworkIdentity> customAction)
        {
            customAction.Invoke(identity);

            var objectDestroyMessage = new ObjectDestroyMessage()
            {
                netId = identity.netId,
            };

            foreach (var player in ExPlayer.Players)
            {
                player.Connection.Send(objectDestroyMessage, 0);
                SendSpawnMessage(identity, player.Connection);
            }
        }

        public static void MakeCustomSyncWriter(this NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncObject, Action<NetworkWriter> customSyncVar, NetworkWriter owner, NetworkWriter observer)
        {
            ulong value = 0;
            NetworkBehaviour behaviour = null;

            for (int i = 0; i < behaviorOwner.NetworkBehaviours.Length; i++)
            {
                if (behaviorOwner.NetworkBehaviours[i].GetType() == targetType)
                {
                    behaviour = behaviorOwner.NetworkBehaviours[i];
                    value = 1UL << (i & 31);
                    break;
                }
            }

            Compression.CompressVarUInt(owner, value);

            var position = owner.Position;
            owner.WriteByte(0);
            var position2 = owner.Position;

            if (customSyncObject is not null)
                customSyncObject(owner);
            else
                behaviour.SerializeObjectsDelta(owner);

            customSyncVar?.Invoke(owner);

            var position3 = owner.Position;

            owner.Position = position;
            owner.WriteByte((byte)(position3 - position2 & 255));
            owner.Position = position3;

            if (behaviour.syncMode != SyncMode.Observers)
                observer.WriteBytes(owner.ToArraySegment().Array, position, owner.Position - position);
        }
    }
}