using LabApi.Loader;

using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Reload;

public partial class ReloadCommand
{
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