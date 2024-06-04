using LabExtended.API.Map;

namespace LabExtended.API
{

    public static class ExMap
    {
        private static readonly Dictionary<TeslaGate, Tesla> _gates = new Dictionary<TeslaGate, Tesla>();

        public static IEnumerable<Tesla> TeslaGates => _gates.Values;
        public static int TeslaCount => _gates.Count;

        internal static void OnMapGenerated()
        {
            _gates.Clear();

            foreach (var gate in TeslaGateController.Singleton.TeslaGates)
                _gates[gate] = new Tesla(gate);
        }

        internal static void OnRoundStarted()
        {

        }
    }
}