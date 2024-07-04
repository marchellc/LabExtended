using LabExtended.API;

using PluginAPI.Events;

namespace LabExtended.Events
{
    internal static class InternalEvents
    {
        internal static void OnMapGenerated(MapGeneratedEvent ev)
        {
            ExMap.GenerateMap();
        }
    }
}