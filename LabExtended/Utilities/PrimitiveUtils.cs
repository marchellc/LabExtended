using AdminToys;

using LabExtended.API.Toys;

using UnityEngine;

namespace LabExtended.Utilities
{
    public static class PrimitiveUtils
    {
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

            var startCube = new PrimitiveToy(PrimitiveType.Cube, flags)
            {
                Color = startColor,
                Scale = startCubeScale
            };

            var endCube = new PrimitiveToy(PrimitiveType.Cube, flags)
            {
                Color = endColor,
                Scale = endCubeScale
            };

            var line = SpawnLine(startPosition, endPosition, lineSize, lineCol, flags, lineType);
            return (startCube, endCube, line);
        }

        public static PrimitiveToy SpawnLine(Vector3 startPosition, Vector3 endPosition, float size = 0.01f, Color? color = null, PrimitiveFlags flags = PrimitiveFlags.Visible, PrimitiveType type = PrimitiveType.Cylinder)
        {
            var scale = new Vector3(size, Vector3.Distance(startPosition, endPosition) * (type is PrimitiveType.Cube ? 1f : 0.5f), size);
            var position = startPosition + (endPosition - startPosition) * 0.5f;
            var rotation = Quaternion.LookRotation(endPosition - startPosition) * Quaternion.Euler(90f, 0f, 0f);

            return new PrimitiveToy(type, flags)
            {
                Rotation = rotation,
                Position = position,
                Scale = scale,
                
                Color = color ?? Color.white
            };
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
    }
}