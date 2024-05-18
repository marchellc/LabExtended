using LabExtended.Core.Hooking;

namespace LabExtended.Core.Events
{
    public class HookEvent
    {
        public HookInfo PreviousHook { get; private set; }
        public HookInfo CurrentHook { get; private set; }
        public HookInfo NextHook { get; private set; }

        internal void SyncHooks(HookInfo current, HookInfo next)
        {
            if (CurrentHook != null)
                PreviousHook = CurrentHook;

            CurrentHook = current;
            NextHook = next;
        }
    }
}