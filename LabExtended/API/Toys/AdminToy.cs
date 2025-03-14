﻿using AdminToys;

using Footprinting;

using LabExtended.API.Wrappers;
using LabExtended.Attributes;
using LabExtended.Events;

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
        
        internal AdminToy(AdminToyBase baseValue) : base(baseValue) { Lookup.Add(baseValue, this); }

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
        
        internal static AdminToy Create(AdminToyBase adminToy)
        {
            if (adminToy is null)
                throw new ArgumentNullException(nameof(adminToy));

            var wrapperToy = adminToy switch
            {
                LightSourceToy lightSource => new LightToy(lightSource),
                ShootingTarget shootingTarget => new TargetToy(shootingTarget),
                PrimitiveObjectToy primitiveObject => new PrimitiveToy(primitiveObject),
                
                AdminToys.SpeakerToy speakerToy => new SpeakerToy(speakerToy),

                _ => new AdminToy(adminToy)
            };
            
            return wrapperToy;
        }

        [LoaderInitialize(1)]
        private static void OnInit()
        {
            InternalEvents.OnRoundRestart += Lookup.Clear;
        }
    }
}