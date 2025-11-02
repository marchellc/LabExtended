using LabApi.Loader;

using LabExtended.Commands.Attributes;

using LabExtended.Core;
using LabExtended.Extensions;

namespace LabExtended.Commands.Custom.Reload;

public partial class ReloadCommand
{
    /// <summary>
    /// Reloads the specified plugin, optionally reloading only its configuration.
    /// </summary>
    /// <remarks>Use this method to refresh a plugin's configuration or to fully reload the plugin without
    /// restarting the application. If the specified plugin is not found among enabled plugins, the operation fails with
    /// an error message.</remarks>
    /// <param name="pluginName">The name of the plugin to reload. The name comparison is case-insensitive. Must refer to an enabled plugin.</param>
    /// <param name="onlyConfig">If <see langword="true"/>, only the plugin's configuration is reloaded; otherwise, the plugin is fully reloaded
    /// by disabling and re-enabling it.</param>
    [CommandOverload("plugin", "Reloads a specific plugin.", null)]
    public void PluginOverload(
        [CommandParameter("Name", "Name of the plugin to reload.")] string pluginName,
        [CommandParameter("OnlyConfig", "Whether or not to reload only the plugin's config.")] bool onlyConfig = false)
    {
        if (!PluginLoader.EnabledPlugins.TryGetFirst(x => string.Equals(x.Name, pluginName, StringComparison.InvariantCultureIgnoreCase), 
                out var plugin))
        {
            Fail($"Unknown plugin: {pluginName}");
            return;
        }

        try
        {
            plugin.LoadConfigs();

            if (!onlyConfig)
            {
                plugin.Disable();
                plugin.Enable();
            }
        }
        catch (Exception ex)
        {
            Fail($"Failed while reloading plugin '{plugin.Name}': {ex.Message}");
            
            ApiLog.Error("LabExtended", ex);
        }
        
        Ok($"Successfully reloaded plugin '{plugin.Name}'");
    }    
}