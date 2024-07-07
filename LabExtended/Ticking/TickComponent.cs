using LabExtended.Core;

using UnityEngine;

namespace LabExtended.Ticking
{
    public class TickComponent : MonoBehaviour
    {
        void Start()
            => ExLoader.Debug("Ticking API", "Component started.");

        void Update()
            => TickManager.CallUpdate();

        void OnDestroy()
            => ExLoader.Warn("Ticking API", "Tick component destroyed! This should NOT happen.");
    }
}