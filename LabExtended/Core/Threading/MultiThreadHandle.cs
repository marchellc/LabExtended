using System.Collections.Concurrent;

using LabExtended.Extensions;

namespace LabExtended.Core.Threading;

public class MultiThreadHandle
{
    public volatile ConcurrentQueue<MultiThreadOperation> Queue = new ConcurrentQueue<MultiThreadOperation>();
    public volatile Thread Thread;
    
    public volatile bool IsRunning;
    public volatile int Id;

    internal void RunQueue()
    {
        while (IsRunning)
        {
            try
            {
                while (Queue.TryDequeue(out var next))
                {
                    RunOperation(next);
                }
            }
            catch { }
        }
    }

    private void RunOperation(MultiThreadOperation operation)
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
            
            operation.Callback.InvokeSafe(ex, null);
            operation.ReturnToPool<MultiThreadOperation>();
            
            ApiLog.Error("MultiThread", $"An error was caught while executing job on side thread (ID: &6{Id}&r) &6{operation.Id}&r:\n{ex.ToColoredString()}");
        }
    }
}