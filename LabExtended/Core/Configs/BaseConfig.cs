using System.ComponentModel;

using LabExtended.Patches.Functions.Players;
using LabExtended.Utilities;

namespace LabExtended.Core.Configs;

/// <summary>
/// The base config of LabExtended
/// </summary>
public class BaseConfig
{
    /// <summary>
    /// Whether or not to show debug messages.
    /// </summary>
    [Description("Toggles logging of debug messages.")]
    public bool DebugEnabled { get; set; }

    /// <summary>
    /// Whether or not to show transpiler debug logs (this includes transpilers of other plugins).
    /// </summary>
    [Description("Toggles debug logs of transpilers.")]
    public bool TranspilerDebugEnabled { get; set; }

    /// <summary>
    /// Whether or not to add true color tags to logs.
    /// </summary>
    [Description("Toggles true color log formatting.")]
    public bool TrueColorEnabled { get; set; } = true;

    /// <summary>
    /// Whether or not to disable round lock once the player who enabled it leaves the server.
    /// </summary>
    [Description("Whether or not to disable Round Lock when the player who enabled it leaves.")]
    public bool DisableRoundLockOnLeave { get; set; } = true;

    /// <summary>
    /// Whether or not to disable lobby lock once the player who enabled it leaves the server.
    /// </summary>
    [Description("Whether or not to disable Lobby Lock when the player who enabled it leaves.")]
    public bool DisableLobbyLockOnLeave { get; set; } = true;

    /// <summary>
    /// Whether or not to unload all plugins once the server's process quits.
    /// </summary>
    [Description("Whether or not to unload all plugins once the server's process quits.")]
    public bool UnloadPluginsOnQuit { get; set; } = true;

    /// <summary>
    /// The maximum distance between a disarmer and a disarmed player before being automatically uncuffed.
    /// </summary>
    [Description("The maximum distance between a disarmer and a disarmed player before being automatically uncuffed.")]
    public float RemoveDisarmRange
    {
        get => DisarmValidateEntryPatch.DisarmDistance;
        set => DisarmValidateEntryPatch.DisarmDistance = value;
    }

    /// <summary>
    /// A list of named positions plugins can use to spawn objects consistently across map seeds.
    /// </summary>
    [Description("A list of named positions plugins can use to spawn objects consistently across map seeds.")]
    public Dictionary<string, MapUtilities.NamedPosition> Positions { get; set; } = new()
    {
        ["example"] = new(),
        ["example2"] = new(),
    };

    /// <summary>
    /// A list of source names which will not be logged using debug logs.
    /// </summary>
    [Description("Sets a list of sources that cannot send debug messages.")]
    public List<string> DisabledDebugSources { get; set; } = new();
}