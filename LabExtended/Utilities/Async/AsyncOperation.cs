using Common.Extensions;

using LabExtended.Core;

using MEC;

namespace LabExtended.Utilities.Async
{
    public class AsyncOperation
    {
        private volatile bool _done;
        private volatile Exception _error;

        public bool IsDone => _done;

        public Exception Error => _error;

        public void Await(Action callback)
        {
            if (_done)
            {
                callback();
                return;
            }

            Timing.RunCoroutine(AwaitCoroutine(callback));
        }

        internal void Report(Exception error)
        {
            if (_done)
                throw new InvalidOperationException($"This operation is already marked as finished.");

            _error = error;
            _done = true;
        }

        private IEnumerator<float> AwaitCoroutine(Action callback)
        {
            while (!_done)
                yield return Timing.WaitForSeconds(0.1f);

            callback.Call(null, ex => ExLoader.Error("Async API", $"Callback caught an error:\n{ex}"));
        }
    }
}
