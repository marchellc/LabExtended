using HarmonyLib;

using LabExtended.Extensions;

using System.Reflection;

namespace LabExtended.Utilities
{
    public static class FastEvents
    {
        public static FastEvent<THandler> DefineEvent<THandler>(Type type) where THandler : Delegate
            => DefineEvent<THandler>(type.FindEvent<THandler>());

        public static FastEvent<THandler> DefineEvent<THandler>(Type type, string eventName) where THandler : Delegate
            => DefineEvent<THandler>(type.FindEvent(eventName));

        public static FastEvent<THandler> DefineEvent<THandler>(EventInfo ev) where THandler : Delegate
        {
            var fieldRef = AccessTools.FieldRefAccess<THandler>(ev.DeclaringType, ev.Name);
            var evInvoker = FastReflection.ForDelegate(ev.EventHandlerType, ev.EventHandlerType.FindMethod("Invoke"));
            var fastEvent = new FastEvent<THandler>(fieldRef, evInvoker, ev);

            return fastEvent;
        }
    }
}