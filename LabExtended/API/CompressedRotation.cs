using UnityEngine;

namespace LabExtended.API
{
    public struct CompressedRotation
    {
        public readonly ushort HorizontalAxis;
        public readonly ushort VerticalAxis;

        public CompressedRotation(ushort horizontal, ushort vertical)
        {
            HorizontalAxis = horizontal;
            VerticalAxis = vertical;
        }

        public CompressedRotation(Quaternion rotation)
            => Compress(rotation, out HorizontalAxis, out VerticalAxis);

        public CompressedRotation(Vector3 rotation)
            : this(new Quaternion(rotation.x, rotation.y, rotation.z, 0f)) { }

        public static (ushort horizontalAxis, ushort verticalAxis) Compress(Quaternion rotation)
        {
            Compress(rotation, out var horizontalAxis, out var verticalAxis);
            return (horizontalAxis, verticalAxis);
        }

        public static void Compress(Quaternion rotation, out ushort horizontalAxis, out ushort verticalAxis)
        {
            if (rotation.eulerAngles.z != 0f)
                rotation = Quaternion.LookRotation(rotation * Vector3.forward, Vector3.up);

            var outfHorizontal = rotation.eulerAngles.y;
            var outfVertical = -rotation.eulerAngles.x;

            if (outfVertical < -90f)
                outfVertical += 360f;
            else if (outfVertical > 270f)
                outfVertical -= 360f;

            horizontalAxis = (ushort)Mathf.RoundToInt(Mathf.Clamp(outfHorizontal, 0f, 360f) * (65535f / 360f));
            verticalAxis = (ushort)Mathf.RoundToInt((Mathf.Clamp(outfVertical, -88f, 88f) + 88f) * (65535 / 176f));
        }
    }
}