using LabExtended.Core.Pooling;

namespace LabExtended.Core.Threading;

public class MultiThreadOperation : PoolObject
{
    public volatile bool IsMainThread;
    public volatile int Id;

    public volatile Func<object> Target;
    public volatile Action<Exception, object> Callback;

    public volatile Exception Exception;
    public volatile object Result;

    public volatile bool IsFinished;

    public override void OnReturned()
    {
        base.OnReturned();

        IsMainThread = false;
        IsFinished = false;
        
        Id = 0;

        Target = null;
        Callback = null;

        Exception = null;
        Result = null;
    }
}