using LabExtended.Extensions;

using UnityEngine;

namespace LabExtended.Core.Ticking.Distributors.Unity
{
    public class UnityTickComponent : MonoBehaviour
    {
        private Action _onUpdate;

        void Update()
        {
            if (_onUpdate is null)
                return;

            if (UnityTickLoader.EnableTiming)
            {
                UnityTickLoader.TickTime = Time.deltaTime;

                if (UnityTickLoader.MinTickTime < 0f || UnityTickLoader.MinTickTime > UnityTickLoader.TickTime)
                    UnityTickLoader.MinTickTime = UnityTickLoader.TickTime;

                if (UnityTickLoader.MaxTickTime < 0f || UnityTickLoader.MaxTickTime < UnityTickLoader.TickTime)
                    UnityTickLoader.MaxTickTime = UnityTickLoader.TickTime;
            }

            UnityTickLoader.TickRate = 1f / Time.deltaTime;

            if (UnityTickLoader.MinTickRate < 0f || UnityTickLoader.TickRate < UnityTickLoader.MinTickRate)
                UnityTickLoader.MinTickRate = UnityTickLoader.TickRate;

            if (UnityTickLoader.MaxTickRate < 0f || UnityTickLoader.TickRate > UnityTickLoader.MaxTickRate)
                UnityTickLoader.MaxTickRate = UnityTickLoader.TickRate;

            _onUpdate.InvokeSafe();
        }

        public void SetUpdate(Action onUpdate)
            => _onUpdate = onUpdate;
    }
}