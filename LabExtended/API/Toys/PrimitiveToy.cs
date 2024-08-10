using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.Utilities.Values;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Toys
{
    public class PrimitiveToy : AdminToy, IWrapper<PrimitiveObjectToy>
    {
        public PrimitiveToy(PrimitiveObjectToy baseValue) : base(baseValue)
        {
            Base = baseValue;
            Flags = new EnumValue<PrimitiveFlags>(() => baseValue.NetworkPrimitiveFlags, flags => baseValue.NetworkPrimitiveFlags = flags);
        }

        public new PrimitiveObjectToy Base { get; }

        public EnumValue<PrimitiveFlags> Flags { get; }

        public Color Color
        {
            get => Base.NetworkMaterialColor;
            set => Base.NetworkMaterialColor = value;
        }

        public PrimitiveType Type
        {
            get => Base.NetworkPrimitiveType;
            set => Base.NetworkPrimitiveType = value;
        }

        public static PrimitiveToy Spawn(Vector3 position, Quaternion? rotation = null, Vector3? scale = null, PrimitiveType type = PrimitiveType.Capsule, PrimitiveFlags flags = PrimitiveFlags.Collidable | PrimitiveFlags.Visible, Color? color = null)
        {
            var primitiveObject = UnityEngine.Object.Instantiate(Prefabs.PrimitiveObject).GetComponent<PrimitiveObjectToy>();

            primitiveObject.SpawnerFootprint = ExPlayer.Host.Footprint;

            primitiveObject.NetworkPrimitiveType = type;
            primitiveObject.NetworkPrimitiveFlags = flags;
            primitiveObject.NetworkMaterialColor = color.HasValue ? color.Value : Color.gray;

            primitiveObject.transform.SetPositionAndRotation(position, rotation.HasValue ? rotation.Value : Quaternion.identity);
            primitiveObject.transform.localScale = scale.HasValue ? scale.Value : Vector3.one;

            primitiveObject.NetworkScale = primitiveObject.transform.localScale;

            var toy = new PrimitiveToy(primitiveObject);

            ExMap._toys.Add(toy);

            NetworkServer.Spawn(primitiveObject.gameObject);
            return toy;
        }
    }
}