using LabExtended.Core;
using LabExtended.Core.Hooking;
using LabExtended.Events.Server;

using PluginAPI.Core.Attributes;

using System;

namespace LabExtended.Testing
{
    public class TestingPlugin
    {
        [PluginEntryPoint("Testing", "1.0.0", "A testing plugin for LabExtended.", "marchellc")]
        public void Load()
        {
            ExLoader.Info("Testing Plugin", "Loaded the testing plugin.");
        }

        [HookEvent(typeof(ServerStartedArgs))]
        public void OnServerStartedCustom(DateTime startTime, TimeSpan time)
            => ExLoader.Info("Testing Plugin", $"Server start took {time.TotalMilliseconds} ms");
    }
}