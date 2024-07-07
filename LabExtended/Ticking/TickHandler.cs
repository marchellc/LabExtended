using LabExtended.Core.Profiling;

namespace LabExtended.Ticking
{
    public class TickHandler
    {
        internal DateTime? _nextTickTime;
        internal DateTime? _lastTickTime;

        internal int _passedFrames = 0;

        internal ProfilerMarker _marker;

        public string Id { get; }

        public Action Method { get; }

        public TickOptions Options { get; }

        public bool IsPaused { get; set; } = false;

        public bool IsRunning => TickManager.IsRunning(Id) && !IsPaused;
        public bool IsSubscribed => TickManager._activeTicks.ContainsKey(Id);

        public bool CanTick
        {
            get
            {
                if (IsPaused)
                    return false;

                if (Options.DelayType is TickDelayType.None)
                    return true;

                if (_nextTickTime.HasValue && DateTime.Now < _nextTickTime.Value)
                    return false;

                if (Options.DelayType is TickDelayType.Frames && ++_passedFrames < Options.DelayValue)
                    return false;

                return true;
            }
        }

        public TickHandler(string id, Action method, TickOptions options)
        {
            Id = id;
            Method = method;
            Options = options;
        }

        public void Pause()
            => IsPaused = true;

        public void Resume()
            => IsPaused = false;

        public void Toggle()
            => IsPaused = !IsPaused;

        public void Unsubscribe()
            => TickManager._activeTicks.Remove(Id);

        public void Subscribe()
            => TickManager._activeTicks[Id] = this;

        internal void RegisterTickStart()
        {
            if (Options.IsProfiled)
                _marker.MarkStart();
        }

        internal void RegisterTickEnd()
        {
            if (Options.IsProfiled)
                _marker.MarkEnd();

            _passedFrames = 0;

            if (Options.DelayType is TickDelayType.Static && Options.DelayValue > 0f)
                _nextTickTime = DateTime.Now.AddMilliseconds(Options.DelayValue);
            else if (Options.DelayType is TickDelayType.Dynamic && Options.DelayRange != null && Options.DelayRange.Item2 > Options.DelayRange.Item1)
                _nextTickTime = DateTime.Now.AddMilliseconds(UnityEngine.Random.Range(Options.DelayRange.Item1, Options.DelayRange.Item2));
            else
                _nextTickTime = null;
        }
    }
}