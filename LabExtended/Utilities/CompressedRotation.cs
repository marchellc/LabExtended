using UnityEngine;

namespace LabExtended.Utilities
{
    /// <summary>
    /// Represents a rotation compressed into two 16-bit unsigned integer axes.
    /// </summary>
    public struct CompressedRotation
    {
        /// <summary>
        /// Represents the value of the horizontal axis for this instance.
        /// </summary>
        public readonly ushort HorizontalAxis;

        /// <summary>
        /// Represents the value of the vertical axis for this instance.
        /// </summary>
        public readonly ushort VerticalAxis;

        /// <summary>
        /// Initializes a new instance of the CompressedRotation structure with the specified horizontal and vertical
        /// axis values.
        /// </summary>
        /// <param name="horizontal">The value representing the horizontal axis rotation, typically encoded as an unsigned 16-bit integer.</param>
        /// <param name="vertical">The value representing the vertical axis rotation, typically encoded as an unsigned 16-bit integer.</param>
        public CompressedRotation(ushort horizontal, ushort vertical)
        {
            HorizontalAxis = horizontal;
            VerticalAxis = vertical;
        }

        /// <summary>
        /// Initializes a new instance of the CompressedRotation structure by compressing the specified quaternion
        /// rotation.
        /// </summary>
        /// <remarks>Use this constructor to create a compressed representation of a rotation for
        /// efficient storage or transmission. The input quaternion should be normalized to ensure accurate compression
        /// and decompression.</remarks>
        /// <param name="rotation">The quaternion representing the rotation to be compressed. Must be a normalized quaternion.</param>
        public CompressedRotation(Quaternion rotation)
            => Compress(rotation, out HorizontalAxis, out VerticalAxis);

        /// <summary>
        /// Initializes a new instance of the CompressedRotation structure using the specified Euler rotation vector.
        /// </summary>
        /// <remarks>This constructor converts the provided Euler angles to a quaternion with a zero
        /// w-component before compression. Use this overload when you have rotation data in Euler format rather than
        /// quaternion format.</remarks>
        /// <param name="rotation">A Vector3 representing the rotation in Euler angles, in degrees, to be compressed.</param>
        public CompressedRotation(Vector3 rotation)
            : this(new Quaternion(rotation.x, rotation.y, rotation.z, 0f)) { }

        /// <summary>
        /// Compresses a quaternion rotation into two 16-bit axis values for efficient storage or transmission.
        /// </summary>
        /// <param name="rotation">The quaternion representing the rotation to compress. Must be a normalized quaternion.</param>
        /// <returns>A tuple containing the compressed horizontal and vertical axis values as unsigned 16-bit integers.</returns>
        public static (ushort horizontalAxis, ushort verticalAxis) Compress(Quaternion rotation)
        {
            Compress(rotation, out var horizontalAxis, out var verticalAxis);
            return (horizontalAxis, verticalAxis);
        }

        /// <summary>
        /// Converts a rotation represented by a Quaternion into two compressed 16-bit axis values suitable for
        /// efficient storage or transmission.
        /// </summary>
        /// <param name="rotation">The rotation to compress, represented as a Quaternion. The Z component of the Euler angles is ignored; only
        /// the horizontal (Y) and vertical (X) axes are encoded.</param>
        /// <param name="horizontalAxis">When this method returns, contains the compressed horizontal axis value, corresponding to the Y component of
        /// the rotation, in the range 0 to 65535.</param>
        /// <param name="verticalAxis">When this method returns, contains the compressed vertical axis value, corresponding to the X component of
        /// the rotation, in the range 0 to 65535.</param>
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