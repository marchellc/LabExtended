using LabExtended.API.Images.Configs;

namespace LabExtended.Core.Configs.Sections;

/// <summary>
/// Configs targeting the image API.
/// </summary>
public class ImageSection
{
    /// <summary>
    /// Gets or sets custom image spawn points.
    /// </summary>
    public List<SpawnableImage> SpawnImages { get; set; } = new()
    {
        new()
        {
            Name = "example"
        }
    };
}