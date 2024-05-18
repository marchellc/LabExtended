namespace LabExtended.Core.Hooking
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HookEventAttribute : Attribute
    {
        public Type Type { get; }
        public HookPriority Priority { get; }
        public HookSyncOptionsValue SyncOptions { get; }

        public HookEventAttribute(Type eventType = null, HookPriority priority = HookPriority.Normal, bool doNotWait = false, bool doNotKill = false, float timeout = -1f)
        {
            Type = eventType;
            Priority = priority;
            SyncOptions = new HookSyncOptionsValue(doNotWait, doNotKill, timeout < 0f ? null : timeout);
        }
    }
}