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

        public static (PrimitiveObjectToy startCube, PrimitiveObjectToy line, PrimitiveObjectToy endCube) SpawnTraceLine(
            Vector3 startPosition,
            Vector3 endPosition,

            Vector3 startCubeScale,
            Vector3 endCubeScale,

            PrimitiveFlags flags = PrimitiveFlags.Visible,
            PrimitiveType lineType = PrimitiveType.Capsule,

            float lineSize = 0.01f,

            Color? startCubeColor = null,
            Color? endCubeColor = null,
            Color? lineColor = null,

            bool isLineStatic = true,
            bool isStartCubeStatic = true,
            bool isEndCubeStatic = true)
        {
            var startColor = startCubeColor.HasValue ? startCubeColor.Value : Color.blue;
            var lineCol = lineColor.HasValue ? lineColor.Value : Color.blue;
            var endColor = endCubeColor.HasValue ? endCubeColor.Value : Color.blue;

            var startCube = SpawnPrimitive(startPosition, Quaternion.identity, startCubeScale, PrimitiveType.Cube, flags, startColor);
            var endCube = SpawnPrimitive(endPosition, Quaternion.identity, endCubeScale, PrimitiveType.Cube, flags, endColor);
            var line = SpawnLine(startPosition, endPosition, lineSize, lineCol, flags, lineType, true);

            startCube.NetworkIsStatic = true;
            endCube.NetworkIsStatic = true;

            return (startCube, endCube, line);
        }

        public static PrimitiveObjectToy SpawnLine(Vector3 startPosition, Vector3 endPosition, float size = 0.01f, Color? color = null, PrimitiveFlags flags = PrimitiveFlags.Visible, PrimitiveType type = PrimitiveType.Cylinder, bool isStatic = true)
        {
            var scale = new Vector3(size, Vector3.Distance(startPosition, endPosition) * (type is PrimitiveType.Cube ? 1f : 0.5f), size);
            var position = startPosition + (endPosition - startPosition) * 0.5f;
            var rotation = Quaternion.LookRotation(endPosition - startPosition) * Quaternion.Euler(90f, 0f, 0f);

            return SpawnPrimitive(position, rotation, scale, type, flags, color);
        }

        public static Color GlowColor(this Color color)
            => new Color(color.r * 50f, color.g * 50f, color.b * 50f, 0.1f);

        public static void FixColor(ref Color color)
        {
            var rValue = color.r;
            var gValue = color.g;
            var bValue = color.b;
            var aValue = color.a;

            var changed = false;

            if (rValue > 1f)
            {
                rValue = rValue / 255f;
                changed = true;
            }

            if (gValue > 1f)
            {
                gValue = gValue / 255f;
                changed = true;
            }

            if (bValue > 1f)
            {
                bValue = bValue / 255f;
                changed = true;
            }

            if (aValue > 1f)
            {
                aValue = aValue / 255f;
                changed = true;
            }

            if (changed)
                color = new Color(rValue, gValue, bValue, aValue);
        }
    }
}