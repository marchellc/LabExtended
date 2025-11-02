using LabExtended.Utilities;

namespace LabExtended.Core;

/// <summary>
/// Loader version handler.
/// </summary>
public static class ApiVersion
{
    /// <summary>
    /// The major version part.
    /// </summary>
    public const int Major = 1;

    /// <summary>
    /// The minor version part.
    /// </summary>
    public const int Minor = 2;

    /// <summary>
    /// The build version part.
    /// </summary>
    public const int Build = 0;

    /// <summary>
    /// The patch version part.
    /// </summary>
    public const int Patch = 0;

    /// <summary>
    /// Gets the loader's current version.
    /// </summary>
    public static Version Version { get; } = new(Major, Minor, Build, Patch);

    /// <summary>
    /// Gets the game's current version.
    /// </summary>
    public static Version Game { get; } =
        new(GameCore.Version.Major, GameCore.Version.Minor, GameCore.Version.Revision);

    /// <summary>
    /// Gets the loader's game version compatibility.
    /// </summary>
    public static VersionRange? Compatibility { get; } = new VersionRange(new Version(14, 2, 1));

    /// <summary>
    /// Checks for server version compatibility.
    /// </summary>
    /// <returns>true if this loader version is compatible with this server version.</returns>
    public static bool CheckCompatibility()
    {
        if (!Compatibility.HasValue || Compatibility.Value.InRange(Game)) 
            return true;

        ApiLog.Error("LabExtended",
            $"Attempted to load for an unsupported game version (&1{Game}&r) - supported: &2{Compatibility.Value}&r");
        return false;
    }
}