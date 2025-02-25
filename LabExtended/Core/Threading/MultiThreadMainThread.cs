using System.Collections.Concurrent;
using System.Diagnostics;

using LabExtended.Core.Configs.Sections;
using LabExtended.Utilities.Unity;
using LabExtended.Extensions;

using UnityEngine.PlayerLoop;

namespace LabExtended.Core.Threading;

public static class MultiThreadMainThread
{
    public struct MultiThreadUpdateLoop { }

    private static volatile ConcurrentQueue<MultiThreadOperation> _pendingOperations = new ConcurrentQueue<MultiThreadOperation>();
    private static volatile TaskScheduler _taskScheduler;
    
    private static Stopwatch _timeWatch = new Stopwatch();

    public static MultiThreadSection Config => ApiLoader.ApiConfig.MultiThreadSection;

    public static TaskScheduler MainTaskScheduler => _taskScheduler;
    
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

    internal static void ProcessOperation(MultiThreadOperation threadOperation)
    {
        if (threadOperation is null || !threadOperation.IsMainThread)
            return;

        _pendingOperations.Enqueue(threadOperation);
    }
    
    internal static void InitMainThread()
    {
        PlayerLoopHelper.ModifySystem(x =>
        {
            if (!x.InjectBefore<Initialization.ProfilerStartFrame>(UpdateMainThread, typeof(MultiThreadUpdateLoop)))
                return null;

            return x;
        });
        
        _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
    }

    private static void Process(MultiThreadOperation operation)
    {
        try
        {
            operation.Result = operation.Target();
            operation.IsFinished = true;

            operation.Callback.InvokeSafe(null, operation.Result);
            operation.ReturnToPool<MultiThreadOperation>();
        }
        catch (Exception ex)
        {
            operation.Exception = ex;
            operation.IsFinished = true;
            
            operation.Callback.InvokeSafe(ex, operation.Result);
            operation.ReturnToPool<MultiThreadOperation>();
            
            ApiLog.Error("MultiThread", $"An error was caught while executing job on main thread &6{operation.Id}&r:\n{ex.ToColoredString()}");
        }
    }

    private static void UpdateMainThread()
    {
        _timeWatch.Restart();

        var size = 0;

        while (_pendingOperations.TryDequeue(out var next))
        {
            Process(next);
            
            if (Config.MainThreadMaxQueueSize > 0 && size + 1 >= Config.MainThreadMaxQueueSize)
                break;

            if (Config.MainThreadMaxQueueTime > 0 && _timeWatch.ElapsedMilliseconds >= Config.MainThreadMaxQueueTime)
                break;
        }
    }
}