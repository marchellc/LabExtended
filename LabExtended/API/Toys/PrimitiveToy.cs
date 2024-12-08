using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.Extensions;
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

        public static PrimitiveToy Spawn(Vector3 position, Action<PrimitiveToy> configure = null)
        {
            var primitiveObject = UnityEngine.Object.Instantiate(Prefabs.PrefabList.Primitive.GameObject).GetComponent<PrimitiveObjectToy>();

            primitiveObject.SpawnerFootprint = ExPlayer.Host.Footprint;

            primitiveObject.PrimitiveType = PrimitiveType.Capsule;
            primitiveObject.PrimitiveFlags = PrimitiveFlags.Visible | PrimitiveFlags.Collidable;

            primitiveObject.MaterialColor = Color.red;

            primitiveObject.transform.SetPositionAndRotation(position, Quaternion.identity);

            primitiveObject.Scale = primitiveObject.transform.localScale;

            var toy = new PrimitiveToy(primitiveObject);

            configure.InvokeSafe(toy);

            ExMap._toys.Add(toy);

            NetworkServer.Spawn(primitiveObject.gameObject);
            return toy;
        }
    }
}