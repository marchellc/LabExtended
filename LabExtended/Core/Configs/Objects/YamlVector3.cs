using UnityEngine;

using YamlDotNet.Serialization;

namespace LabExtended.Core.Configs.Objects;

/// <summary>
/// Represents a YAML-serializable Vector3.
/// </summary>
public class YamlVector3
{
    private Vector3? cachedVector;
    
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public float X { get; set; } = 0f;
    
    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public float Y { get; set; } = 0f;
    
    /// <summary>
    /// Gets or sets the Z coordinate.
    /// </summary>
    public float Z { get; set; } = 0f;

    /// <summary>
    /// Gets the converted Unity Vector3.
    /// </summary>
    [YamlIgnore]
    public Vector3 Vector
    {
        get
        {
            if (!cachedVector.HasValue || cachedVector.Value.x != X || cachedVector.Value.y != Y ||
                cachedVector.Value.z != Z)
                cachedVector = new(X, Y, Z);

            return cachedVector.Value;
        }
    }

    /// <summary>
    /// Creates a new <see cref="YamlVector3"/> instance.
    /// </summary>
    public YamlVector3()
    {
        
    }

    /// <summary>
    /// Creates a new <see cref="YamlVector3"/> instance.
    /// </summary>
    /// <param name="vector">The source Vector3.</param>
    public YamlVector3(Vector3 vector)
    {
        X = vector.x;
        Y = vector.y;
        Z = vector.z;

        cachedVector = vector;
    }

    /// <summary>
    /// Creates a new <see cref="YamlVector3"/> instance.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    public YamlVector3(float x, float y = 0f, float z = 0f)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Converts a <see cref="YamlVector3"/> instance to a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="vector">The instance to convert.</param>
    /// <returns>The converted Vector3.</returns>
    public static implicit operator Vector3(YamlVector3 vector)
        => vector.Vector;
}