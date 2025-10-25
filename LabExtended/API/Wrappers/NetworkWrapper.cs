using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using Mirror;

using RelativePositioning;

using UnityEngine;

namespace LabExtended.API.Wrappers
{
    /// <summary>
    /// Base class for wrappers for networked objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetworkWrapper<T> :
        Wrapper<T>,

        IMapObject,

        INetworkedPosition,
        INetworkedRotation

        where T : NetworkBehaviour
    {
        public NetworkWrapper(T baseValue) : base(baseValue) 
        { }

        /// <summary>
        /// Gets the NetworkIdentity of the wrapped object.
        /// </summary>
        public virtual NetworkIdentity Identity => Base.netIdentity;

        /// <summary>
        /// Gets the network ID of the wrapped object.
        /// </summary>
        public virtual uint NetId => Base.netId;

        /// <summary>
        /// Gets a value indicating whether the object is currently spawned on the network. 
        /// </summary>
        /// <remarks>An object is considered spawned if it has a non-null base object, a valid network
        /// identity, and its network ID is present in the server's spawned objects collection.</remarks>
        public bool IsSpawned => Base != null 
            && Base.netIdentity != null
            && NetworkServer.spawned.ContainsKey(NetId);

        /// <summary>
        /// Gets or sets the scale of the wrapped object.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the position of the wrapped object.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the rotation of the wrapped object.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the NetIdWaypoint associated with this networked object.
        /// </summary>
        public NetIdWaypoint? Waypoint
        {
            get => NetIdWaypoint.AllNetWaypoints.FirstOrDefault(x => x != null && x._targetNetId != null && x._targetNetId == Identity);
            set => value!._targetNetId = Identity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation, Vector3? scale = null)
        {
            Despawn();

            Base.transform.position = position;
            Base.transform.rotation = rotation;

            if (scale.HasValue)
                Base.transform.localScale = scale.Value;

            Spawn();
        }

        /// <summary>
        /// Despawns the object.
        /// </summary>
        public void Despawn()
        {
            if (Identity != null && Identity.gameObject != null)
                NetworkServer.UnSpawn(Identity.gameObject);
        }

        /// <summary>
        /// Despawns the object for the specified connection.
        /// </summary>
        /// <param name="connection">The connection to despawn the object for.</param>
        public void Despawn(NetworkConnection connection)
            => connection?.Send(new ObjectHideMessage() { netId = NetId });

        /// <summary>
        /// Despawns the object for the specified connections.
        /// </summary>
        /// <param name="connections">The connections to despawn the object for.</param>
        public void Despawn(IEnumerable<NetworkConnection> connections)
            => connections?.ForEach(Despawn);

        /// <summary>
        /// Spawns the object.
        /// </summary>
        public void Spawn()
        {
            if (Identity != null && Identity.gameObject != null)
                NetworkServer.Spawn(Identity.gameObject);
        }

        /// <summary>
        /// Sends a spawn message to the specified network connection, initializing the object with the provided
        /// transform properties.
        /// </summary>
        /// <remarks>The spawn message includes information about the object's identity, such as whether
        /// it is the local player or owned by the client, as well as its network ID. This method does not perform any
        /// action if the <paramref name="connection"/> is <see langword="null"/>.</remarks>
        /// <param name="connection">The network connection to which the spawn message will be sent. Cannot be <see langword="null"/>.</param>
        /// <param name="position">The optional position to initialize the object with. If <see langword="null"/>, the object's current
        /// position is used.</param>
        /// <param name="scale">The optional scale to initialize the object with. If <see langword="null"/>, the object's current scale is
        /// used.</param>
        /// <param name="rotation">The optional rotation to initialize the object with. If <see langword="null"/>, the object's current
        /// rotation is used.</param>
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

        /// <summary>
        /// Sends a spawn message to the specified connections.
        /// </summary>
        /// <param name="connections">The connections to send the spawn message to.</param>
        /// <param name="position">The position to send.</param>
        /// <param name="scale">The scale to send.</param>
        /// <param name="rotation">The rotation to send.</param>
        public void Spawn(IEnumerable<NetworkConnection> connections, Vector3? position = null, Vector3? scale = null, Quaternion? rotation = null)
            => connections?.ForEach(x => Spawn(x, position, scale, rotation));

        /// <summary>
        /// Destroys the object.
        /// </summary>
        public void Delete()
        {
            if (Identity != null && Identity.gameObject != null)
                NetworkServer.Destroy(Identity.gameObject);
        }

        /// <summary>
        /// Sends an object destroy message to the specified connection.
        /// </summary>
        /// <param name="connection">The connection to send the message to.</param>
        public void Delete(NetworkConnection connection)
            => connection?.Send(new ObjectDestroyMessage() { netId = NetId });

        /// <summary>
        /// Sends object destroy messages to the specified connections.
        /// </summary>
        /// <param name="connections">The connections to send the message to.</param>
        public void Delete(IEnumerable<NetworkConnection> connections)
            => connections?.ForEach(Delete);
    }
}