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
    /// Gets or sets the path to the parent directory for player storage.
    /// </summary>
    [Description("Sets the path to the parent directory for player storage.")]
    public string PlayerPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the directory of the shared storage.
    /// </summary>
    [Description("Path to the shared storage directory.")]
    public string SharedPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the directory of the server's storage.
    /// </summary>
    [Description("Sets the directory of the server's storage.")]
    public string ServerPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a collection of custom paths for storage instances, identified by name.
    /// </summary>
    [Description("Sets custom paths for storage instances by name.")]
    public Dictionary<string, string> CustomPaths { get; set; } = new();
}