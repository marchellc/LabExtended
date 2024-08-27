using LabExtended.Core.Ticking;

using System.Collections.Concurrent;

namespace LabExtended.Utilities.Threading
{
    public static class ThreadSafe
    {
        static ThreadSafe()
            => TickDistribution.UnityTick.CreateHandle(TickDistribution.CreateWith(UpdatePending, new TickOptions(TickFlags.Separate)));

        private static volatile ConcurrentQueue<ThreadSafeOperation> m_Pending = new ConcurrentQueue<ThreadSafeOperation>();

        public static void Invoke(Action action, bool wait = true)
        {
            if (!wait)
            {
                m_Pending.Enqueue(new ThreadSafeOperation { Delegate = () => { action(); return null; }, });
                return;
            }

            var finished = false;
            var error = default(Exception);

            void Callback(Exception ex, object _)
            {
                error = ex;
                finished = true;
            }

            m_Pending.Enqueue(new ThreadSafeOperation
            {
                Delegate = () => { action(); return null; },
                Callback = Callback
            });

            while (!finished)
                continue;

            if (error != null)
                throw error;
        }

        public static T Get<T>(Func<T> action)
        {
            var value = Invoke(() => action());

            if (value is null)
                return default;

            return (T)value;
        }

        public static object Invoke(Func<object> action)
        {
            var finished = false;

            var result = default(object);
            var error = default(Exception);

            void Callback(Exception ex, object value)
            {
                result = value;
                error = ex;

                finished = true;
            }

            m_Pending.Enqueue(new ThreadSafeOperation
            {
                Delegate = action,
                Callback = Callback
            });

            while (!finished)
                continue;

            if (error != null)
                throw error;

            return result;
        }

        private static void UpdatePending()
        {
            while (m_Pending.TryDequeue(out var nextOp))
            {
                try
                {
                    nextOp.Result = nextOp.Delegate();
                    nextOp.Callback?.Invoke(null, nextOp.Result);
                }
                catch (Exception ex)
                {
                    nextOp.Error = ex;
                    nextOp.Callback?.Invoke(ex, null);
                }

                nextOp.Done = true;
            }
        }
    }
}