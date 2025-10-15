using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections;

/// <summary>
/// File storage configuration.
/// </summary>
public class StorageSection
{
    /// <summary>
    /// Whether or not file storage should be enabled.
    /// </summary>
    [Description("Enables or disables the file storage.")]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Whether or not a storage instance should be automatically loaded for each player with Do Not Track disabled.
    /// </summary>
    [Description("Whether or not a storage instance should be automatically loaded for each player with Do Not Track disabled.")]
    public bool LoadPlayerStorage { get; set; }

    /// <summary>
    /// Gets or sets the directory of the shared storage.
    /// </summary>
    [Description("Path to the storage directory.")]
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a collection of custom paths for storage instances, identified by name.
    /// </summary>
    [Description("Sets custom paths for storage instances by name.")]
    public Dictionary<string, string> CustomPaths { get; set; } = new();
}