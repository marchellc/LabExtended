using LabExtended.Core.Configs.Objects;

using UnityEngine;

namespace LabExtended.API.Images.Configs;

/// <summary>
/// Represents a config option of a spawnable image.
/// </summary>
public class SpawnableImage
{
    /// <summary>
    /// Gets or sets the image spawn chance.
    /// </summary>
    public float Chance { get; set; } = 0f;

    /// <summary>
    /// Gets or sets the name of the image.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the spawn position.
    /// </summary>
    public YamlVector3 Position { get; set; } = new()
    {
        X = 1f,
        Y = 1f,
        Z = 1f
    };

    /// <summary>
    /// Gets or sets the spawn rotation.
    /// </summary>
    public YamlQuaternion Rotation { get; set; } = new()
    {
        X = 0f,
        Y = 0f,
        Z = 0f,
        W = 0f
    };
}