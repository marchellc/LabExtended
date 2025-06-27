using UnityEngine;

using YamlDotNet.Serialization;

namespace LabExtended.Core.Configs.Objects;

/// <summary>
/// Represents a YAML-serializable Quaternion.
/// </summary>
public class YamlQuaternion
{
    private Quaternion? cachedQuart;
    
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
    /// Gets or sets the W coordinate.
    /// </summary>
    public float W { get; set; } = 0f;

    /// <summary>
    /// Gets the converted Unity Quaternion.
    /// </summary>
    [YamlIgnore]
    public Quaternion Quaternion
    {
        get
        {
            if (!cachedQuart.HasValue || cachedQuart.Value.x != X || cachedQuart.Value.y != Y ||
                cachedQuart.Value.z != Z || cachedQuart.Value.w != W)
                cachedQuart = new(X, Y, Z, W);

            return cachedQuart.Value;
        }
    }
}