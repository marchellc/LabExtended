using LabExtended.Core.Events;

using PluginAPI.Events;

namespace LabExtended.Core.Hooking
{
    public class HookWrapper<T> : HookEvent
        where T : Event
    {
        public T Event { get; internal set; }

        public HookWrapper(T vanillaEvent)
            => Event = vanillaEvent;
    }
}