using Mirror;

using AdminToys;

using UnityEngine;

namespace LabExtended.Utilities
{
    public static class PrimitiveUtils
    {
        public static GameObject PrimitivePrefab { get; } = NetworkClient.prefabs.FirstOrDefault(p => p.Value.GetComponent<PrimitiveObjectToy>() != null).Value;

        public static PrimitiveObjectToy Spawn(Vector3 position, Vector3 scale, Quaternion rotatino, PrimitiveType type = PrimitiveType.Capsule, PrimitiveFlags flags = PrimitiveFlags.Collidable | PrimitiveFlags.Visible, Color? color = null)
        {
            var toy = UnityEngine.Object.Instantiate(PrimitivePrefab).GetComponent<PrimitiveObjectToy>();

            NetworkServer.Spawn(toy.gameObject);

            toy.NetworkPosition = position;
            toy.NetworkRotation = new LowPrecisionQuaternion(rotatino);
            toy.NetworkScale = scale;

            toy.NetworkPrimitiveType = type;
            toy.NetworkPrimitiveFlags = flags;

            if (color.HasValue)
                toy.NetworkMaterialColor = color.Value;

            toy.NetworkIsStatic = true;
            return toy;
        }
    }
}