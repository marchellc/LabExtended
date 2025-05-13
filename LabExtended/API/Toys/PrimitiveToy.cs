using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;

using Mirror;

using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8601 // Possible null reference assignment.

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
        /// <param name="position">Primitive spawn position.</param>
        /// <param name="rotation">Primitive spawn rotation.</param>
        /// <param name="type">Type of the primitive object.</param>
        /// <param name="flags">Primitive flags.</param>
        /// <exception cref="Exception"></exception>
        public PrimitiveToy(Vector3? position = null, Quaternion? rotation = null,
            PrimitiveType type = PrimitiveType.Capsule, 
            PrimitiveFlags flags = PrimitiveFlags.Visible | PrimitiveFlags.Collidable) 
            : base(PrefabList.Primitive.CreateInstance().GetComponent<AdminToyBase>())
        {
            Base = base.Base as PrimitiveObjectToy;
            
            if (Base is null)
                throw new Exception($"Failed to spawn PrimitiveObjectToy");
            
            Base.SpawnerFootprint = ExPlayer.Host.Footprint;

            Base.NetworkPosition = position ?? Vector3.zero;
            Base.NetworkRotation = rotation ?? Quaternion.identity;
            
            Base.PrimitiveType = type;
            Base.PrimitiveFlags = flags;
            
            Base.transform.SetPositionAndRotation(Base.NetworkPosition, Base.NetworkRotation);

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