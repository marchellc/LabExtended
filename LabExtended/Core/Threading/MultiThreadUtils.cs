namespace LabExtended.Core.Threading;

public static class MultiThreadUtils
{
    public static void ContinueWithOnMain(this Task task, Action<Task> continuation)
        => task.ContinueWith(continuation, MultiThreadMainThread.MainTaskScheduler);

    public static void ContinueWithOnMain<T>(this Task<T> task, Action<Task<T>> continuation)
        => task.ContinueWith(continuation, MultiThreadMainThread.MainTaskScheduler);
}