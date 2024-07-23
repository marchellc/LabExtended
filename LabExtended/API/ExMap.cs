using InventorySystem.Items.Pickups;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities;

using MapGeneration.Distributors;

namespace LabExtended.API
{
    public static class ExMap
    {
        private static bool _pickupEventsRegistered;

        internal static readonly List<ItemPickupBase> _pickups = new List<ItemPickupBase>();
        internal static readonly List<Locker> _lockers = new List<Locker>();

        public static IEnumerable<ExTeslaGate> TeslaGates => ExTeslaGate._wrappers.Values;

        public static IEnumerable<ItemPickupBase> Pickups => _pickups;
        public static IEnumerable<Locker> Lockers => _lockers;

        internal static void GenerateMap()
        {
            try
            {
                if (!_pickupEventsRegistered)
                {
                    ItemPickupBase.OnPickupAdded += OnPickupAdded;
                    _pickupEventsRegistered = true;
                }

                ExTeslaGate._wrappers.Clear();

                _pickups.Clear();
                _lockers.Clear();

                if (TeslaGateController.Singleton != null)
                {
                    foreach (var gate in TeslaGateController.Singleton.TeslaGates)
                    {
                        ExTeslaGate._wrappers[gate] = new ExTeslaGate(gate);
                    }
                }
                else
                {
                    ExLoader.Warn("Extended API", $"Attempted to reload Tesla Gates while the singleton is still null!");
                }

                foreach (var locker in UnityEngine.Object.FindObjectsOfType<Locker>())
                    _lockers.Add(locker);
            }
            catch (Exception ex)
            {
                ExLoader.Error("Extended API", $"Map generation failed!\n{ex.ToColoredString()}");
            }
        }

        private static void OnPickupAdded(ItemPickupBase pickup)
        {
            if (pickup is null)
                return;

            _pickups.Add(pickup);

            NetworkDestroy.Subscribe(pickup.netId, id =>
            {
                ExLoader.Debug("Map API", $"Pickup identity destroyed: {id.netId} ({pickup.netId})");
                _pickups.RemoveAll(p => p.netId == id.netId);
            });
        }
    }
}