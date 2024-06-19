using Common.Extensions;

using LabExtended.Core.Events;
using LabExtended.Core.Hooking.Binders;

using NorthwoodLib.Pools;

namespace LabExtended.Core.Hooking
{
    public static class HookUtils
    {
        public static List<HookEventObject> GetBinding(HookEvent hookEvent, List<HookInfo> activeHooks)
        {
            if (!activeHooks.Any(hook => hook.Binder is HookCustomParamBinder))
                return null;

            var list = ListPool<HookEventObject>.Shared.Rent();
            var props = ((object)(hookEvent is HookWrapper wrapper ? wrapper.Event : hookEvent)).GetType().GetAllProperties();

            foreach (var prop in props)
            {
                var propName = prop.Name;
                var propValue = prop.Get(hookEvent is HookWrapper evWrapper ? evWrapper.Event : hookEvent);

                list.Add(new HookEventObject(propName, propValue, prop));
            }

            return list;
        }
    }
}