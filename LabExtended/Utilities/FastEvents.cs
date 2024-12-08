using HarmonyLib;

using LabExtended.API.Collections.Locked;

using LabExtended.Core;
using LabExtended.Extensions;

using System.Reflection;

namespace LabExtended.Utilities
{
    public static class FastEvents<THandler> where THandler : Delegate
    {
        public struct DefinedEvent
        {
            public readonly AccessTools.FieldRef<object, THandler> Getter;
            public readonly Func<object, object[], object> Invoker;
            public readonly EventInfo Event;

            public DefinedEvent(AccessTools.FieldRef<object, THandler> getter, Func<object, object[], object> invoker, EventInfo ev)
            {
                Getter = getter;
                Invoker = invoker;
                Event = ev;
            }
        }

        private static readonly LockedHashSet<DefinedEvent> _definedEvents = new LockedHashSet<DefinedEvent>();

        public static void DefineEvent(Type type)
            => DefineEvent(type.FindEvent<THandler>());

        public static void DefineEvent(Type type, string eventName)
            => DefineEvent(type.FindEvent(eventName));

        public static void DefineEvent(EventInfo ev)
        {
            if (_definedEvents.Any(e => e.Event == ev))
                return;

            var fieldRef = AccessTools.FieldRefAccess<THandler>(ev.DeclaringType, ev.Name);
            var evInvoker = FastReflection.ForDelegate(ev.EventHandlerType, ev.EventHandlerType.FindMethod("Invoke"));

            _definedEvents.Add(new DefinedEvent(fieldRef, evInvoker, ev));
        }

        public static void InvokeEvent(Type type, string eventName, object target, params object[] args)
        {
            if (!_definedEvents.TryGetFirst(e => e.Event.DeclaringType == type && e.Event.Name == eventName, out var definedEvent))
                throw new InvalidOperationException($"Event '{type.FullName}.{eventName}' has not been registered");

            var value = definedEvent.Getter(target);

            if (value is null)
                return;

            try
            {
                definedEvent.Invoker(value, args);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Fast Events", $"Event &3{definedEvent.Event.GetMemberName()}&r caught an exception while executing:\n{ex.ToColoredString()}");
            }
        }

        public static void InvokeEvent(EventInfo ev, object target, params object[] args)
        {
            if (!_definedEvents.TryGetFirst(e => e.Event == ev, out var definedEvent))
                throw new InvalidOperationException($"Event '{ev.GetMemberName()}' has not been registered");

            var value = definedEvent.Getter(target);

            if (value is null)
                return;

            try
            {
                definedEvent.Invoker(value, args);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Fast Events", $"Event &3{ev.GetMemberName()}&r caught an exception while executing:\n{ex.ToColoredString()}");
            }
        }
    }
}