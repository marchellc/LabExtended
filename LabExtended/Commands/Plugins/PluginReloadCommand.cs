using CommandSystem;
using LabExtended.Extensions;
using LabExtended.Utilities;
using PluginAPI.Core;
using PluginAPI.Loader;

using System.Text;

namespace LabExtended.Commands.Plugins
{
    public class PluginReloadCommand : VanillaCommandBase
    {
        public override string Command => "reload";
        public override string Description => "Reloads a specified list of plugins.";

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = $"Missing arguments!\nplugin reload <list of plugins>";
                return false;
            }

            var listSplit = string.Join(" ", arguments).Split(',');

            if (listSplit.Length < 1)
            {
                response = "You need to specify a list of plugins to reload (separated by ,).";
                return false;
            }

            var list = new List<PluginHandler>();

            foreach (var plugin in AssemblyLoader.InstalledPlugins)
            {
                if (listSplit.Contains("*") || listSplit.Any(name => name.GetSimilarity(plugin.PluginName) >= 0.8))
                    list.Add(plugin);
            }

            if (list.Count < 1)
            {
                response = "No valid plugins were found.";
                return false;
            }

            var builder = new StringBuilder();
            var reloaded = 0;

            builder.AppendLine($"Reload results:");

            foreach (var plugin in list)
            {
                var status = string.Empty;

                builder.Append($"\n[{plugin.PluginName}] ({plugin.PluginVersion}): ");

                try
                {
                    plugin.ReloadConfig(plugin._plugin);
                    status = "Config reloaded.";

                    if (plugin._onUnload is null)
                    {
                        status += " | Unload method is missing, skipped.";
                        reloaded++;
                        continue;
                    }

                    plugin._onUnload.InvokeMethod(plugin._plugin);
                    plugin._entryPoint.InvokeMethod(plugin._plugin);

                    status += " | Plugin reloaded.";
                    reloaded++;
                }
                catch (Exception ex)
                {
                    if (status == string.Empty)
                        status = $"Error while reloading: {ex.Message}";
                    else
                        status += $" | Error while reloading: {ex.Message}";
                }

                if (status == string.Empty)
                    status = "Nothing to reload.";

                builder.Append(status);
            }

            response = $"Reloaded {reloaded}/{list.Count} plugin(s).\n{builder}";
            return reloaded == list.Count;
        }
    }
}