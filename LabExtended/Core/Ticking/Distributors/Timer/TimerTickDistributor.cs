using LabExtended.API.Collections.Locked;

using LabExtended.Core.Ticking.Interfaces;
using LabExtended.Core.Ticking.Internals;

using LabExtended.Extensions;

using System.Diagnostics;

namespace LabExtended.Core.Ticking.Distributors.Timer
{
    public class TimerTickDistributor : ITickDistributor
    {
        private volatile System.Timers.Timer _timer;

        private volatile Stopwatch _watch;
        private volatile LockedHashSet<InternalTickHandleWrapper<TimerTickOptions>> _handles;

        private volatile float _tickRate = 0f;
        private volatile float _tickTime = 0f;

        private volatile float _maxTickRate = -1f;
        private volatile float _minTickRate = -1f;

        private volatile float _maxTickTime = -1f;
        private volatile float _minTickTime = -1f;

        public TimerTickDistributor()
        {
            EnableTiming = ApiLoader.ApiConfig.TickSection.EnableMetrics;

            _handles = new LockedHashSet<InternalTickHandleWrapper<TimerTickOptions>>();

            if (EnableTiming)
                _watch = new Stopwatch();

            _timer = new System.Timers.Timer(1);
            _timer.Elapsed += (x, y) => RunWrappers();
            _timer.Start();

            TickDistribution.AddDistributor(this);
        }

        public event Action OnTick;

        public float TickRate => _tickRate;
        public float TickTime => _tickTime;

        public float MaxTickRate => _maxTickRate;
        public float MaxTickTime => _maxTickTime;

        public float MinTickRate => _minTickRate;
        public float MinTickTime => _minTickTime;

        public int HandleCount => _handles.Count;

        public bool EnableTiming { get; set; }

        public IEnumerable<string> Handles => _handles.Select(x => x.ToString());

        public TickHandle CreateHandle(InternalTickHandle handle)
        {
            if (handle is null)
                throw new ArgumentNullException(nameof(handle));

            if (handle is not InternalTickHandle internalTickHandle)
                throw new Exception($"Unsupported handle: {handle.GetType().FullName}");

            if (_handles.Any(x => x.Base.Id == internalTickHandle.Id))
                throw new Exception($"Duplicate handle ID: {internalTickHandle.Id}");

            var wrapper = new InternalTickHandleWrapper<TimerTickOptions>(internalTickHandle, null);

            if (wrapper.BaseOptions != null && wrapper.BaseOptions.HasFlag(TickFlags.Separate))
            {
                wrapper.Options = new TimerTickOptions();

                wrapper.Options.Timer = new System.Timers.Timer(1);
                wrapper.Options.Timer.Elapsed += (x, y) => RunWrapper(wrapper);
                wrapper.Options.Timer.Start();
            }

            _handles.Add(wrapper);
            return new TickHandle(internalTickHandle.Id, this);
        }

        public void RemoveHandle(TickHandle handle)
        {
            if (_handles.TryGetFirst(x => x.Base.Id == handle.Id, out var wrapperHandle))
            {
                if (wrapperHandle.BaseOptions != null && wrapperHandle.BaseOptions.HasFlag(TickFlags.Separate))
                {
                    wrapperHandle.Options.Timer.Stop();
                    wrapperHandle.Options.Timer.Dispose();
                    wrapperHandle.Options.Timer = null;
                }

                if (wrapperHandle.Base.Timer != null)
                {
                    wrapperHandle.Base.Timer.Dispose();
                    wrapperHandle.Base.Timer = null;
                }

                if (wrapperHandle.Base.Watch != null)
                {
                    wrapperHandle.Base.Watch.Stop();
                    wrapperHandle.Base.Watch = null;
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

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            if (_watch != null)
            {
                _watch.Stop();
                _watch = null;
            }
        }

        private void RunWrapper(InternalTickHandleWrapper<TimerTickOptions> wrapper)
        {
            if (wrapper.Base.Paused)
                return;

            if (wrapper.Base.Timer != null && !wrapper.Base.Timer.CanContinue())
                return;

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
        }

        private void RunWrappers()
        {
            if (EnableTiming)
            {
                _watch.Reset();
                _watch.Start();
            }

            OnTick.InvokeSafe();

            _handles.ForEach(x =>
            {
                if (x.Base.Paused)
                    return;

                if (x.Options != null)
                    return;

                if (x.Base.Timer != null && !x.Base.Timer.CanContinue())
                    return;

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
            });

            if (EnableTiming)
            {
                _watch.Stop();

                _tickTime = _watch.ElapsedMilliseconds;
                _tickRate = 1f / _tickTime;

                if (_maxTickRate < 0f || _tickRate > _maxTickRate)
                    _maxTickRate = _tickRate;

                if (_maxTickTime < 0f || _tickTime > _maxTickTime)
                    _maxTickTime = _tickTime;

                if (_minTickRate < 0f || _tickRate < _minTickRate)
                    _minTickRate = _tickRate;

                if (_minTickTime < 0f || _tickTime < _minTickTime)
                    _minTickTime = _tickTime;
            }
        }

        public override string ToString()
            => $"TimerTickDistributor Handles={HandleCount} TickRate={TickRate} TPS TickTime={TickTime} ms";

        private void Info(object msg)
            => ApiLog.Info("Timer Tick Distributor", msg);

        private void Warn(object msg)
            => ApiLog.Warn("Timer Tick Distributor", msg);

        private void Error(object msg)
            => ApiLog.Error("Timer Tick Distributor", msg);

        private void Debug(object msg)
            => ApiLog.Debug("Timer Tick Distributor", msg);
    }
}