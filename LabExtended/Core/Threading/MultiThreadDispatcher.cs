using LabExtended.Attributes;
using LabExtended.Extensions;
using LabExtended.Utilities.Values;

using LabExtended.Core.Configs.Sections;
using LabExtended.Core.Pooling.Pools;

namespace LabExtended.Core.Threading;

public static class MultiThreadDispatcher
{
    private static volatile LockedValue<List<MultiThreadHandle>> _threadHandlers = new LockedValue<List<MultiThreadHandle>>(new List<MultiThreadHandle>());
    
    private static volatile int _threadIdClock = 0;
    private static volatile int _operationIdClock = 0;

    public static MultiThreadSection Config => ApiLoader.ApiConfig.MultiThreadSection;

    public static void DispatchOnMainThread(this Action target, Action<Exception> callback = null)
        => DispatchOnMainThread(
            () =>
            {
                target();
                return null;
            },
            (ex, _) =>
            {
                callback.InvokeSafe(ex);
            });

    public static void DispatchOnMainThread<T>(this Func<T> target, Action<Exception, T> callback = null)
        => DispatchOnMainThread(
            () => target(),
            (ex, resultObj) =>
            {
                if (ex != null || resultObj is null || resultObj is not T result)
                {
                    callback.InvokeSafe(ex, default);
                    return;
                }

                callback.InvokeSafe(ex, result);
            });
    
    public static void DispatchOnSideThread(this Action target, Action<Exception> callback = null)
        => DispatchOnSideThread(
            () =>
            {
                target();
                return null;
            },
            (ex, _) =>
            {
                callback.InvokeSafe(ex);
            });

    public static void DispatchOnSideThread<T>(this Func<T> target, Action<Exception, T> callback = null)
        => DispatchOnSideThread(
            () => target(),
            (ex, resultObj) =>
            {
                if (ex != null || resultObj is null || resultObj is not T result)
                {
                    callback.InvokeSafe(ex, default);
                    return;
                }

                callback.InvokeSafe(ex, result);
            });

    public static void DispatchOnSideThread(this Func<object> target, Action<Exception, object> callback = null)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        var operation = ObjectPool<MultiThreadOperation>.Shared.Rent(null, () => new MultiThreadOperation());

        operation.Target = target;
        operation.Callback = callback;
        
        Dispatch(operation);
    }
    
    public static void DispatchOnMainThread(this Func<object> target, Action<Exception, object> callback = null)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        var operation = ObjectPool<MultiThreadOperation>.Shared.Rent(null, () => new MultiThreadOperation());

        operation.Target = target;
        operation.Callback = callback;
        operation.IsMainThread = true;
        
        Dispatch(operation);
    }
    
    public static void Dispatch(MultiThreadOperation threadOperation)
    {
        if (threadOperation is null)
            throw new ArgumentNullException(nameof(threadOperation));

        if (_operationIdClock + 1 >= int.MaxValue)
            _operationIdClock = 0;

        threadOperation.Id = _operationIdClock++;
        StartOnAvailableHandle(threadOperation);
    }

    private static void StartOnAvailableHandle(MultiThreadOperation threadOperation)
    {
        if (threadOperation is null)
            return;

        if (threadOperation.IsMainThread)
        {
            MultiThreadMainThread.ProcessOperation(threadOperation);
            return;
        }

        _threadHandlers.Access((_, list) =>
        {
            var targetHandle = GetAvailableHandle(list);

            if (targetHandle is null)
                return;

            targetHandle.Queue.Enqueue(threadOperation);

            ApiLog.Debug("MultiThread", $"Dispatched operation (ID: &6{threadOperation.Id}&r) on thread ID &6{targetHandle.Id}&r (&3{threadOperation.Target.Method.GetMemberName()}&r)");
        });
    }

    private static MultiThreadHandle GetAvailableHandle(List<MultiThreadHandle> handles)
    {
        if (handles.TryGetFirst(
                x => x.IsRunning && (Config.MultiThreadHandleMaxSize < 1 ||
                                     x.Queue.Count < Config.MultiThreadHandleMaxSize), out var freeHandle))
            return freeHandle;

        freeHandle = StartNewHandle();

        handles.Add(freeHandle);
        return freeHandle;
    }

    private static MultiThreadHandle StartNewHandle()
    {
        var handle = new MultiThreadHandle();

        handle.Id = _threadIdClock++;
        handle.IsRunning = true;

        handle.Thread = new Thread(handle.RunQueue);
        handle.Thread.IsBackground = true;
        handle.Thread.Priority = ThreadPriority.Lowest;
        handle.Thread.Start();

        ApiLog.Debug("MultiThread", $"Dispatched a new worker thread (ID: &6{handle.Id}&r)");
        return handle;
    }

    [LoaderInitialize(1)]
    private static void InitDispatch()
    {
        MultiThreadMainThread.InitMainThread();
    }
}