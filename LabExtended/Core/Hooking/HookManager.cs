using LabExtended.Core.Hooking.Executors;
using LabExtended.Core.Hooking.Binders;
using LabExtended.Core.Events;

using System.Reflection;

using Common.Utilities;
using Common.Extensions;

using MEC;

using NorthwoodLib.Pools;
using Common;

namespace LabExtended.Core.Hooking
{
    public static class HookManager
    {
        private static readonly Dictionary<Type, List<HookInfo>> _activeHooks = new Dictionary<Type, List<HookInfo>>();
        private static readonly Dictionary<Type, List<HookDelegate>> _eventDelegates = new Dictionary<Type, List<HookDelegate>>();

        public static HookNoParamBinder NoParamBinder => HookNoParamBinder.Instance;

        public static HookCoroutineExecutor CoroutineExecutor => HookCoroutineExecutor.Instance;
        public static HookClassicExecutor ClassicExecutor => HookClassicExecutor.Instance;
        public static HookTaskExecutor TaskExecutor => HookTaskExecutor.Instance;

        public static T ExecuteCancellable<T>(HookEvent hookEvent) where T : ICancellableEvent<T>
            => ((ICancellableEvent<T>)Execute(hookEvent)).IsCancelled;

        public static T Execute<T>(T hookEvent) where T : HookEvent
        {
            if (hookEvent is null)
                throw new ArgumentNullException(nameof(hookEvent));

            try
            {
                var hookType = hookEvent.GetType();

                if (!_activeHooks.TryGetValue(hookType, out var activeHooks) || activeHooks.Count < 1)
                    return hookEvent;

                var hookStart = DateTime.Now;
                var hookIndex = 0;
                var hookRuntime = new HookRuntimeInfo(hookEvent, HookUtils.GetBinding(hookEvent, activeHooks));
                var hookStatus = false;

                void HookCallback(HookResult hookResult)
                {
                    try
                    {
                        if (hookResult.Type != HookResultType.Success)
                        {
                            if (hookResult.Type is HookResultType.TimedOut)
                                ExLoader.Warn("Hook Manager", $"Hook '{activeHooks[hookIndex].Target.ToName()}' has timed out while executing event '{hookType.Name}'");
                            else if (hookResult.Type is HookResultType.Error)
                                ExLoader.Error("Hook Manager", $"Hook '{activeHooks[hookIndex].Target.ToName()}' has caught an error while executing event '{hookType.Name}'{(hookResult.ReturnedValue != null ? $":\n{hookResult.ReturnedValue}" : "")}");
                            else
                                ExLoader.Debug("Hook Manager", $"Hook '{activeHooks[hookIndex].Target.ToName()}' has finished executing.");
                        }

                        if (hookEvent.NextHook != null)
                        {
                            hookIndex++;
                            hookEvent.SyncHooks(activeHooks[hookIndex], (hookIndex + 1).IsValidIndex(activeHooks.Count) ? activeHooks[hookIndex + 1] : null);
                            hookEvent.CurrentHook.Execute(hookRuntime, HookCallback);
                        }
                        else
                        {
                            hookStatus = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        hookStatus = true;
                        ExLoader.Error("Hook Manager", $"Callback caught an exception while executing after index '{hookIndex}':\n{ex}");
                    }
                }

                hookEvent.SyncHooks(activeHooks[hookIndex], (hookIndex + 1).IsValidIndex(activeHooks.Count) ? activeHooks[hookIndex + 1] : null);
                activeHooks[hookIndex].Execute(hookRuntime, HookCallback);

                while (!hookStatus)
                {
                    if ((DateTime.Now - hookStart).TotalMilliseconds >= 2000)
                    {
                        ExLoader.Warn("Hook Manager", $"Timed out while executing event '{hookType.Name}' (current hook index: {hookIndex})");
                        break;
                    }
                }

                if (hookRuntime.EventObjects != null)
                    ListPool<HookEventObject>.Shared.Return(hookRuntime.EventObjects);

                hookEvent.SyncHooks(null, null);

                if (_eventDelegates.TryGetValue(hookType, out var hookDelegates))
                {
                    ExLoader.Debug("Hook Manager", $"Found {hookDelegates.Count} delegates for event '{hookType.Name}'");

                    var delegateArgs = new object[] { hookEvent };

                    foreach (var hookDelegate in hookDelegates)
                    {
                        try
                        {
                            ExLoader.Debug("Hook Manager", $"Executing delegate '{hookDelegate.Event.DeclaringType.FullName}::{hookDelegate.Event.Name}'");

                            var delegateValue = hookDelegate.Field.Get<MulticastDelegate>();

                            if (delegateValue is null)
                                continue;

                            delegateValue.DynamicInvoke(delegateArgs);
                        }
                        catch (Exception ex)
                        {
                            ExLoader.Error("Hook Manager", $"Caught an error while executing delegate '{hookDelegate.Event.Name}' in {hookDelegate.Event.DeclaringType.FullName}:\n{ex}");
                        }
                    }

                    if (delegateArgs[0] != null)
                        hookEvent = (T)delegateArgs[0];
                }

                ExLoader.Debug("Hook Manager", $"Finished executing event '{hookType.Name}' in {(DateTime.Now - hookStart).TotalMilliseconds} ms.");
            }
            catch (Exception ex)
            {
                ExLoader.Error("Hook Manager", $"Caught an exception while calling event '{hookEvent.GetType().FullName}':\n{ex}");
            }

            return hookEvent;
        }

        public static void RegisterCustomDelegates(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var ev in type.GetAllEvents())
                {
                    if (!ev.IsMulticast)
                        continue;

                    if (ev.EventHandlerType.GetGenericTypeDefinition() != typeof(Action<>))
                        continue;

                    var genericArgs = ev.EventHandlerType.GetGenericArguments();

                    if (genericArgs.Length != 1 || !genericArgs[0].InheritsType<HookEvent>())
                        continue;

                    var field = type.Field(ev.Name);

                    if (field is null)
                        continue;

                    if (!_eventDelegates.TryGetValue(genericArgs[0], out var hookDelegates))
                        _eventDelegates[genericArgs[0]] = hookDelegates = new List<HookDelegate>();

                    if (hookDelegates.Any(del => del.Event == ev && del.Field == field))
                        continue;

                    hookDelegates.Add(new HookDelegate(ev, field));
                    ExLoader.Debug("Hook Manager", $"Registered custom delegate for event '{genericArgs[0].Name}': {ev.DeclaringType.FullName}::{ev.Name}");
                }
            }
        }

        public static void UnregisterCustomDelegates(Assembly assembly)
        {
            foreach (var eventType in _eventDelegates.Keys)
                _eventDelegates[eventType].RemoveAll(del => del.Event.DeclaringType.Assembly == assembly);
        }

        public static void RegisterFromThis()
            => RegisterFrom(Assembly.GetCallingAssembly());

        public static void RegisterFrom(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                RegisterFrom(type);
        }

        public static void RegisterFrom(Type type, object typeInstance = null)
        {
            foreach (var method in type.GetAllMethods())
                RegisterFromInternal(method, typeInstance, false, false);
        }

        public static bool RegisterFrom(MethodInfo method, object typeInstance = null)
            => RegisterFromInternal(method, typeInstance, true, true);

        public static bool RegisterFrom<TEvent>(Action<TEvent> hook) where TEvent : HookEvent
            => RegisterFromInternal(hook.Method, hook.Target, true, true);

        public static void UnregisterFromThis()
            => UnregisterFrom(Assembly.GetCallingAssembly());

        public static void UnregisterFrom(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                UnregisterFrom(type);
        }

        public static void UnregisterFrom(Type type, object typeInstance = null)
        {
            foreach (var method in type.GetAllMethods())
                UnregisterFromInternal(method, typeInstance);
        }

        public static void UnregisterFrom(MethodInfo method, object typeInstance = null)
            => UnregisterFromInternal(method, typeInstance);

        public static void UnregisterFrom<TEvent>(Action<TEvent> hook) where TEvent : HookEvent
            => UnregisterFromInternal(hook.Method, hook.Target);

        public static bool TryGetExecutor(MethodInfo method, out HookExecutor executor)
        {
            if (method.ReturnType == typeof(void))
            {
                executor = ClassicExecutor;
                return true;
            }

            if (method.ReturnType == typeof(CoroutineHandle) || method.ReturnType == typeof(IEnumerator<float>))
            {
                executor = CoroutineExecutor;
                return true;
            }

            if (method.ReturnType == typeof(Task) || (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
            {
                executor = TaskExecutor;
                return true;
            }

            executor = null;
            return false;
        }

        public static bool TryGetBinder(Type targetEvent, ParameterInfo[] parameters, bool log, out HookBinder binder)
        {
            if (parameters.Length == 0)
            {
                binder = NoParamBinder;
                return true;
            }

            if (parameters.Length == 1 && parameters[0].ParameterType.InheritsType<HookEvent>())
            {
                binder = new HookEventParamBinder();
                return true;
            }

            var properties = targetEvent.GetAllProperties();

            foreach (var parameter in parameters)
            {
                if (!properties.TryGetFirst(prop => prop.PropertyType == parameter.ParameterType && prop.Name.ToLower() == parameter.Name.ToLower(), out var targetProperty))
                {
                    if (log)
                        ExLoader.Warn("Hook Manager", $"Failed to find a matching event property for parameter '{parameter.ParameterType.FullName} {parameter.Name}' in event '{targetEvent.FullName}'");

                    binder = null;
                    return false;
                }
            }

            binder = new HookCustomParamBinder(parameters);
            return true;
        }

        private static bool RegisterFromInternal(MethodInfo method, object typeInstance, bool log, bool bypass)
        {
            if (method.DeclaringType == typeof(HookManager))
                return false;

            if (!method.IsStatic && !method.DeclaringType.IsValidInstance(typeInstance, false))
            {
                if (log)
                    ExLoader.Warn("Hook Manager", $"Attempted to register a non-static method without a valid type instance. ({method.ToName()})");

                return false;
            }

            if (!bypass)
            {
                if (method.HasAttribute<HookIgnoreAttribute>())
                {
                    ExLoader.Debug("Hook Manager", $"Ignoring hook method {method.ToName()} - found a HookIgnoreAttribute.");
                    return false;
                }

                if (method.DeclaringType.HasAttribute<HookIgnoreAttribute>() && !method.HasAttribute<HookEventAttribute>())
                {
                    ExLoader.Debug("Hook Manager", $"Ignoring hook method {method.ToName()} - found a HookIgnoreAttribute on declaring class.");
                    return false;
                }
            }

            var parameters = method.Parameters();

            var hookEventAttribute = method.GetCustomAttribute<HookEventAttribute>();
            var hookEventType = default(Type);

            if (hookEventAttribute != null && hookEventAttribute.Type != null)
                hookEventType = hookEventAttribute.Type;
            else
            {
                if (parameters.Length == 1 && parameters[0].ParameterType.InheritsType<HookEvent>())
                    hookEventType = parameters[0].ParameterType;
                else
                {
                    if (log)
                        ExLoader.Warn("Hook Manager", $"Attempted to register a method with an unknown target event - you need to specify the event via the HookEvent attribute or in the method's overload. ({method.ToName()}");

                    return false;
                }
            }

            if (hookEventType is null)
            {
                if (log)
                    ExLoader.Warn("Hook Manager", $"Failed to recognize targeted event type by method '{method.ToName()}'");

                return false;
            }

            if (!TryGetBinder(hookEventType, parameters, log, out var binder))
            {
                ExLoader.Warn("Hook Manager", $"Failed to match a parameter binder to method '{method.ToName()}'!");
                return false;
            }

            if (!TryGetExecutor(method, out var executor))
            {
                ExLoader.Warn("Hook Manager", $"Failed to match an executor to method '{method.ToName()}' - this most likely means that it has an invalid return type (which must be either void, a MEC CoroutineHandle or a MEC IEnumerator<float>).");
                return false;
            }

            if (!_activeHooks.TryGetValue(hookEventType, out var activeHooks))
            {
                _activeHooks[hookEventType] = activeHooks = new List<HookInfo>();
                RegisterDelegates(hookEventType);
            }

            if (activeHooks.Any(hook => hook.Target == method && hook.Instance.IsEqualTo(typeInstance)))
            {
                ExLoader.Warn("Hook Manager", $"Attempted to register a duplicate hook: {method.ToName()}");
                return false;
            }

            var hookPriority = HookPriority.Normal;

            if (hookEventAttribute != null)
                hookPriority = hookEventAttribute.Priority;

            if ((hookPriority is HookPriority.AlwaysLast || hookPriority is HookPriority.AlwaysFirst) && activeHooks.Any(hook => hook.Priority == hookPriority))
            {
                if (hookPriority is HookPriority.AlwaysLast)
                    hookPriority = HookPriority.Lowest;
                else
                    hookPriority = HookPriority.Highest;

                ExLoader.Warn("Hook Manager", $"Hook '{method.ToName()}' tried to register it's priority as '{hookPriority}', but another hook has already claimed that position, it's priority will be set to {hookPriority}. This might break it's functionality!");
            }

            var hookInfo = new HookInfo(executor, binder, hookPriority, hookEventAttribute?.SyncOptions, method, typeInstance);

            activeHooks.Add(hookInfo);
            _activeHooks[hookEventType] = activeHooks = activeHooks.OrderBy(hook => (short)hook.Priority).ToList();

            ExLoader.Debug("Hook Manager", $"Registered a new hook: {method.ToName()} (event: {hookEventType.Name} [{activeHooks.Count}])");
            return true;
        }

        private static bool UnregisterFromInternal(MethodInfo method, object typeInstance)
        {
            var total = 0;

            foreach (var hookPair in _activeHooks)
                total += hookPair.Value.RemoveAll(hook => hook.Target == method && hook.Instance.IsEqualTo(typeInstance));

            if (total > 0)
            {
                ReorderHooks();
                ExLoader.Debug("Hook Manager", $"Unregistered hook '{method.ToName()}'");
            }

            return total > 0;
        }

        private static void ReorderHooks()
        {
            foreach (var hookType in _activeHooks.Keys)
            {
                var hooks = _activeHooks[hookType];

                if (hooks.Count < 1)
                    continue;

                _activeHooks[hookType] = hooks.OrderBy(hook => (short)hook.Priority).ToList();
            }
        }

        private static void RegisterDelegates(Type eventType)
        {
            var types = ModuleInitializer.SafeQueryTypes();
            var delegateType = typeof(Action<>).MakeGenericType(eventType);

            if (!_eventDelegates.TryGetValue(eventType, out var hookDelegates))
                _eventDelegates[eventType] = hookDelegates = new List<HookDelegate>();

            foreach (var type in types)
            {
                var events = type.GetAllEvents();

                foreach (var ev in events)
                {
                    if (!ev.IsMulticast || ev.EventHandlerType != delegateType)
                        continue;

                    var field = type.Field(ev.Name);

                    if (field is null)
                        continue;

                    if (hookDelegates.Any(del => del.Event == ev && del.Field == field))
                        continue;

                    hookDelegates.Add(new HookDelegate(ev, field));
                }
            }
        }
    }
}