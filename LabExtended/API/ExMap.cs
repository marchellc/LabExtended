using LabExtended.API.Map;

using LabExtended.Core;
using LabExtended.Core.Profiling;
using LabExtended.Extensions;

namespace LabExtended.API
{
    public static class ExMap
    {
        private static readonly ProfilerMarker _genMarker = new ProfilerMarker("Map Generation");

        public static IEnumerable<ExTeslaGate> TeslaGates => ExTeslaGate._wrappers.Values;

        internal static void GenerateMap()
        {
            _genMarker.MarkStart();

            try
            {
                ExTeslaGate._wrappers.Clear();

                if (TeslaGateController.Singleton is null)
                {
                    ExLoader.Warn("Extended API", $"Attempted to reload Tesla Gates while the singleton is still null!");
                    _genMarker.MarkEnd();
                    return;
                }

                foreach (var gate in TeslaGateController.Singleton.TeslaGates)
                    ExTeslaGate._wrappers[gate] = new ExTeslaGate(gate);
            }
            catch (Exception ex)
            {
                ExLoader.Error("Extended API", $"Map generation failed!\n{ex.ToColoredString()}");
            }

            _genMarker.MarkEnd();
        }
    }
}