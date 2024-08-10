namespace LabExtended.Utilities.Threading
{
    public class ThreadSafeOperation
    {
        public volatile Action<Exception, object> Callback;
        public volatile Func<object> Delegate;
        public volatile Exception Error;
        public volatile object Result;
        public volatile bool Done;
    }
}