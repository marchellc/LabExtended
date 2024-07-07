using Common.Extensions;

namespace LabExtended.Utilities.Threading
{
    public static class ThreadSafeUnity
    {
        public static void Invoke(Action action, bool waitForCompletion = false)
        {
            var hasCompleted = false;

            UnityThread.Thread.Run(action, () => hasCompleted = true);

            if (waitForCompletion)
            {
                while (!hasCompleted)
                    continue;
            }
        }

        public static T Invoke<T>(Func<T> getter)
        {
            var hasCompleted = false;
            var returnValue = default(T);

            UnityThread.Thread.Run(getter, value =>
            {
                hasCompleted = true;
                returnValue = value;
            });

            while (!hasCompleted)
                continue;

            return returnValue;
        }
    }
}