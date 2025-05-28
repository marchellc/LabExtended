using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections;

/// <summary>
/// File storage configuration.
/// </summary>
public class StorageSection
{
    /// <summary>
    /// Gets or sets the directory.
    /// </summary>
    [Description("Path to the storage directory.")]
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether or not file storage should be enabled.
    /// </summary>
    [Description("Enables or disables the file storage.")]
    public bool IsEnabled { get; set; } = true;
}