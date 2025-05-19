using LabApi.Loader;

using LabExtended.Commands.Attributes;

using LabExtended.Core;
using LabExtended.Extensions;

namespace LabExtended.Commands.Custom.Reload;

public partial class ReloadCommand
{
    [CommandOverload("plugin", "Reloads a specific plugin.")]
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