using LabExtended.API.Collections.Locked;
using LabExtended.Extensions;

using System.Reflection;

namespace LabExtended.Utilities
{
    public static class EventUtils<THandler> where THandler : Delegate
    {
        public struct DefinedEvent
        {
            public readonly Func<object, THandler> Getter;
            public readonly EventInfo Event;

            public DefinedEvent(Func<object, THandler> getter, EventInfo ev)
            {
                Getter = getter;
                Event = ev;
            }
        }

        private static readonly LockedHashSet<DefinedEvent> _definedEvents = new LockedHashSet<DefinedEvent>();

        public static void DefineEvent(Type type, Func<object, THandler> getter = null)
            => DefineEvent(type.FindEvent<THandler>(), getter);

        public static void DefineEvent(Type type, string eventName, Func<object, THandler> getter = null)
            => DefineEvent(type.FindEvent(eventName), getter);

        public static void DefineEvent(EventInfo ev, Func<object, THandler> getter = null)
        {
            if (ev is null)
                throw new ArgumentNullException(nameof(getter));

            if (_definedEvents.Any(e => e.Event == ev))
                return;

            if (getter is null)
            {
                var field = ev.DeclaringType.FindField(ev.Name);

                if (field is null)
                    throw new Exception($"Failed to find event field.");

                getter = target =>
                {
                    var result = field.GetValue(target);

                    if (result != null)
                        return (THandler)result;

                    return null;
                };
            }

            _definedEvents.Add(new DefinedEvent(getter, ev));
        }

        public static void InvokeEvent(Type type, string eventName, object target, params object[] args)
        {
            if (!_definedEvents.TryGetFirst(e => e.Event.DeclaringType == type && e.Event.Name == eventName, out var definedEvent))
                throw new InvalidOperationException($"Event '{type.FullName}.{eventName}' has not been registered");

            definedEvent.Getter(target)?.DynamicInvoke(args);
        }

        public static void InvokeEvent(EventInfo ev, object target, params object[] args)
        {
            if (!_definedEvents.TryGetFirst(e => e.Event == ev, out var definedEvent))
                throw new InvalidOperationException($"Event '{ev.GetMemberName()}' has not been registered");

            definedEvent.Getter(target)?.DynamicInvoke(args);
        }
    }
}