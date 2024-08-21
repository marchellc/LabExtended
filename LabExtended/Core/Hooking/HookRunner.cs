using LabExtended.Core.Hooking.Interfaces;
using LabExtended.Core.Profiling;

using LabExtended.Extensions;

namespace LabExtended.Core.Hooking
{
    public static class HookRunner
    {
        private static readonly ProfilerMarker _marker = new ProfilerMarker("Hooks (Execution)");

        public static void RunEvent(object eventObject)
            => RunEvent<object>(eventObject, null);

        public static T RunEvent<T>(object eventObject, T returnValue = default)
        {
            if (eventObject is null)
                throw new ArgumentNullException(nameof(eventObject));

            var type = eventObject.GetType();
            var cancellable = eventObject as ICancellableEvent<T>;

            _marker.MarkStart(type.Name);

            if (cancellable != null)
                cancellable.IsAllowed = returnValue;

            try
            {
                if (HookManager.PredefinedDelegates.TryGetValue(type, out var predefinedDelegates) && predefinedDelegates != null && predefinedDelegates.Count > 0)
                {
                    foreach (var predefinedDelegate in predefinedDelegates)
                    {
                        if (predefinedDelegate is null || predefinedDelegate.Method is null)
                            continue;

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

                if (HookManager.PredefinedReturnDelegates.TryGetValue(type, out var predefinedReturnDelegates) && predefinedReturnDelegates != null && predefinedReturnDelegates.Count > 0)
                {
                    foreach (var predefinedReturnDelegate in predefinedReturnDelegates)
                    {
                        if (predefinedReturnDelegate is null || predefinedReturnDelegate.Method is null)
                            continue;

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

                if (HookManager._activeHooks.TryGetValue(type, out var hooks) && hooks != null && hooks.Count > 0)
                {
                    for (int i = 0; i < hooks.Count; i++)
                    {
                        try
                        {
                            var hookResult = hooks[i].Run(eventObject);

                            if (hookResult != null && hookResult is T castValue)
                            {
                                returnValue = castValue;

                                if (cancellable != null)
                                    cancellable.IsAllowed = returnValue;
                            }
                        }
                        catch (Exception ex)
                        {
                            ApiLoader.Error("Hooking API", $"Failed to run hook &3{hooks[i].Method.GetMemberName()}&r due to an exception:\n{ex.ToColoredString()}");
                        }
                    }
                }

                if (HookManager._activeDelegates.TryGetValue(type, out var hookDelegateObjects) && hookDelegateObjects != null && hookDelegateObjects.Count > 0)
                {
                    var args = new object[] { eventObject };

                    foreach (var hookDelegateObject in hookDelegateObjects)
                    {
                        try
                        {
                            if (hookDelegateObject.FieldGetter is null || hookDelegateObject.Invoker is null)
                                continue;

                            var value = hookDelegateObject.FieldGetter();

                            if (value is null)
                            {
                                ApiLoader.Warn("Hooking API", $"Failed to get delegate value of event &3{hookDelegateObject.Event.GetMemberName()}&r");
                                continue;
                            }

                            if (value is Action action)
                            {
                                action();
                                continue;
                            }

                            hookDelegateObject.Invoker(value, args);
                        }
                        catch (Exception ex)
                        {
                            ApiLoader.Error("Hooking API", $"An error occured while running custom delegates (&3{hookDelegateObject.Event.GetMemberName()}&r)!\n{ex.ToColoredString()}");
                        }
                    }
                }

                if (cancellable != null)
                    cancellable.IsAllowed = returnValue;
            }
            catch (Exception ex)
            {
                ApiLoader.Error("Hooking API", $"Failed to run vanilla event &3{type.Name}&r:\n{ex.ToColoredString()}");
            }

            _marker.MarkEnd();
            return returnValue;
        }
    }
}