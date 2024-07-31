using LabExtended.Core.Profiling;
using LabExtended.Extensions;

namespace LabExtended.Core.Ticking
{
    public class TickInfo
    {
        internal TickComponent _separateComponent;
        internal ProfilerMarker _marker;

        internal DateTime? _nextTickTime;
        internal DateTime? _lastTickTime;

        internal int _passedFrames = 0;

        public Action Target { get; }

        public string Id { get; }

        public TickTimer Timer { get; }

        public bool IsPaused { get; set; } = false;
        public bool IsSeparate { get; set; } = false;

        public bool IsRunning => TickManager.IsRunning(Id) && !IsPaused;
        public bool IsSubscribed => TickManager._activeTicks.Any(h => h.Id == Id);

        public bool CanTick
        {
            get
            {
                if (IsPaused)
                    return false;

                if (_nextTickTime.HasValue && DateTime.Now < _nextTickTime.Value)
                    return false;

                if (Timer.IsFramed && ++_passedFrames < Timer.DelayValue.Value)
                    return false;

                return true;
            }
        }


        internal TickInfo(Action target, string id, TickTimer timer)
        {
            Target = target;
            Timer = timer;
            Id = id;
        }

        public void Pause()
            => IsPaused = true;

        public void Resume()
            => IsPaused = false;

        public void Toggle()
            => IsPaused = !IsPaused;

        public void Unsubscribe()
        {
            TickManager._activeTicks.RemoveWhere(h => h.Id == Id);

            _separateComponent?.Stop();
            _separateComponent = null;
        }

        public void Subscribe()
        {
            if (TickManager._activeTicks.Contains(this))
                return;

            TickManager._activeTicks.Add(this);

            if (IsSeparate)
            {
                _separateComponent?.Stop();

                TickComponent.Create(Id, Target, () => CanTick, out _, out _separateComponent);
            }
        }

        internal void RegisterTickStart()
        {
            if (Timer.IsProfiled)
                (_marker ??= new ProfilerMarker(Target.Method.GetMemberName())).MarkStart();
            else
            {
                _marker?.Dispose();
                _marker = null;
            }
        }

        internal void RegisterTickEnd()
        {
            if (Timer.IsProfiled)
                (_marker ??= new ProfilerMarker(Target.Method.GetMemberName())).MarkEnd();
            else
            {
                _marker?.Dispose();
                _marker = null;
            }

            _passedFrames = 0;

            if (Timer.DelayValue.HasValue && !Timer.IsFramed && Timer.DelayValue.Value > 0f)
                _nextTickTime = DateTime.Now.AddMilliseconds(Timer.DelayValue.Value);
            else if (Timer.DelayRange != null && Timer.DelayRange.Item2 > Timer.DelayRange.Item1)
                _nextTickTime = DateTime.Now.AddMilliseconds(UnityEngine.Random.Range(Timer.DelayRange.Item1, Timer.DelayRange.Item2));
            else
                _nextTickTime = null;
        }
    }
}