using AdminToys;

using Mirror;

using UnityEngine;

using LabExtended.API;

namespace LabExtended.Utilities
{
    public static class PrimitiveUtils
    {
        private static GameObject _lightSource;
        private static GameObject _primitiveObject;

        public static GameObject LightSourceToy => _lightSource ??= NetworkClient.prefabs.Values.First(p => p.TryGetComponent<AdminToyBase>(out var adminToyBase) && adminToyBase.CommandName is "LightSource");
        public static GameObject PrimitiveObject => _primitiveObject ??= NetworkClient.prefabs.Values.First(p => p.TryGetComponent<AdminToyBase>(out var adminToyBase) && adminToyBase.CommandName is "PrimitiveObject");

        public static LightSourceToy SpawnLight(Vector3 position, Vector3? scale = null)
        {
            var lightSource = UnityEngine.Object.Instantiate(LightSourceToy).GetComponent<LightSourceToy>();

            lightSource.SpawnerFootprint = ExPlayer.Host.Footprint;

            lightSource.transform.position = position;
            lightSource.transform.localScale = scale.HasValue ? scale.Value : Vector3.one;

            NetworkServer.Spawn(lightSource.gameObject);

            return lightSource;
        }

        public static PrimitiveObjectToy SpawnPrimitive(Vector3 position, Quaternion? rotation = null, Vector3? scale = null, PrimitiveType type = PrimitiveType.Capsule, PrimitiveFlags flags = PrimitiveFlags.Collidable | PrimitiveFlags.Visible, Color? color = null)
        {
            var primitiveObject = UnityEngine.Object.Instantiate(PrimitiveObject).GetComponent<PrimitiveObjectToy>();

            primitiveObject.SpawnerFootprint = ExPlayer.Host.Footprint;

            primitiveObject.NetworkPrimitiveType = type;
            primitiveObject.NetworkPrimitiveFlags = flags;
            primitiveObject.NetworkMaterialColor = color.HasValue ? color.Value : Color.gray;

            primitiveObject.transform.SetPositionAndRotation(position, rotation.HasValue ? rotation.Value : Quaternion.identity);
            primitiveObject.transform.localScale = scale.HasValue ? scale.Value : Vector3.one;

            primitiveObject.NetworkScale = primitiveObject.transform.localScale;

            NetworkServer.Spawn(primitiveObject.gameObject);

            return primitiveObject;
        }
    }
}