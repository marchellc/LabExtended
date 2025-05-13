using AdminToys;

using Footprinting;

using LabExtended.API.Wrappers;
using LabExtended.Attributes;
using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Toys
{
    /// <summary>
    /// Represents spawnable admin toys.
    /// </summary>
    public class AdminToy : NetworkWrapper<AdminToyBase>
    {
        /// <summary>
        /// Lookup table for ALL admin toys.
        /// </summary>
        public static Dictionary<AdminToyBase, AdminToy> Lookup { get; } = new();
        
        /// <summary>
        /// Tries to find a wrapper by it's base object.
        /// </summary>
        /// <param name="baseObject">The base object.</param>
        /// <param name="wrapper">The found wrapper instance.</param>
        /// <returns>true if the wrapper was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet(AdminToyBase baseObject, out AdminToy wrapper)
        {
            if (baseObject is null)
                throw new ArgumentNullException(nameof(baseObject));
            
            return Lookup.TryGetValue(baseObject, out wrapper);
        }
        
        /// <summary>
        /// Tries to find a wrapper by it's base object.
        /// </summary>
        /// <param name="baseObject">The base object.</param>
        /// <param name="wrapper">The found wrapper instance.</param>
        /// <returns>true if the wrapper was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet<T>(AdminToyBase baseObject, out T? wrapper) where T : AdminToy
        {
            if (baseObject is null)
                throw new ArgumentNullException(nameof(baseObject));

            if (Lookup.TryGetValue(baseObject, out var wrapperObj) && wrapperObj is T castWrapper)
            {
                wrapper = castWrapper;
                return true;
            }
            
            wrapper = null;
            return false;
        }

        /// <summary>
        /// Tries to find a specific wrapper.
        /// </summary>
        /// <param name="predicate">The predicate used to search.</param>
        /// <param name="wrapper">The found wrapper instance.</param>
        /// <returns>true if the wrapper was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet(Func<AdminToy, bool> predicate, out AdminToy? wrapper)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            foreach (var pair in Lookup)
            {
                if (!predicate(pair.Value))
                    continue;
                
                wrapper = pair.Value;
                return true;
            }

            wrapper = null;
            return false;
        }
        
        /// <summary>
        /// Tries to find a specific wrapper.
        /// </summary>
        /// <param name="predicate">The predicate used to search.</param>
        /// <param name="wrapper">The found wrapper instance.</param>
        /// <typeparam name="T">Type of the admin toy.</typeparam>
        /// <returns>true if the wrapper was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet<T>(Func<T, bool> predicate, out T? wrapper) where T : AdminToy
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            foreach (var pair in Lookup)
            {
                if (pair.Value is not T toy)
                    continue;
                
                if (!predicate(toy))
                    continue;
                
                wrapper = toy;
                return true;
            }

            wrapper = null;
            return false;
        }

        /// <summary>
        /// Gets a wrapper by it's base object.
        /// </summary>
        /// <param name="baseObject">The base object.</param>
        /// <returns>The found wrapper instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static AdminToy Get(AdminToyBase baseObject)
        {
            if (baseObject is null)
                throw new ArgumentNullException(nameof(baseObject));

            if (!Lookup.TryGetValue(baseObject, out var wrapper))
                throw new KeyNotFoundException($"Could not find a base object");

            return wrapper;
        }
        
        /// <summary>
        /// Gets a wrapper by it's base object.
        /// </summary>
        /// <param name="baseObject">The base object.</param>
        /// <returns>The found wrapper instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static T Get<T>(AdminToyBase baseObject) where T : AdminToy
        {
            if (baseObject is null)
                throw new ArgumentNullException(nameof(baseObject));

            if (!Lookup.TryGetValue(baseObject, out var wrapper))
                throw new KeyNotFoundException($"Could not find a base object");

            return (T)wrapper;
        }

        /// <summary>
        /// Gets a wrapper by a predicate.
        /// </summary>
        /// <param name="predicate">The predicate used to search.</param>
        /// <returns>The found wrapper instance if found, otherwise null.</returns>
        /// <exception cref="Exception"></exception>
        public static AdminToy? Get(Func<AdminToy, bool> predicate)
            => TryGet(predicate, out var wrapper) ? wrapper : null;

        /// <summary>
        /// Gets a wrapper by a predicate.
        /// </summary>
        /// <param name="predicate">The predicate used to search.</param>
        /// <typeparam name="T">Type of the admin toy.</typeparam>
        /// <returns>The found wrapper instance if found, otherwise null.</returns>
        /// <exception cref="Exception"></exception>
        public static T? Get<T>(Func<T, bool> predicate) where T : AdminToy
            => TryGet(predicate, out var wrapper) ? wrapper : null;

        internal AdminToy(AdminToyBase baseValue) : base(baseValue)
        {
            Lookup.Add(baseValue, this);
        }

        /// <summary>
        /// Gets the toy's transform.
        /// </summary>
        public Transform Transform => Base.transform;
        
        /// <summary>
        /// Gets the toy's game object.
        /// </summary>
        public GameObject GameObject => Base.gameObject;

        /// <summary>
        /// Gets the toy's name.
        /// </summary>
        public string Name => Base.CommandName;

        /// <summary>
        /// Gets or sets the toy's movement smoothing.
        /// </summary>
        public byte MovementSmoothing
        {
            get => Base.NetworkMovementSmoothing;
            set => Base.NetworkMovementSmoothing = value;
        }

        /// <summary>
        /// Disables / enables position & rotation synchronization.
        /// </summary>
        public bool IsStatic
        {
            get => Base.NetworkIsStatic;
            set => Base.NetworkIsStatic = value;
        }

        /// <summary>
        /// Gets the footprint of the player who spawned this toy.
        /// </summary>
        public Footprint Spawner
        {
            get => Base.SpawnerFootprint;
            set => Base.SpawnerFootprint = value;
        }

        /// <summary>
        /// Gets or sets the toy's position (<b>won't do anything if <see cref="IsStatic"/> is true</b>).
        /// </summary>
        public override Vector3 Position
        {
            get => Base.NetworkPosition;
            set => Base.NetworkPosition = Base.transform.position = value;
        }

        /// <summary>
        /// Gets or sets the toy's scale (<b>won't do anything if <see cref="IsStatic"/> is true</b>).
        /// </summary>
        public override Vector3 Scale
        {
            get => Base.NetworkScale;
            set => Base.NetworkScale = Base.transform.localScale = value;
        }

        /// <summary>
        /// Gets or sets the toy's rotation (<b>won't do anything if <see cref="IsStatic"/> is true</b>).
        /// </summary>
        public override Quaternion Rotation
        {
            get => Base.NetworkRotation;
            set => Base.NetworkRotation = Base.transform.rotation = value;
        }

        /// <summary>
        /// Gets or sets the toy's custom network parent.
        /// </summary>
        public NetworkIdentity? Parent
        {
            get
            {
                if (Base._clientParentId != 0 &&
                    NetworkServer.spawned.TryGetValue(Base._clientParentId, out var identity))
                    return identity;

                return null;
            }
            set
            {
                if (value is null)
                {
                    Base.transform.parent = null;
                    
                    Base._previousParent = Base.transform.parent;
                    Base._clientParentId = 0;
                    
                    Base.RpcChangeParent(0);
                }
                else
                {
                    Base._previousParent = Base.transform.parent;
                    Base.transform.parent = value.transform;
                    Base._clientParentId = value.netId;
                    
                    Base.RpcChangeParent(value.netId);
                }
            }
        }

        /// <summary>
        /// Sets the toy's position, rotation and scale (<b>won't do anything if <see cref="IsStatic"/> is true</b>).
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="scale">The scale to set.</param>
        public override void SetPositionAndRotation(Vector3 position, Quaternion rotation, Vector3? scale = null)
        {
            Position = position;
            Rotation = rotation;

            if (scale.HasValue && scale.Value != Scale)
                Scale = scale.Value;
        }

        /// <summary>
        /// Sets the toy's parent to a specific game object.
        /// <remarks>The targeted game object MUST have a NetworkIdentity component.</remarks>
        /// </summary>
        /// <param name="gameObject">The target game object.</param>
        /// <returns>true if the parent was changed</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SetParent(GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            if (!gameObject.TryFindComponent<NetworkIdentity>(out var identity))
                return false;

            if (Base._clientParentId == identity.netId)
                return false;

            Parent = identity;
            return true;
        }
        
        /// <summary>
        /// Sets the toy's parent to a specific behaviour.
        /// <remarks>The targeted behaviour MUST have a NetworkIdentity component.</remarks>
        /// </summary>
        /// <param name="behaviour">The target behaviour.</param>
        /// <returns>true if the parent was changed</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SetParent(MonoBehaviour behaviour)
        {
            if (behaviour == null)
                throw new ArgumentNullException(nameof(behaviour));

            if (!behaviour.gameObject.TryFindComponent<NetworkIdentity>(out var identity))
                return false;

            if (Base._clientParentId == identity.netId)
                return false;

            Parent = identity;
            return true;
        }
        
        /// <summary>
        /// Sets the toy's parent to a specific transform.
        /// <remarks>The targeted transform MUST have a NetworkIdentity component.</remarks>
        /// </summary>
        /// <param name="transform">The target transform.</param>
        /// <returns>true if the parent was changed</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SetParent(Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            if (!transform.gameObject.TryFindComponent<NetworkIdentity>(out var identity))
                return false;

            if (Base._clientParentId == identity.netId)
                return false;

            Parent = identity;
            return true;
        }
        
        /// <summary>
        /// Sets the toy's parent to a specific network behaviour.
        /// </summary>
        /// <param name="behaviour">The target behaviour.</param>
        /// <returns>true if the parent was changed</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SetParent(NetworkBehaviour behaviour)
        {
            if (behaviour == null)
                throw new ArgumentNullException(nameof(behaviour));

            if (Base._clientParentId == behaviour.netId)
                return false;

            Parent = behaviour.netIdentity;
            return true;
        }

        private static void OnCreated(AdminToyBase toy)
        {
            if (Lookup.ContainsKey(toy))
                return;

            if (toy is Scp079CameraToy scp079CameraToy)
            {
                _ = new CameraToy(scp079CameraToy);
                return;
            }

            if (toy is LightSourceToy lightSourceToy)
            {
                _ = new LightToy(lightSourceToy);
                return;
            }

            if (toy is ShootingTarget shootingTarget)
            {
                _ = new TargetToy(shootingTarget);
                return;
            }

            if (toy is PrimitiveObjectToy primitiveObjectToy)
            {
                _ = new PrimitiveToy(primitiveObjectToy);
                return;
            }

            if (toy is InvisibleInteractableToy invisibleInteractableToy)
            {
                _ = new InteractableToy(invisibleInteractableToy);
                return;
            }

            if (toy is AdminToys.CapybaraToy capybaraToy)
            {
                _ = new CapybaraToy(capybaraToy);
                return;
            }

            if (toy is AdminToys.SpeakerToy speakerToy)
            {
                _ = new SpeakerToy(speakerToy);
                return;
            }
            
            ApiLog.Error("LabExtended API", $"Spawned an unknown admin toy: &1{toy.GetType().FullName}&r");
        }

        private static void OnDestroyed(AdminToyBase toy)
        {
            if (!Lookup.TryGetValue(toy, out var wrapper))
                return;
            
            if (wrapper is IDisposable disposable)
                disposable.Dispose();
            
            Lookup.Remove(toy);
        }

        [LoaderInitialize(1)]
        private static void OnInit()
        {
            InternalEvents.OnRoundRestart += Lookup.Clear;

            AdminToyBase.OnAdded += OnCreated;
            AdminToyBase.OnRemoved += OnDestroyed;
        }
    }
}