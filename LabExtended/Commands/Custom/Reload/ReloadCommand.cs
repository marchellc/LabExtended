using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Reload;

/// <summary>
/// Represents a server-side command that reloads a specified system, such as game configuration, plugin configuration,
/// plugins, or an individual plugin.
/// </summary>
/// <remarks>Use this command to apply changes made to configuration files or plugins without restarting the
/// server. The command targets the specified system and reloads its state to reflect any updates.</remarks>
[Command("reload", "Reloads a specific system (gameconfig, pluginconfig, plugins, plugin)")]
public partial class ReloadCommand : CommandBase, IServerSideCommand { }