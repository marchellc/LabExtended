using LabExtended.Core.Hooking.Interfaces;
using LabExtended.Core.Profiling;

using LabExtended.Extensions;
using LabExtended.Utilities;

namespace LabExtended.Core.Hooking
{
    public static class HookRunner
    {
        private static readonly ProfilerMarker _marker = new ProfilerMarker("Hooks (Execution)");

        public static T RunCancellable<T>(object eventObject, T cancellation)
        {
            ArgumentUtils.Null(eventObject, "eventObject");

            if (eventObject is ICancellableEvent<T> cancellableEvent)
            {
                cancellableEvent.IsAllowed = cancellation;

                RunEvent(eventObject, cancellation);
                return cancellableEvent.IsAllowed;
            }
            else
            {
                return RunEvent(eventObject, cancellation);
            }
        }

        public static void RunEvent(object eventObject)
            => RunEvent<object>(eventObject, null);

        public static T RunEvent<T>(object eventObject, T returnValue = default)
        {
            ArgumentUtils.Null(eventObject, "eventObject");

            var type = eventObject.GetType();

            _marker.MarkStart(type.Name);

            try
            {
                if (HookManager.PredefinedDelegates.TryGetValue(type, out var predefinedDelegates) && predefinedDelegates.Count > 0)
                {
                    foreach (var predefinedDelegate in predefinedDelegates)
                    {
                        try
                        {
                            predefinedDelegate(eventObject);
                        }
                        catch (Exception ex)
                        {
                            ApiLoader.Error("Hooking API", $"An error occured while executing predefined delegate &3{predefinedDelegate.Method.GetMemberName()}&r:\n{ex.ToColoredString()}");
                        }
                    }
                }

                if (HookManager.PredefinedReturnDelegates.TryGetValue(type, out var predefinedReturnDelegates) && predefinedDelegates.Count > 0)
                {
                    foreach (var predefinedReturnDelegate in predefinedReturnDelegates)
                    {
                        try
                        {
                            var result = predefinedReturnDelegate(eventObject);

                            if (result != null && result is T castResult)
                                returnValue = castResult;
                        }
                        catch (Exception ex)
                        {
                            ApiLoader.Error("Hooking API", $"An error occured while executing predefined delegate &3{predefinedReturnDelegate.Method.GetMemberName()}&r:\n{ex.ToColoredString()}");
                        }
                    }
                }

                if (HookManager._activeHooks.TryGetValue(type, out var hooks) && hooks.Count > 0)
                    returnValue = RunInternal(eventObject, hooks, returnValue);

                if (HookManager._activeDelegates.TryGetValue(type, out var hookDelegateObjects) && hookDelegateObjects.Count > 0)
                {
                    var args = new object[] { eventObject };

                    foreach (var hookDelegateObject in hookDelegateObjects)
                    {
                        try
                        {
                            var value = hookDelegateObject.FieldGetter();

                            if (value is null || value is not Delegate del)
                            {
                                ApiLoader.Warn("Hooking API", $"Failed to get delegate value of event &3{hookDelegateObject.Event.GetMemberName()}&r");
                                continue;
                            }

                            if (value is Action action)
                            {
                                action();
                                continue;
                            }

                            hookDelegateObject.Invoker(null, args);
                        }
                        catch (Exception ex)
                        {
                            ApiLoader.Error("Hooking API", $"An error occured while running custom delegates (&3{hookDelegateObject.Event.GetMemberName()}&r)!\n{ex.ToColoredString()}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApiLoader.Error("Hooking API", $"Failed to run vanilla event &3{type.Name}&r:\n{ex.ToColoredString()}");
            }

            _marker.MarkEnd();
            return returnValue;
        }

        private static T RunInternal<T>(object eventObject, List<HookInfo> hooks, T returnValue)
        {
            for (int i = 0; i < hooks.Count; i++)
            {
                try
                {
                    var hookResult = hooks[i].Run(eventObject);

                    if (hookResult != null && hookResult is T castValue)
                        returnValue = castValue;
                }
                catch (Exception ex)
                {
                    ApiLoader.Error("Hooking API", $"Failed to run hook &3{hooks[i].Method.GetMemberName()}&r due to an exception:\n{ex.ToColoredString()}");
                }
            }

            return returnValue;
        }
    }
}