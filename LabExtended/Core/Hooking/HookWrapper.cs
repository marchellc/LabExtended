using LabExtended.Core.Events;

using PluginAPI.Events;

namespace LabExtended.Core.Hooking
{
    public class HookWrapper : HookEvent
    {
        public IEventArguments Event { get; internal set; }

        public HookWrapper(IEventArguments vanillaEvent)
            => Event = vanillaEvent;
    }
}