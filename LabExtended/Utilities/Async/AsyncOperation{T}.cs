using Common.Extensions;
using Common.Values;

using LabExtended.Core;

using MEC;

namespace LabExtended.Utilities.Async
{
    public class AsyncOperation<T>
    {
        private volatile ReferenceValue<T> _result = new ReferenceValue<T>(default);

        private volatile bool _done;
        private volatile Exception _error;

        public bool IsDone => _done;
        public Exception Error => _error;

        public T Result
        {
            get
            {
                if (_error != null)
                    throw _error;

                if (!_done)
                    throw new InvalidOperationException($"This operation has not finished yet.");

                return _result.Value;
            }
        }

        public void Await(Action<T> callback)
        {
            if (_done)
            {
                callback(_result.Value);
                return;
            }

            Timing.RunCoroutine(AwaitCoroutine(callback));
        }

        internal void Report(Exception error, T result)
        {
            if (_done)
                throw new InvalidOperationException($"This operation is already marked as finished.");

            _error = error;
            _done = true;

            _result.Value = result;
        }

        private IEnumerator<float> AwaitCoroutine(Action<T> callback)
        {
            while (!_done)
                yield return Timing.WaitForSeconds(0.1f);

            callback.Call(_result.Value, null, ex => ExLoader.Error("Async API", $"Callback caught an error:\n{ex}"));
        }
    }
}