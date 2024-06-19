namespace LabExtended.Core.Hooking
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Event, AllowMultiple = false, Inherited = false)]
    public class HookEventAttribute : Attribute
    {
        public Type Type { get; }
        public HookPriority Priority { get; }
        public HookSyncOptions SyncOptions { get; }

        public HookEventAttribute(Type eventType = null, HookPriority priority = HookPriority.Normal, bool doNotWait = false, bool doNotKill = false, float timeout = -1f)
        {
            Type = eventType;
            Priority = priority;
            SyncOptions = new HookSyncOptions(doNotWait, doNotKill, timeout < 0f ? null : timeout);
        }
    }
}