using MEC;

namespace LabExtended.Ticking
{
    public class TickHandler
    {
        public string Id { get; }

        public Action Method { get; }

        public TickOptions Options { get; }

        public CoroutineHandle Coroutine { get; internal set; }

        public bool IsPaused
        {
            get => Options._isPaused;
            set => Options._isPaused = value;
        }

        public bool IsRunning => TickManager.IsRunning(Id);
        public bool IsSubscribed => TickManager._activeTicks.ContainsKey(Id);

        public TickHandler(string id, Action method, TickOptions options, CoroutineHandle coroutine = default)
        {
            Id = id;
            Method = method;
            Options = options;
            Coroutine = coroutine;

            options._tickId = id;
        }

        public void Unsubscribe()
            => TickManager.UnsubscribeTick(Id);

        public void Subscribe()
        {
            TickManager._activeTicks[Id] = this;

            if (Options.IsSeparate && !Timing.IsRunning(Coroutine))
                Coroutine = Timing.RunCoroutine(TickManager.OnCustomUpdate(this));
            else
                Coroutine = TickManager._globalHandle;
        }
    }
}