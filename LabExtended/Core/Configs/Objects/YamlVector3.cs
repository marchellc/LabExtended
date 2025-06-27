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
}