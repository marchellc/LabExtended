using LabExtended.API.Collections.Locked;
using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Wrappers
{
    public class NetworkWrapper<T> :
        Wrapper<T>,

        IMapObject,

        INetworkedPosition,
        INetworkedRotation

        where T : NetworkBehaviour
    {
        public NetworkWrapper(T baseValue) : base(baseValue) { }

        public virtual NetworkIdentity Identity => Base.netIdentity;
        public virtual uint NetId => Base.netId;

        public bool IsSpawned => Base != null && Base.netIdentity != null && NetworkServer.spawned.ContainsKey(NetId);

        public virtual Vector3 Scale
        {
            get => Base.transform.localScale;
            set
            {
                if (Base.transform.localScale == value)
                    return;

                Despawn();

                Base.transform.localScale = value;

                Spawn();
            }
        }

        public virtual Vector3 Position
        {
            get => Base.transform.position;
            set
            {
                Despawn();

                Base.transform.position = value;

                Spawn();
            }
        }

        public virtual Quaternion Rotation
        {
            get => Base.transform.rotation;
            set
            {
                Despawn();

                Base.transform.rotation = value;

                Spawn();
            }
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, Vector3? scale = null)
        {
            Despawn();

            Base.transform.position = position;
            Base.transform.rotation = rotation;

            if (scale.HasValue)
                Base.transform.localScale = scale.Value;

            Spawn();
        }

        public void Despawn()
            => NetworkServer.UnSpawn(Identity.gameObject);

        public void Despawn(NetworkConnection connection)
            => connection?.Send(new ObjectHideMessage() { netId = NetId });

        public void Despawn(IEnumerable<NetworkConnection> connections)
            => connections.ForEach(Despawn);

        public void Spawn()
            => NetworkServer.Spawn(Identity.gameObject);

        public void Spawn(NetworkConnection connection, Vector3? position = null, Vector3? scale = null, Quaternion? rotation = null)
            => connection?.Send(new SpawnMessage()
            {
                assetId = 0,
                sceneId = 0,

                isLocalPlayer = Identity.isLocalPlayer,
                isOwner = Identity.isOwned,

                position = position.HasValue ? position.Value : Identity.transform.position,
                rotation = rotation.HasValue ? rotation.Value : Identity.transform.rotation,

                scale = scale.HasValue ? scale.Value : Identity.transform.localScale,

                netId = NetId
            });

        public void Spawn(IEnumerable<NetworkConnection> connections, Vector3? position = null, Vector3? scale = null, Quaternion? rotation = null)
            => connections.ForEach(x => Spawn(x, position, scale, rotation));

        public void Delete()
            => NetworkServer.Destroy(Identity.gameObject);

        public void Delete(NetworkConnection connection)
            => connection?.Send(new ObjectDestroyMessage() { netId = NetId });

        public void Delete(IEnumerable<NetworkConnection> connections)
            => connections.ForEach(Delete);

        public virtual void OnDestroyed() { }
    }
}