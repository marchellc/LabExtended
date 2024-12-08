using LabExtended.Extensions;
using LabExtended.Utilities.Unity;

using System.Diagnostics;

using UnityEngine;
using UnityEngine.PlayerLoop;

namespace LabExtended.Core.Ticking.Distributors.Unity
{
    public class UnityTickLoop
    {
        private struct UnityTickDistributorLoop { }

        private static event Action _customLoop;

        static UnityTickLoop()
            => PlayerLoopHelper.ModifySystem(x => 
            {
                if (!x.InjectBefore<PreLateUpdate.ScriptRunBehaviourLateUpdate>(InvokeCustom, typeof(UnityTickDistributorLoop)))
                    return null;

                return x;
            });

        private Stopwatch _watch = new Stopwatch();

        public event Action OnInvoke;

        public UnityTickLoop()
            => _customLoop += Invoke;

        public void Stop()
        {
            OnInvoke = null;

            _customLoop -= Invoke;

            _watch?.Reset();
            _watch = null;
        }

        private void Invoke()
        {
            if (UnityTickLoader.EnableTiming && _watch != null)
                _watch.Restart();

            OnInvoke.InvokeSafe();

            if (UnityTickLoader.EnableTiming && _watch != null)
            {
                UnityTickLoader.TickTime = _watch.ElapsedTicks / TimeSpan.TicksPerMillisecond;

                if (UnityTickLoader.MinTickTime < 0f || UnityTickLoader.MinTickTime > UnityTickLoader.TickTime)
                    UnityTickLoader.MinTickTime = UnityTickLoader.TickTime;

                if (UnityTickLoader.MaxTickTime < 0f || UnityTickLoader.MaxTickTime < UnityTickLoader.TickTime)
                    UnityTickLoader.MaxTickTime = UnityTickLoader.TickTime;
            }

            UnityTickLoader.TickRate = 1f / _watch.ElapsedMilliseconds;

            if (UnityTickLoader.MinTickRate < 0f || UnityTickLoader.TickRate < UnityTickLoader.MinTickRate)
                UnityTickLoader.MinTickRate = UnityTickLoader.TickRate;

            if (UnityTickLoader.MaxTickRate < 0f || UnityTickLoader.TickRate > UnityTickLoader.MaxTickRate)
                UnityTickLoader.MaxTickRate = UnityTickLoader.TickRate;
        }

        private static void InvokeCustom()
            => _customLoop.InvokeSafe();
    }
}