using LabExtended.API;
using LabExtended.API.Map;

using PluginAPI.Events;

namespace LabExtended.Events
{
    internal static class InternalEvents
    {
        internal static void OnMapGenerated(MapGeneratedEvent ev)
        {
            ExMap.GenerateMap();
            ExTeslaGate._pauseUpdate = false;
        }
    }
}