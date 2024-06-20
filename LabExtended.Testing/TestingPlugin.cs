using LabExtended.Attributes;
using LabExtended.Core;

using MEC;

using PluginAPI.Core.Attributes;
using PluginAPI.Events;

using System;
using System.Collections.Generic;

namespace LabExtended.Testing
{
    public class TestingPlugin
    {
        [HookDescriptor]
        public static event Action<WaitingForPlayersEvent> OnWaitingForPlayers;

        [PluginEntryPoint("Testing", "1.0.0", "A testing plugin for LabExtended.", "marchellc")]
        public void Load()
        {
            ExLoader.Info("Testing Plugin", "Loaded the testing plugin.");
            OnWaitingForPlayers += _ => ExLoader.Info("Testing Plugin", "Waiting from event");
        }

        [PluginEvent]
        public IEnumerator<float> OnWaiting(WaitingForPlayersEvent ev)
        {
            ExLoader.Info("Testing Plugin", $"2 sec {Timing.DeltaTime} {Timing.LocalTime}");
            yield return Timing.WaitForSeconds(2f);
            ExLoader.Info("Testing Plugin", "waiting");
        }
    }
}