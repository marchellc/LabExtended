using Common.Extensions;
using Common.Utilities.Threading;
using LabExtended.Ticking;
using System.Collections.Concurrent;

namespace LabExtended.Utilities.Threading
{
    public class UnityThread : IThreadManager
    {
        private struct UnityAction
        {
            public readonly ThreadAction Action;
            public readonly Action Callback;

            internal UnityAction(ThreadAction action, Action callback)
            {
                Action = action;
                Callback = callback;
            }
        }

        public static UnityThread Thread { get; } = new UnityThread();

        private static readonly ConcurrentQueue<UnityAction> _actions = new ConcurrentQueue<UnityAction>();

        private UnityThread()
            => TickManager.SubscribeTick(Tick, TickOptions.NoneProfiled, "Unity Thread Tick");

        public int Size => _actions.Count;

        public bool IsRunning => TickManager.IsRunning("Unity Thread Tick");

        public void Run(ThreadAction threadAction, Action callback)
        {
            if (threadAction is null)
                throw new ArgumentNullException(nameof(threadAction));

            _actions.Enqueue(new UnityAction(threadAction, callback));
        }

        private void Tick()
        {
            while (_actions.TryDequeue(out var unityAction))
            {
                unityAction.Action.TargetMethod.TryCall(unityAction.Action.TargetObject, unityAction.Action.TargetArgs);
                unityAction.Callback.Call();
            }
        }
    }
}