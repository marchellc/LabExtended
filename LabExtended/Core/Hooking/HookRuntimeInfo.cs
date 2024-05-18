using LabExtended.Core.Events;

namespace LabExtended.Core.Hooking
{
    public struct HookRuntimeInfo
    {
        public List<HookEventObject> EventObjects { get; }
        public HookEvent Event { get; }

        public HookRuntimeInfo(HookEvent hookEvent, List<HookEventObject> eventObjects)
        {
            Event = hookEvent;
            EventObjects = eventObjects;
        }
    }
}