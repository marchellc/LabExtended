using Common.Extensions;

using LabExtended.Core.Hooking.Interfaces;
using LabExtended.Core.Profiling;

using LabExtended.Extensions;

namespace LabExtended.Core.Hooking
{
    public static class HookRunner
    {
        private static readonly ProfilerMarker _marker = new ProfilerMarker("Hooks (Execution)");

        public static T RunCancellable<T>(object eventObject, T cancellation)
        {
            cancellation = RunEvent(eventObject, cancellation);

            if (eventObject is ICancellableEvent<T> cancellableEvent)
            {
                cancellableEvent.Cancellation = cancellation;
                return cancellableEvent.Cancellation;
            }

            return cancellation;
        }

        public static void RunEvent(object eventObject)
            => RunEvent<object>(eventObject, null);

        public static T RunEvent<T>(object eventObject, T returnValue = default)
        {
            var type = eventObject.GetType();

            _marker.MarkStart(type.FullName);

            try
            {
                if (HookManager.PredefinedDelegates.TryGetValue(type, out var predefinedDelegates))
                {
                    foreach (var predefinedDelegate in predefinedDelegates)
                    {
                        try
                        {
                            predefinedDelegate(eventObject);
                        }
                        catch (Exception ex)
                        {
                            ExLoader.Error("Hooking API", $"An error occured while executing predefined delegate &3{predefinedDelegate.Method.ToName()}&r:\n{ex.ToColoredString()}");
                        }
                    }
                }

                if (HookManager._activeHooks.TryGetValue(type, out var hooks))
                    returnValue = RunInternal(eventObject, hooks, returnValue);

                if (HookManager._activeDelegates.TryGetValue(type, out var hookDelegateObjects))
                {
                    foreach (var hookDelegateObject in hookDelegateObjects)
                    {
                        try
                        {
                            var value = hookDelegateObject.Field.Get<Delegate>();

                            if (value is null)
                            {
                                ExLoader.Warn("Hooking API", $"Failed to get delegate value of event &3{hookDelegateObject.Event.ToName()}&r");
                                continue;
                            }

                            if (value is Action action)
                            {
                                action();
                                continue;
                            }

                            value.DynamicInvoke(eventObject);
                        }
                        catch (Exception ex)
                        {
                            ExLoader.Error("Hooking API", $"An error occured while running custom delegates (&3{hookDelegateObject.Event.ToName()}&r)!\n{ex.ToColoredString()}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExLoader.Error("Hooking API", $"Failed to run vanilla event &3{type.Name}&r:\n{ex.ToColoredString()}");
            }

            _marker.MarkEnd();
            return returnValue;
        }

        private static T RunInternal<T>(object eventObject, IEnumerable<HookInfo> hooks, T returnValue)
        {
            var syncResult = returnValue;

            foreach (var hook in hooks)
            {
                try
                {
                    var syncFinished = false;
                    var syncStart = DateTime.Now;

                    hook.Runner.OnEvent(eventObject, hook, hook.Binder, (hasFailed, hasTimedOut, exception, result) =>
                    {
                        // lets be safe .. it wont deadlock, but we dont need added latency
                        try
                        {
                            if (result != null && result is T methodValue)
                                returnValue = methodValue;
                        }
                        catch { }

                        syncFinished = true;

                        if (exception != null)
                        {
                            ExLoader.Error("Hooking API", $"Hook &3{hook.Method.ToName()}&r threw an error:\n{exception.ToColoredString()}");
                            return;
                        }

                        if (hasTimedOut)
                        {
                            ExLoader.Warn("Hooking API", $"Hook &3{hook.Method.ToName()}&r has timed out!");
                            return;
                        }

                        if (hasFailed)
                        {
                            ExLoader.Warn("Hooking API", $"Hook &3{hook.Method.ToName()}&r has failed{(exception != null ? $"!\n{exception.ToColoredString()}" : " with an unknown reason.")}");
                            return;
                        }
                    });

                    while (!syncFinished)
                    {
                        if ((DateTime.Now - syncStart).TotalSeconds >= 2.5)
                        {
                            ExLoader.Warn("Hooking API", $"Hook &3{hook.Method.ToName()}&r has timed out!");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExLoader.Error("Hooking API", $"Failed to start hook execution on &3{hook.Method.ToName()}&r!\n{ex.ToColoredString()}");
                }
            }

            return syncResult;
        }
    }
}