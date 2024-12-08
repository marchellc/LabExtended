using LabExtended.Core.Configs.Sections;
using LabExtended.Extensions;

using MEC;

using UnityEngine;

namespace LabExtended.Core.Ticking.Distributors.Unity
{
    public static class UnityTickLoader
    {
        private static UnityTickLoop _loop;
        private static UnityTickComponent _component;

        private static CoroutineHandle? _handle;

        private static Action _onTick;

        public static float TickRate = -1f;
        public static float TickTime = -1f;

        public static float MaxTickRate = -1f;
        public static float MaxTickTime = -1f;

        public static float MinTickRate = -1f;
        public static float MinTickTime = -1f;

        public static bool EnableTiming;

        public static TickSection TickSection => ApiLoader.ApiConfig.TickSection;

        public static void Load(Action onTick, out string type)
        {
            type = null;

            if (!TickSection.UseTickCoroutine && !TickSection.UseTickComponent && !TickSection.UseTickLoop)
            {
                ApiLog.Warn("Unity Tick Distributor", $"All Unity tick distribution options have been disabled by config.");
                return;
            }

            EnableTiming = TickSection.EnableMetrics;

            if (TickSection.UseTickLoop)
            {
                _loop = new UnityTickLoop();
                _loop.OnInvoke += OnUpdate;

                type = "Player Loop";
            }
            else if (TickSection.UseTickComponent)
            {
                var go = new GameObject($"Unity Tick Component ({DateTime.Now.Ticks})");

                _component = go.AddComponent<UnityTickComponent>();
                _component.SetUpdate(OnUpdate);

                UnityEngine.Object.DontDestroyOnLoad(go);
                UnityEngine.Object.DontDestroyOnLoad(_component);

                type = "Component";
            }
            else if (TickSection.UseTickCoroutine)
            {
                _handle = Timing.RunCoroutine(TickCoroutine());

                type = "Coroutine";
            }

            _onTick = onTick;
        }

        public static void Destroy()
        {
            if (_loop != null)
            {
                _loop.OnInvoke -= OnUpdate;
                _loop.Stop();
                _loop = null;
            }

            if (_component != null)
            {
                _component.SetUpdate(null);

                UnityEngine.Object.Destroy(_component);

                _component = null;
            }

            if (_handle.HasValue && Timing.IsRunning(_handle.Value))
            {
                Timing.KillCoroutines(_handle.Value);

                _handle = null;
            }    
        }

        private static void OnUpdate()
        {
            _onTick.InvokeSafe();
        }

        private static IEnumerator<float> TickCoroutine()
        {
            while (true)
            {
                try
                {
                    if (EnableTiming)
                    {
                        TickTime = Time.deltaTime;

                        if (MinTickTime < 0f || MinTickTime > TickTime)
                            MinTickTime = TickTime;

                        if (MaxTickTime < 0f || MaxTickTime < TickTime)
                            MaxTickTime = TickTime;
                    }

                    TickRate = 1f / Time.deltaTime;

                    if (MinTickRate < 0f || TickRate < MinTickRate)
                        MinTickRate = TickRate;

                    if (MaxTickRate < 0f || TickRate > MaxTickRate)
                        MaxTickRate = TickRate;

                    OnUpdate();
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Unity Tick Distributor", ex);
                }

                yield return Timing.WaitForOneFrame;
            }
        }
    }
}
