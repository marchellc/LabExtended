using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Reload;

[Command("reload", "Reloads a specific system (gameconfig, pluginconfig, plugins, plugin)")]
public partial class ReloadCommand : CommandBase, IServerSideCommand { }