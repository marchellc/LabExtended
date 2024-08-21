using HarmonyLib;

using LabExtended.API.Collections.Locked;
using LabExtended.Extensions;

using System.Reflection;

namespace LabExtended.Utilities
{
    public static class EventUtils<THandler> where THandler : Delegate
    {
        public struct DefinedEvent
        {
            public readonly AccessTools.FieldRef<object, THandler> Getter;
            public readonly EventInfo Event;

            public DefinedEvent(AccessTools.FieldRef<object, THandler> getter, EventInfo ev)
            {
                Getter = getter;
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

            _definedEvents.Add(new DefinedEvent(AccessTools.FieldRefAccess<THandler>($"{ev.DeclaringType.FullName}:{ev.Name}"), ev));
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