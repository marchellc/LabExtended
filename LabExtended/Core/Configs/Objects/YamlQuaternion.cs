using UnityEngine;

using YamlDotNet.Serialization;

namespace LabExtended.Core.Configs.Objects;

/// <summary>
/// Represents a YAML-serializable Quaternion.
/// </summary>
public class YamlQuaternion
{
    /// <summary>
    /// Gets a NEW instance of a <see cref="Quaternion.identity"/>.
    /// </summary>
    public static YamlQuaternion IdentityNew => new(Quaternion.identity);

    /// <summary>
    /// Gets the singleton instance of a <see cref="Quaternion.identity"/>.
    /// </summary>
    public static YamlQuaternion IdentitySingleton { get; } = new(Quaternion.identity);
    
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

    /// <summary>
    /// Creates a new <see cref="YamlQuaternion"/> instance.
    /// </summary>
    public YamlQuaternion()
    {
        
    }

    /// <summary>
    /// Creates a new <see cref="YamlQuaternion"/> instance.
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    public YamlQuaternion(Quaternion quaternion)
    {
        X = quaternion.x;
        Y = quaternion.y;
        Z = quaternion.z;
        W = quaternion.w;

        cachedQuart = quaternion;
    }
    
    /// <summary>
    /// Converts a <see cref="YamlQuaternion"/> to a <see cref="UnityEngine.Quaternion"/>.
    /// </summary>
    /// <param name="quaternion">The instance to convert.</param>
    /// <returns>The converted quaternion.</returns>
    public static implicit operator Quaternion(YamlQuaternion quaternion)
        => quaternion.Quaternion;
}