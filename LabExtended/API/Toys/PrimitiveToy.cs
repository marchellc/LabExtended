using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Toys
{
    /// <summary>
    /// Represents a primitive toy.
    /// </summary>
    public class PrimitiveToy : AdminToy, IWrapper<PrimitiveObjectToy>
    {
        /// <summary>
        /// Spawns a new PrimitiveToy.
        /// </summary>
        /// <param name="type">Type of the primitive object.</param>
        /// <param name="flags">Primitive flags.</param>
        /// <exception cref="Exception"></exception>
        public PrimitiveToy(
            PrimitiveType type = PrimitiveType.Capsule, 
            PrimitiveFlags flags = PrimitiveFlags.Visible | PrimitiveFlags.Collidable) 
            : base(PrefabList.Primitive.CreateInstance().GetComponent<AdminToyBase>())
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            Base = base.Base as PrimitiveObjectToy;
#pragma warning restore CS8601 // Possible null reference assignment.
            
            if (Base is null)
                throw new Exception($"Failed to spawn PrimitiveObjectToy");

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Base.SpawnerFootprint = ExPlayer.Host.Footprint;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            Base.PrimitiveType = type;
            Base.PrimitiveFlags = flags;

            NetworkServer.Spawn(Base.gameObject);
        }
        
        internal PrimitiveToy(PrimitiveObjectToy baseValue) : base(baseValue)
            => Base = baseValue;

        /// <summary>
        /// Gets the primitive toy base.
        /// </summary>
        public new PrimitiveObjectToy Base { get; }

        /// <summary>
        /// Gets or sets the primitive's color.
        /// </summary>
        public Color Color
        {
            get => Base.MaterialColor;
            set => Base.NetworkMaterialColor = value;
        }

        /// <summary>
        /// Gets or sets the primitive's type.
        /// </summary>
        public PrimitiveType Type
        {
            get => Base.PrimitiveType;
            set => Base.NetworkPrimitiveType = value;
        }

        /// <summary>
        /// Gets or sets the primitive's flags.
        /// </summary>
        public PrimitiveFlags Flags
        {
            get => Base.PrimitiveFlags;
            set => Base.NetworkPrimitiveFlags = value;
        }
    }
}