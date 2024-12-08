using AdminToys;

using LabExtended.API.Toys;

using UnityEngine;

namespace LabExtended.Utilities
{
    public static class PrimitiveUtils
    {
        /*
        public static void UpdateTraceLine(PrimitiveToy startCube, PrimitiveToy endCube, PrimitiveToy line,
           Vector3 startPosition,
            Vector3 endPosition,

            Vector3 startCubeScale,
            Vector3 endCubeScale,

            float lineSize = 0.01f)
        {
            startCube.Position = startPosition;
            startCube.Scale = startCubeScale;

            endCube.Position = endPosition;
            endCube.Scale = endCubeScale;

            UpdateLine(line, startPosition, endPosition, lineSize);
        }

        public static (PrimitiveToy startCube, PrimitiveToy line, PrimitiveToy endCube) SpawnTraceLine(
            Vector3 startPosition,
            Vector3 endPosition,

            Vector3 startCubeScale,
            Vector3 endCubeScale,

            PrimitiveFlags flags = PrimitiveFlags.Visible,
            PrimitiveType lineType = PrimitiveType.Capsule,

            float lineSize = 0.01f,

            Color? startCubeColor = null,
            Color? endCubeColor = null,
            Color? lineColor = null)
        {
            var startColor = startCubeColor.HasValue ? startCubeColor.Value : Color.blue;
            var lineCol = lineColor.HasValue ? lineColor.Value : Color.blue;
            var endColor = endCubeColor.HasValue ? endCubeColor.Value : Color.blue;

            var startCube = PrimitiveToy.Spawn(startPosition, Quaternion.identity, startCubeScale, PrimitiveType.Cube, flags, startColor);
            var endCube = PrimitiveToy.Spawn(endPosition, Quaternion.identity, endCubeScale, PrimitiveType.Cube, flags, endColor);

            var line = SpawnLine(startPosition, endPosition, lineSize, lineCol, flags, lineType);

            return (startCube, endCube, line);
        }

        public static PrimitiveToy SpawnLine(Vector3 startPosition, Vector3 endPosition, float size = 0.01f, Color? color = null, PrimitiveFlags flags = PrimitiveFlags.Visible, PrimitiveType type = PrimitiveType.Cylinder)
        {
            var scale = new Vector3(size, Vector3.Distance(startPosition, endPosition) * (type is PrimitiveType.Cube ? 1f : 0.5f), size);
            var position = startPosition + (endPosition - startPosition) * 0.5f;
            var rotation = Quaternion.LookRotation(endPosition - startPosition) * Quaternion.Euler(90f, 0f, 0f);

            return PrimitiveToy.Spawn(position, rotation, scale, type, flags, color);
        }

        public static void UpdateLine(PrimitiveToy toy, Vector3 startPosition, Vector3 endPosition, float size = 0.01f)
        {
            var scale = new Vector3(size, Vector3.Distance(startPosition, endPosition) * (toy.Type is PrimitiveType.Cube ? 1f : 0.5f), size);
            var position = startPosition + (endPosition - startPosition) * 0.5f;
            var rotation = Quaternion.LookRotation(endPosition - startPosition) * Quaternion.Euler(90f, 0f, 0f);

            toy.Rotation = rotation;
            toy.Position = position;

            toy.Scale = scale;
        }
        */

        public static Color GlowColor(this Color color)
            => new Color(color.r * 50f, color.g * 50f, color.b * 50f, 0.1f);

        public static Color FixColor(this Color color)
        {
            FixColor(ref color);
            return color;
        }

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