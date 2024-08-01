using LabExtended.Attributes;
using LabExtended.Extensions;
using LabExtended.Commands;

using LabExtended.Events;
using LabExtended.Events.Player;
using LabExtended.Events.Other;

using LabExtended.API;
using LabExtended.API.Input;
using LabExtended.API.Collections.Locked;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Firearms;

using LabExtended.Core.Hooking.Binders;
using LabExtended.Core.Hooking.Enums;
using LabExtended.Core.Hooking.Executors;
using LabExtended.Core.Hooking.Interfaces;
using LabExtended.Core.Hooking.Objects;
using LabExtended.Core.Events;

using MEC;

using PluginAPI.Events;
using PluginAPI.Core.Attributes;

using System.Reflection;
using LabExtended.Core.Performance;

namespace LabExtended.Core.Hooking
{
    public static class HookManager
    {
        public static LockedDictionary<Type, List<Func<object, object>>> PredefinedReturnDelegates { get; } = new LockedDictionary<Type, List<Func<object, object>>>()
        {
            [typeof(RemoteAdminCommandEvent)] = new List<Func<object, object>>()
            {
                ev => CustomCommand.InternalHandleRemoteAdminCommand((RemoteAdminCommandEvent)ev)
            },

            [typeof(ConsoleCommandEvent)] = new List<Func<object, object>>()
            {
                ev => CustomCommand.InternalHandleConsoleCommand((ConsoleCommandEvent)ev)
            },

            [typeof(PlayerGameConsoleCommandEvent)] = new List<Func<object, object>>()
            {
                ev => CustomCommand.InternalHandleGameConsoleCommand((PlayerGameConsoleCommandEvent)ev)
            },
        };

        public static LockedDictionary<Type, List<Action<object>>> PredefinedDelegates { get; } = new LockedDictionary<Type, List<Action<object>>>()
        {
            [typeof(RoundStartEvent)] = new List<Action<object>>()
            {
                _ => InternalEvents.InternalHandleRoundStart(),
                _ => Camera.OnRoundStart(),
                _ => RoundEvents.InvokeStarted(),
            },

            [typeof(RoundEndEvent)] = new List<Action<object>>()
            {
                _ => RoundEvents.InvokeEnded()
            },

            [typeof(RoundRestartEvent)] = new List<Action<object>>()
            {
                _ => InternalEvents.InternalHandleRoundRestart(),
                _ => Camera.OnRestart(),
                _ => RoundEvents.InvokeRestarted()
            },

            [typeof(WaitingForPlayersEvent)] = new List<Action<object>>()
            {
                _ => PerformanceWatcher.SubmitReport(),
                _ => InternalEvents.InternalHandleRoundWaiting(),
                _ => CustomItem.InternalHandleWaiting(),
                _ => RoundEvents.InvokeWaiting(),
                _ => InputHandler.OnWaiting()
            },

            [typeof(MapGeneratedEvent)] = new List<Action<object>>()
            {
                _ => InternalEvents.InternalHandleMapGenerated()
            },

            [typeof(PlayerDyingEvent)] = new List<Action<object>>()
            {
                ev => CustomItem.InternalHandleDying((PlayerDyingEvent)ev),
            },

            [typeof(PlayerTogglingNoClipArgs)] = new List<Action<object>>()
            {
                ev => InputHandler.OnTogglingNoClip((PlayerTogglingNoClipArgs)ev),
            },

            [typeof(ProcessingFirearmRequestArgs)] = new List<Action<object>>()
            {
                ev => CustomFirearm.InternalOnProcessingRequest((ProcessingFirearmRequestArgs)ev),
            },

            [typeof(ProcessedFirearmRequestArgs)] = new List<Action<object>>()
            {
                ev => CustomFirearm.InternalOnProcessedRequested((ProcessedFirearmRequestArgs)ev),
            },

            [typeof(PlayerAuthorizingShotArgs)] = new List<Action<object>>()
            {
                ev => CustomFirearm.InternalOnAuthorizingShot((PlayerAuthorizingShotArgs)ev),
            },

            [typeof(PlayerPerformingShotArgs)] = new List<Action<object>>()
            {
                ev => CustomFirearm.InternalOnPerformingShot((PlayerPerformingShotArgs)ev),
            },
        };

        internal static readonly LockedDictionary<Type, List<HookInfo>> _activeHooks = new LockedDictionary<Type, List<HookInfo>>();
        internal static readonly LockedDictionary<Type, List<HookDelegateObject>> _activeDelegates = new LockedDictionary<Type, List<HookDelegateObject>>();

        public static void RegisterAll()
            => RegisterAll(Assembly.GetCallingAssembly());

        public static void UnregisterAll()
            => UnregisterAll(Assembly.GetCallingAssembly());

        public static void RegisterAll(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                RegisterAll(type, null);
            }
        }

        public static void UnregisterAll(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                UnregisterAll(type, null);
            }
        }

        public static void RegisterAll(Type type, object typeInstance = null)
        {
            foreach (var method in type.GetAllMethods())
            {
                Register(method, typeInstance, false);
                RegisterDelegates(type, typeInstance);
            }
        }

        public static void UnregisterAll(Type type, object typeInstance = null)
        {
            foreach (var method in type.GetAllMethods())
            {
                Unregister(method, typeInstance);
                UnregisterDelegates(type, typeInstance);
            }
        }

        public static void Unregister<T>(Action<T> handler)
            => _activeHooks.ForEach(p => p.Value.RemoveAll(h => h.Method == handler.Method && h.Instance.IsEqualTo(handler.Target)));

        public static void Unregister<T, TReturn>(Func<T, TReturn> handler) where TReturn : struct
            => _activeHooks.ForEach(p => p.Value.RemoveAll(h => h.Method == handler.Method && h.Instance.IsEqualTo(handler.Target)));

        public static void Unregister(MethodInfo method, object typeInstance = null)
            => _activeHooks.ForEach(p => p.Value.RemoveAll(h => h.Method == method && h.Instance.IsEqualTo(typeInstance)));

        public static void UnregisterDelegates(Type type, object typeInstance = null)
            => _activeDelegates.ForEach(p => p.Value.RemoveAll(h => h.Event.DeclaringType != null && h.Event.DeclaringType == type && h.TypeInstance.IsEqualTo(typeInstance)));

        public static void Register<T>(Action<T> handler)
            => RegisterInternal(handler.Method, handler.Target, true, true, null, null);

        public static void Register<T, TReturn>(Func<T, TReturn> handler) where TReturn : struct
            => RegisterInternal(handler.Method, handler.Target, true, true, null, null);

        public static void Register(MethodInfo method, object typeInstance = null, bool logAdditionalInfo = false)
            => RegisterInternal(method, typeInstance, logAdditionalInfo, false, method.GetCustomAttribute<HookDescriptorAttribute>(), method.GetCustomAttribute<PluginEvent>());

        public static void RegisterDelegates(Type type, object typeInstance)
        {
            foreach (var eventInfo in type.GetAllEvents())
            {
                var pluginEvent = default(PluginEvent);

                if (_activeDelegates.Any(p => p.Value.Any(d => d.Event == eventInfo && d.TypeInstance.IsEqualTo(typeInstance))))
                    continue;

                var eventField = type.FindField(eventInfo.Name);

                if (eventField is null || !eventInfo.IsMulticast)
                    continue;

                if (!eventInfo.HasAttribute<HookDescriptorAttribute>(out var hookDescriptorAttribute) && !eventInfo.HasAttribute(out pluginEvent))
                    continue;

                var eventType = default(Type);
                var eventPriority = HookPriority.Normal;

                if (eventInfo.EventHandlerType == typeof(Action))
                {
                    if (hookDescriptorAttribute.EventOverride is null && pluginEvent is null)
                        continue;

                    var attrType = hookDescriptorAttribute?.EventOverride;

                    if (attrType is null && pluginEvent != null)
                    {
                        if (!EventManager.Events.TryGetValue(pluginEvent.EventType, out var pluginEv))
                            continue;

                        attrType = pluginEv.EventArgType;
                    }

                    if (attrType is null)
                        continue;

                    if (!TryValidateEventType(attrType))
                        continue;

                    eventType = attrType;
                    eventPriority = hookDescriptorAttribute?.Priority ?? HookPriority.Normal;
                }
                else if (eventType is null)
                {
                    if (eventInfo.EventHandlerType.IsGenericType && (eventInfo.EventHandlerType.GetGenericTypeDefinition() == typeof(Action<>) || eventInfo.EventHandlerType.GetGenericTypeDefinition() == typeof(Func<,>)))
                    {
                        var genericArgs = eventInfo.EventHandlerType.GetGenericArguments();

                        if (genericArgs.Length != 1)
                            continue;

                        if (!TryValidateEventType(genericArgs[0]))
                            continue;

                        eventType = genericArgs[0];
                        eventPriority = HookPriority.Normal;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (eventType is null)
                    continue;

                if (!_activeDelegates.TryGetValue(eventType, out var hookDelegateObjects))
                    _activeDelegates[eventType] = hookDelegateObjects = new List<HookDelegateObject>();

                if ((eventPriority is HookPriority.AlwaysFirst || eventPriority is HookPriority.AlwaysLast) && hookDelegateObjects.Any(h => h.Priority == eventPriority))
                    eventPriority = eventPriority is HookPriority.AlwaysFirst ? HookPriority.Highest : HookPriority.Lowest;

                hookDelegateObjects.Add(new HookDelegateObject(eventInfo, eventField, typeInstance, eventPriority));

                var ordered = hookDelegateObjects.OrderBy(h => (short)h.Priority).ToList();

                hookDelegateObjects.Clear();
                hookDelegateObjects.AddRange(ordered);

                ExLoader.Debug("Hooking API", $"Registered custom delegate: &3{eventInfo.GetMemberName()}&r (&6{eventType.Name}&r)");
            }
        }

        private static void RegisterInternal(MethodInfo method, object typeInstance, bool log, bool skipAttributes, HookDescriptorAttribute hookDescriptorAttribute, PluginEvent pluginEvent)
        {
            if (!skipAttributes && hookDescriptorAttribute is null && pluginEvent is null)
                return;

            if (!method.IsStatic && !method.DeclaringType.IsTypeInstance(typeInstance))
                return;

            if (_activeHooks.Any(p => p.Value.Any(h => h.Instance.IsEqualTo(typeInstance, true) && h.Method == method)))
            {
                ExLoader.Warn("Hooking API", $"Tried to register a duplicate hook: &3{method.GetMemberName()}&r");
                return;
            }

            var methodArgs = method.GetAllParameters();

            if (!TryGetEventType(hookDescriptorAttribute, pluginEvent, methodArgs, out var eventType))
            {
                ExLoader.Warn("Hooking API", $"Failed to recognize event type in method &3{method.GetMemberName()}&r");
                return;
            }

            if (!TryGetBinder(methodArgs, eventType, out var hookBinder))
            {
                ExLoader.Warn("Hooking API", $"Failed to get a valid overload binder for method &3{method.GetMemberName()}&r");
                return;
            }

            if (!TryGetRunner(method.ReturnType, out var hookRunner))
            {
                ExLoader.Warn("Hooking API", $"Failed to get a valid method runner for method &3{method.GetMemberName()}&r");
                return;
            }

            if (!_activeHooks.TryGetValue(eventType, out var hooks))
                _activeHooks[eventType] = hooks = new List<HookInfo>();

            var priority = hookDescriptorAttribute?.Priority ?? HookPriority.Normal;

            if ((priority is HookPriority.AlwaysFirst || priority is HookPriority.AlwaysLast) && hooks.Any(h => h.Priority == priority))
            {
                var newPriority = priority is HookPriority.AlwaysFirst ? HookPriority.Highest : HookPriority.Lowest;

                ExLoader.Warn("Hooking API", $"Hook &3{method.GetMemberName()}&r tried to register it's priority as &6{priority}&r, but that spot was already taken by another hook. It's new priority will be &6{newPriority}&r.");

                priority = newPriority;
            }

            var hook = new HookInfo(method, typeInstance, hookRunner, hookBinder, priority);

            hooks.Add(hook);

            var ordered = hooks.OrderBy(h => (short)h.Priority).ToList();

            hooks.Clear();
            hooks.AddRange(ordered);

            ExLoader.Debug("Hooking API", $"Registered a new hook: &3{method.GetMemberName()}&r (&6{eventType.FullName}&r) ({hooks.Count})");
        }

        private static bool TryGetRunner(Type returnType, out IHookRunner hookRunner)
        {
            if (returnType == typeof(CoroutineHandle) || returnType == typeof(IEnumerator<float>))
            {
                hookRunner = new CoroutineHookRunner();
                return true;
            }

            if (returnType == typeof(bool) || returnType == typeof(void) || returnType.InheritsType<IEventCancellation>())
            {
                hookRunner = new SimpleHookRunner();
                return true;
            }

            hookRunner = null;
            return false;
        }

        private static bool TryGetBinder(ParameterInfo[] methodArgs, Type eventType, out IHookBinder binder)
        {
            if (methodArgs.Length == 0)
            {
                binder = new EmptyOverloadBinder();
                return true;
            }

            if (methodArgs.Length == 1 && methodArgs[0].ParameterType == eventType)
            {
                binder = new SimpleOverloadBinder();
                return true;
            }

            var properties = eventType.GetAllProperties();
            var binding = new PropertyInfo[methodArgs.Length];

            for (int i = 0; i < methodArgs.Length; i++)
            {
                var arg = methodArgs[i];
                var property = default(PropertyInfo);

                for (int x = 0; x < properties.Length; x++)
                {
                    if (properties[x].Name.ToLower() == arg.Name.ToLower())
                    {
                        property = properties[x];
                        break;
                    }
                }

                if (property is null)
                {
                    ExLoader.Warn("Hooking API", $"Failed to find parameter &3{arg.Name}& (&6{arg.ParameterType.Name}&r) in event &3{eventType.Name}&r!");

                    binder = null;
                    return false;
                }

                binding[i] = property;
            }

            binder = new CustomOverloadBinder(methodArgs, binding);
            return true;
        }

        private static bool TryGetEventType(HookDescriptorAttribute hookDescriptorAttribute, PluginEvent pluginEvent, ParameterInfo[] methodArgs, out Type eventType)
        {
            if (hookDescriptorAttribute?.EventOverride != null && TryValidateEventType(hookDescriptorAttribute.EventOverride))
            {
                eventType = hookDescriptorAttribute.EventOverride;
                return true;
            }

            if (methodArgs.Length == 1 && TryValidateEventType(methodArgs[0].ParameterType))
            {
                eventType = methodArgs[0].ParameterType;
                return true;
            }

            if (pluginEvent != null && EventManager.Events.TryGetValue(pluginEvent.EventType, out var eventInfo))
            {
                eventType = eventInfo.EventArgType;
                return true;
            }

            eventType = null;
            return false;
        }

        private static bool TryValidateEventType(Type type)
            => type != null && type != typeof(HookCancellableEventBase<>) && type != typeof(HookBooleanCancellableEventBase) && ((type.InheritsType<IHookEvent>() && type != typeof(IHookEvent)) || type.InheritsType<IEventArguments>() && type != typeof(IEventArguments));
    }
}