using LabExtended.Attributes;

namespace LabExtended.Utilities;

/// <summary>
/// Utilities used for thread management.
/// </summary>
public static class ThreadUtils
{
    private static volatile TaskScheduler mainScheduler;
    
    /// <summary>
    /// Gets the task scheduler of the main thread.
    /// </summary>
    public static TaskScheduler MainTaskScheduler => mainScheduler;
    
    /// <summary>
    /// Queues a task continuation to run on the main thread.
    /// </summary>
    /// <param name="task">The task to wait for.</param>
    /// <param name="continuation">The action to continue with.</param>
    public static void ContinueWithOnMain(this Task task, Action<Task> continuation)
        => task.ContinueWith(continuation, MainTaskScheduler);

    /// <summary>
    /// Queues a task continuation to run on the main thread.
    /// </summary>
    /// <param name="task">The task to wait for.</param>
    /// <param name="continuation">The action to continue with.</param>
    public static void ContinueWithOnMain<T>(this Task<T> task, Action<Task<T>> continuation)
        => task.ContinueWith(continuation, MainTaskScheduler);

    /// <summary>
    /// Starts a task on the main game thread.
    /// </summary>
    /// <param name="action">The delegate to run in a task.</param>
    /// <returns>The started task.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Task RunOnMainThread(this Action action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        var task = new Task(action);
        
        task.Start(MainTaskScheduler);
        return task;
    }

    /// <summary>
    /// Starts a task on the main game thread.
    /// </summary>
    /// <typeparam name="T">The type of the return value.</typeparam>
    /// <param name="func">The delegate to run in a task.</param>
    /// <returns>The started task.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Task<T> RunOnMainThread<T>(this Func<T> func)
    {
        if (func is null)
            throw new ArgumentNullException(nameof(func));
        
        var task = new Task<T>(func);
        
        task.Start(MainTaskScheduler);
        return task;
    }
    
    internal static void Internal_Init() 
        => mainScheduler = TaskScheduler.FromCurrentSynchronizationContext();
}