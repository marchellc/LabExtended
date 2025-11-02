using LabApi.Loader;

using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Reload;

public partial class ReloadCommand
{
    /// <summary>
    /// Reloads all enabled plugins, optionally reloading only their configuration files.
    /// </summary>
    /// <remarks>If onlyConfig is set to false, each plugin is fully reloaded, which may cause temporary
    /// unavailability or reset plugin state. If set to true, only the configuration files are reloaded, and plugin
    /// state is preserved.</remarks>
    /// <param name="onlyConfig">true to reload only the configuration files for each plugin; false to fully reload each plugin by disabling and
    /// re-enabling it.</param>
    [CommandOverload("plugins", "Reloads all plugins.", null)]
    public void PluginsOverload( 
        [CommandParameter("OnlyConfig", "Whether or not to reload only the plugin's config.")] bool onlyConfig = false)
    {
        Ok(x =>
        {
            foreach (var plugin in PluginLoader.EnabledPlugins)
            {
                try
                {
                    plugin.LoadConfigs();

                    if (!onlyConfig)
                    {
                        plugin.Disable();
                        plugin.Enable();
                    }

                    x.AppendLine($"{plugin.Name}: OK");
                }
                catch (Exception ex)
                {
                    x.AppendLine($"{plugin.Name}: {ex.Message}");
                }
            }
        });
    }    
}