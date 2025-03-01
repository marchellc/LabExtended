using LabExtended.Attributes;

namespace LabExtended.Utilities;

public static class ThreadUtils
{
    private static volatile TaskScheduler mainScheduler;
    
    public static TaskScheduler MainTaskScheduler => mainScheduler;
    
    public static void ContinueWithOnMain(this Task task, Action<Task> continuation)
        => task.ContinueWith(continuation, MainTaskScheduler);

    public static void ContinueWithOnMain<T>(this Task<T> task, Action<Task<T>> continuation)
        => task.ContinueWith(continuation, MainTaskScheduler);

    public static Task RunOnMainThread(this Action action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        var task = new Task(action);
        
        task.Start(MainTaskScheduler);
        return task;
    }

    public static Task<T> RunOnMainThread<T>(this Func<T> func)
    {
        if (func is null)
            throw new ArgumentNullException(nameof(func));
        
        var task = new Task<T>(func);
        
        task.Start(MainTaskScheduler);
        return task;
    }
    
    [LoaderInitialize(1)]
    private static void OnInit() => mainScheduler = TaskScheduler.FromCurrentSynchronizationContext();
}