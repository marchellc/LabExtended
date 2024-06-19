using LabExtended.Core.Hooking.Executors;
using LabExtended.Core.Hooking.Binders;
using LabExtended.Core.Events;

using LabExtended.Extensions;
using LabExtended.Utilities;

using System.Reflection;

using Common.Utilities;
using Common.Extensions;

using MEC;

using NorthwoodLib.Pools;

using Common;

using PluginAPI.Events;
using PluginAPI.Core.Attributes;

using HarmonyLib;

namespace LabExtended.Core.Hooking
{
    public static class HookManager
    {
        private static MethodInfo _patchMethod;
        private static MethodInfo _targetMethod;
        private static Harmony _harmony;

        private static readonly Dictionary<Type, List<HookInfo>> _activeHooks = new Dictionary<Type, List<HookInfo>>();
        private static readonly Dictionary<Type, List<HookDelegate>> _eventDelegates = new Dictionary<Type, List<HookDelegate>>();

        private static readonly Dictionary<Type, Action<object>> _predefinedDelegates = new Dictionary<Type, Action<object>>()
        {
            [typeof(RoundEndEvent)] = ev => RoundEvents.InvokeEnded((RoundEndEvent)ev),
            [typeof(RoundStartEvent)] = ev => RoundEvents.InvokeStarted((RoundStartEvent)ev),
            [typeof(RoundRestartEvent)] = ev => RoundEvents.InvokeRestarted((RoundRestartEvent)ev),
            [typeof(WaitingForPlayersEvent)] = ev => RoundEvents.InvokeWaiting((WaitingForPlayersEvent)ev)
        };

        public static HookNoParamBinder NoParamBinder => HookNoParamBinder.Instance;

        public static HookCoroutineExecutor CoroutineExecutor => HookCoroutineExecutor.Instance;
        public static HookClassicExecutor ClassicExecutor => HookClassicExecutor.Instance;

        internal static void Initialize()
        {
            var start = DateTime.Now;

            try
            {
                ExLoader.Info("Hook Manager", $"Initializing the hooking system ..");

                _harmony = new Harmony("com.exloader.hooking");
                _harmony.PatchAll();

                _patchMethod = typeof(HookManager).GetAllMethods().First(m => m.Name == "EventPrefix");
                _targetMethod = typeof(EventManager).GetAllMethods().First(m => m.Name == "ExecuteEvent" && m.ContainsGenericParameters);

                foreach (var type in ModuleInitializer.SafeQueryTypes())
                {
                    if (!type.InheritsType<IEventCancellation>() || type == typeof(IEventCancellation))
                        continue;

                    var origMethod = _targetMethod.MakeGenericMethod(type);
                    var newMethod = _patchMethod.MakeGenericMethod(type);

                    _harmony.Patch(origMethod, new HarmonyMethod(newMethod));
                }

                var orMethod = _targetMethod.MakeGenericMethod(typeof(bool));
                var neMethod = _patchMethod.MakeGenericMethod(typeof(bool));

                _harmony.Patch(orMethod, new HarmonyMethod(neMethod));
            }
            catch (Exception ex)
            {
                ExLoader.Error("Hook Manager", $"Patching NW API events failed!\n{ex.ToColoredString()}");
            }

            var end = DateTime.Now - start;

            ExLoader.Info("Hook Manager", $"Finished loading in &1{end.TotalMilliseconds} ms&r!");
        }

        public static T ExecuteCustomCancellable<T>(HookEvent hookEvent, T cancellation)
        {
            var cancellable = (ICancellableEvent<T>)hookEvent;
            cancellable.IsCancelled = cancellation;
            hookEvent = ExecuteCustom(hookEvent);
            return cancellable.IsCancelled;
        }

        public static T ExecuteVanilla<T>(IEventArguments vanillaEvent, T cancellation) where T : struct
        {
            try
            {
                if (typeof(T) == typeof(bool))
                    cancellation = (T)(object)true;
                else
                    cancellation = default;

                var hookType = vanillaEvent.GetType();

                if (_predefinedDelegates.TryGetValue(hookType, out var delegateInvoker))
                    delegateInvoker(vanillaEvent);

                if (!_activeHooks.TryGetValue(hookType, out var activeHooks) || activeHooks.Count < 1)
                    return cancellation;

                var hookStart = DateTime.Now;
                var hookIndex = 0;
                var hookStatus = false;
                var hookEvent = new HookWrapper(vanillaEvent);
                var hookRuntime = new HookRuntimeInfo(hookEvent, HookUtils.GetBinding(hookEvent, activeHooks));

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

                            if (hookResult.ReturnedValue != null && hookResult.ReturnedValue is T returnedCancellation)
                                cancellation = returnedCancellation;
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

                if (_eventDelegates.TryGetValue(hookType, out var hookDelegates))
                {
                    ExLoader.Debug("Hook Manager", $"Found {hookDelegates.Count} delegates for event '{hookType.Name}'");

                    var delegateArgs = new object[] { hookEvent.Event };

                    foreach (var hookDelegate in hookDelegates)
                    {
                        try
                        {
                            ExLoader.Debug("Hook Manager", $"Executing delegate '{hookDelegate.Event.DeclaringType.FullName}::{hookDelegate.Event.Name}'");

                            var delegateValue = hookDelegate.Field.Get<MulticastDelegate>();

                            if (delegateValue is null)
                                continue;

                            if (delegateValue is Action action)
                                action();
                            else
                                delegateValue.DynamicInvoke(delegateArgs);
                        }
                        catch (Exception ex)
                        {
                            ExLoader.Error("Hook Manager", $"Caught an error while executing delegate '{hookDelegate.Event.Name}' in {hookDelegate.Event.DeclaringType.FullName}:\n{ex}");
                        }
                    }
                }

                ExLoader.Debug("Hook Manager", $"Finished executing event '{hookType.Name}' in {(DateTime.Now - hookStart).TotalMilliseconds} ms.");
            }
            catch (Exception ex)
            {
                ExLoader.Error("Hook Manager", $"Caught an exception while calling event '{vanillaEvent.GetType().FullName}':\n{ex}");
            }

            return cancellation;
        }

        public static T ExecuteCustom<T>(T hookEvent) where T : HookEvent
        {
            if (hookEvent is null)
                throw new ArgumentNullException(nameof(hookEvent));

            try
            {
                var hookType = hookEvent.GetType();

                if (_predefinedDelegates.TryGetValue(hookType, out var delegateInvoker))
                    delegateInvoker(hookEvent);

                if (!_activeHooks.TryGetValue(hookType, out var activeHooks) || activeHooks.Count < 1)
                    return hookEvent;

                var hookStart = DateTime.Now;
                var hookIndex = 0;
                var hookStatus = false;
                var hookRuntime = new HookRuntimeInfo(hookEvent, HookUtils.GetBinding(hookEvent, activeHooks));

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

                            if (delegateValue is Action action)
                                action();
                            else
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
                    if (ev.HasAttribute<HookIgnoreAttribute>())
                        continue;

                    if (!ev.IsMulticast)
                        continue;

                    if (ev.EventHandlerType == typeof(Action))
                    {
                        if (!ev.HasAttribute<HookEventAttribute>(out var hookEventAttribute))
                            continue;

                        if (hookEventAttribute.Type is null || (!hookEventAttribute.Type.InheritsType<HookEvent>() && !hookEventAttribute.Type.InheritsType<IEventArguments>()))
                            continue;

                        var evField = type.Field(ev.Name);

                        if (evField is null)
                            continue;

                        if (!_eventDelegates.TryGetValue(hookEventAttribute.Type, out var evHookDelegates))
                            _eventDelegates[hookEventAttribute.Type] = evHookDelegates = new List<HookDelegate>();

                        if (evHookDelegates.Any(del => del.Event == ev && del.Field == evField))
                            continue;

                        evHookDelegates.Add(new HookDelegate(ev, evField));
                        ExLoader.Debug("Hook Manager", $"Registered custom delegate for event '{hookEventAttribute.Type.Name}': {ev.DeclaringType.FullName}::{ev.Name}");
                        continue;
                    }

                    if (!ev.EventHandlerType.IsConstructedGenericType || ev.EventHandlerType.GetGenericTypeDefinition() != typeof(Action<>))
                        continue;

                    var genericArgs = ev.EventHandlerType.GetGenericArguments();

                    if (genericArgs.Length != 1 || (!genericArgs[0].InheritsType<HookEvent>() && !genericArgs[0].InheritsType<IEventArguments>()))
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

            if (parameters.Length == 1 && (parameters[0].ParameterType.InheritsType<HookEvent>() || parameters[0].ParameterType.InheritsType<IEventArguments>()))
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

                if (method.DeclaringType.HasAttribute<HookIgnoreAttribute>() && !method.HasAttribute<HookEventAttribute>() && !method.HasAttribute<PluginEvent>())
                {
                    ExLoader.Debug("Hook Manager", $"Ignoring hook method {method.ToName()} - found a HookIgnoreAttribute on declaring class.");
                    return false;
                }
            }

            var parameters = method.Parameters();
            var pluginEventAttribute = method.GetCustomAttribute<PluginEvent>();

            var hookEventAttribute = method.GetCustomAttribute<HookEventAttribute>();
            var hookEventType = default(Type);

            if (hookEventAttribute != null && hookEventAttribute.Type != null)
                hookEventType = hookEventAttribute.Type;
            else
            {
                if (parameters.Length == 1 && (parameters[0].ParameterType.InheritsType<HookEvent>() || parameters[0].ParameterType.InheritsType<IEventArguments>()))
                    hookEventType = parameters[0].ParameterType;
                else if (pluginEventAttribute != null && EventManager.TypeToEvent.TryGetKey(pluginEventAttribute.EventType, out var pluginEventType))
                    hookEventType = pluginEventType;
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

            if (hookEventType.InheritsType<IEventArguments>() && !method.HasAttribute<HookEventAttribute>() && !method.HasAttribute<PluginEvent>())
                return false;

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
                    if (ev.HasAttribute<HookIgnoreAttribute>())
                        continue;

                    if (ev.EventHandlerType == typeof(Action) && ev.HasAttribute<HookEventAttribute>(out var hookEventAttribute))
                    {
                        if (hookEventAttribute.Type is null || (!hookEventAttribute.Type.InheritsType<HookEvent>() && !hookEventAttribute.Type.InheritsType<IEventArguments>()))
                            continue;

                        var evField = type.Field(ev.Name);

                        if (evField is null)
                            continue;

                        if (hookDelegates.Any(del => del.Event == ev && del.Field == evField))
                            continue;

                        hookDelegates.Add(new HookDelegate(ev, evField));
                        continue;
                    }

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

        private static bool EventPrefix<T>(IEventArguments args, ref T __result) where T : struct
        {
            if (ExLoader.Loader.Config.Hooks.BypassWhitelist.Contains(args.BaseType.ToString()))
                return true;

            if (ExLoader.Loader.Config.Hooks.DisableNwEvents && !ExLoader.Loader.Config.Hooks.DisableWhitelist.Contains(args.BaseType.ToString()))
                return false;

            __result = ExecuteVanilla(args, __result);
            return false;
        }
    }
}