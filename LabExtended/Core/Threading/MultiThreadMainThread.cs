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