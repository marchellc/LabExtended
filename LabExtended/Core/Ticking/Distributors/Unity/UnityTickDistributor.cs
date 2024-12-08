using LabExtended.API.Collections.Locked;

using LabExtended.Core.Ticking.Interfaces;
using LabExtended.Core.Ticking.Internals;

using LabExtended.Extensions;

namespace LabExtended.Core.Ticking.Distributors.Unity
{
    public class UnityTickDistributor : ITickDistributor
    {
        private volatile LockedHashSet<InternalTickHandleWrapper<UnityTickOptions>> _handles;

        public UnityTickDistributor()
        {
            _handles = new LockedHashSet<InternalTickHandleWrapper<UnityTickOptions>>();

            EnableTiming = ApiLoader.ApiConfig.TickSection.EnableMetrics;

            UnityTickLoader.Load(OnUpdate, out SubDistributorName);
            TickDistribution.AddDistributor(this);
        }

        public event Action OnTick;

        public string SubDistributorName;

        public float TickRate => UnityTickLoader.TickRate;
        public float TickTime => UnityTickLoader.TickTime;

        public float MaxTickRate => UnityTickLoader.MaxTickRate;
        public float MaxTickTime => UnityTickLoader.MaxTickTime;

        public float MinTickRate => UnityTickLoader.MinTickRate;
        public float MinTickTime => UnityTickLoader.MinTickTime;

        public int HandleCount => _handles.Count;

        public bool EnableTiming
        {
            get => UnityTickLoader.EnableTiming;
            set => UnityTickLoader.EnableTiming = value;
        }

        public IEnumerable<string> Handles => _handles.Select(x => x.ToString());

        public TickHandle CreateHandle(InternalTickHandle handle)
        {
            if (handle is null)
                throw new ArgumentNullException(nameof(handle));

            if (handle is not InternalTickHandle internalTickHandle)
                throw new Exception($"Unsupported handle: {handle.GetType().FullName}");

            if (_handles.Any(x => x.Base.Id == internalTickHandle.Id))
                throw new Exception($"Duplicate handle ID: {internalTickHandle.Id}");

            var wrapper = new InternalTickHandleWrapper<UnityTickOptions>(internalTickHandle, (internalTickHandle.Options as UnityTickOptions) ?? UnityTickOptions.DefaultOptions);

            if (wrapper.Options.HasFlag(TickFlags.Separate))
            {
                wrapper.Options._loop = new UnityTickLoop();
                wrapper.Options._loop.OnInvoke += () => ExecuteSegment(wrapper);
            }

            _handles.Add(wrapper);
            return new TickHandle(internalTickHandle.Id, this);
        }

        public void RemoveHandle(TickHandle handle)
        {
            if (_handles.TryGetFirst(x => x.Base.Id == handle.Id, out var wrapperHandle))
            {
                if (wrapperHandle.Options.HasFlag(TickFlags.Separate))
                {
                    wrapperHandle.Options._loop?.Stop();
                    wrapperHandle.Options._loop = null;
                }

                if (wrapperHandle.Base.Timer != null)
                {
                    wrapperHandle.Base.Timer.Dispose();
                    wrapperHandle.Base.Timer = null;
                }

                wrapperHandle.Base.Paused = true;
            }
            else
            {
                Debug($"Failed to find handle {handle.Id}");
                return;
            }

            _handles.RemoveWhere(x => x.Base.Id == handle.Id);

            handle.InternalDestroy();
        }

        public bool HasHandle(TickHandle handle)
            => _handles.Any(x => x.Base.Id == handle.Id);

        public bool IsActive(TickHandle handle)
            => _handles.Any(x => x.Base.Id == handle.Id && !x.Base.Paused);

        public bool IsPaused(TickHandle handle)
            => _handles.Any(x => x.Base.Id == handle.Id && x.Base.Paused);

        public void Pause(TickHandle handle)
        {
            _handles.ForEach(x =>
            {
                if (x.Base.Id != handle.Id)
                    return;

                if (x.Base.Paused)
                {
                    Warn($"Handle Id={x.Base.Id} Invoker={x.Base.Invoker} is already paused");
                    return;
                }

                x.Base.Paused = true;
            });
        }

        public void Resume(TickHandle handle)
        {
            _handles.ForEach(x =>
            {
                if (x.Base.Id != handle.Id)
                    return;

                if (!x.Base.Paused)
                {
                    Warn($"Handle Id={x.Base.Id} Invoker={x.Base.Invoker} is not paused");
                    return;
                }

                x.Base.Paused = false;
            });
        }

        public void ClearEvent()
            => OnTick = null;

        public void ClearHandles()
            => _handles.Clear();

        public void Dispose()
        {
            OnTick = null;

            if (_handles != null)
            {
                _handles.ForEach(x => TickDistribution.DestroyHandle(x.Base));

                _handles.Clear();
                _handles = null;
            }

            UnityTickLoader.Destroy();
        }

        private void OnUpdate()
            => ExecuteSegment();

        private void ExecuteSegment(InternalTickHandleWrapper<UnityTickOptions> wrapper)
        {
            if (wrapper.Base.Paused)
                return;

            if (wrapper.Base.Timer != null && !wrapper.Base.Timer.CanContinue())
                return;

            if (wrapper.Options.HasUnityFlag(UnityTickFlags.SkipFrames) && wrapper.Options._skippedFrames < wrapper.Options.SkipFrames)
            {
                wrapper.Options._skippedFrames++;
                return;
            }

            if (EnableTiming && wrapper.Base.Watch != null)
            {
                wrapper.Base.Watch.Reset();
                wrapper.Base.Watch.Start();
            }

            wrapper.Base.Invoker?.Invoke();
            wrapper.Base.Timer?.OnExecuted();

            if (EnableTiming && wrapper.Base.Watch != null)
            {
                wrapper.Base.Watch.Stop();
                wrapper.Base.TickTime = wrapper.Base.Watch.ElapsedTicks / TimeSpan.TicksPerMillisecond;

                if (wrapper.Base.MaxTickTime < 0f || wrapper.Base.MaxTickTime < wrapper.Base.TickTime)
                    wrapper.Base.MaxTickTime = wrapper.Base.TickTime;

                if (wrapper.Base.MinTickTime < 0f || wrapper.Base.MinTickTime > wrapper.Base.TickTime)
                    wrapper.Base.MinTickTime = wrapper.Base.TickTime;
            }

            wrapper.Options._skippedFrames = 0;
        }

        private void ExecuteSegment()
        {
            OnTick.InvokeSafe();

            _handles.ForEach(x =>
            {
                if (x.Base.Paused)
                    return;

                if (x.Options.HasFlag(TickFlags.Separate))
                    return;

                if (x.Base.Timer != null && !x.Base.Timer.CanContinue())
                    return;

                if (x.Options.HasUnityFlag(UnityTickFlags.SkipFrames) && x.Options._skippedFrames < x.Options.SkipFrames)
                {
                    x.Options._skippedFrames++;
                    return;
                }

                if (EnableTiming && x.Base.Watch != null)
                {
                    x.Base.Watch.Reset();
                    x.Base.Watch.Start();
                }

                x.Base.Invoker?.Invoke();
                x.Base.Timer?.OnExecuted();

                if (EnableTiming && x.Base.Watch != null)
                {
                    x.Base.Watch.Stop();
                    x.Base.TickTime = x.Base.Watch.ElapsedTicks / TimeSpan.TicksPerMillisecond;

                    if (x.Base.MaxTickTime < 0f || x.Base.MaxTickTime < x.Base.TickTime)
                        x.Base.MaxTickTime = x.Base.TickTime;

                    if (x.Base.MinTickTime < 0f || x.Base.MinTickTime > x.Base.TickTime)
                        x.Base.MinTickTime = x.Base.TickTime;
                }

                x.Options._skippedFrames = 0;
            });
        }

        public override string ToString()
            => $"UnityTickDistributor Handles={HandleCount} TickRate={TickRate} TickTime={TickTime} SubDistributor={SubDistributorName ?? "null"}";

        private void Info(object msg)
            => ApiLog.Info("Unity Tick Distributor", msg);

        private void Warn(object msg)
            => ApiLog.Warn("Unity Tick Distributor", msg);

        private void Error(object msg)
            => ApiLog.Error("Unity Tick Distributor", msg);

        private void Debug(object msg)
            => ApiLog.Debug("Unity Tick Distributor", msg);
    }
}