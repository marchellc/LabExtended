using LabExtended.Core.Configs.Objects;

namespace LabExtended.API.Images.Configs;

/// <summary>
/// Represents a config option of a spawnable image.
/// </summary>
public class SpawnableImage
{
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

    /// <summary>
    /// Gets or sets the image chances.
    /// </summary>
    public Dictionary<string, float> Chances { get; set; } = new()
    {
        ["example"] = 0f,
        ["example_two"] = 0f
    };
}