using LabExtended.Core.Hooking.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Core.Hooking
{
    public static class HookRunner
    {
        public static void RunEvent(object eventObject)
            => RunEvent<object>(eventObject, null);

        public static T RunEvent<T>(object eventObject, T returnValue = default)
        {
            if (eventObject is null)
                throw new ArgumentNullException(nameof(eventObject));

            var type = eventObject.GetType();
            var cancellable = eventObject as ICancellableEvent<T>;

            if (cancellable != null)
                cancellable.IsAllowed = returnValue;

            try
            {
                if (HookManager._activeHooks.TryGetValue(type, out var hooks) && hooks != null && hooks.Count > 0)
                {
                    hooks.ForEach(hook =>
                    {
                        try
                        {
                            var hookResult = hook.Run(eventObject);

                            if (hookResult != null && hookResult is T castValue)
                            {
                                returnValue = castValue;

                                if (cancellable != null)
                                    cancellable.IsAllowed = returnValue;
                            }
                        }
                        catch (Exception ex)
                        {
                            ApiLog.Error("Hooking API", $"Failed to run hook &3{hook.Method.GetMemberName()}&r due to an exception:\n{ex.ToColoredString()}");
                        }
                    });
                }

                if (HookManager._activeDelegates.TryGetValue(type, out var hookDelegateObjects) && hookDelegateObjects != null && hookDelegateObjects.Count > 0)
                {
                    object[] args = [eventObject];

                    hookDelegateObjects.ForEach(hookDelegateObject =>
                    {
                        try
                        {
                            if (hookDelegateObject.FieldGetter is null || hookDelegateObject.Invoker is null)
                                return;

                            var value = hookDelegateObject.FieldGetter();

                            if (value is null)
                            {
                                ApiLog.Warn("Hooking API", $"Failed to get delegate value of event &3{hookDelegateObject.Event.GetMemberName()}&r");
                                return;
                            }

                            if (value is Action action)
                            {
                                action();
                                return;
                            }

                            hookDelegateObject.Invoker(value, args);
                        }
                        catch (Exception ex)
                        {
                            ApiLog.Error("Hooking API", $"An error occured while running custom delegates (&3{hookDelegateObject.Event.GetMemberName()}&r)!\n{ex.ToColoredString()}");
                        }
                    });
                }

                if (cancellable != null)
                    cancellable.IsAllowed = returnValue;
            }
            catch (Exception ex)
            {
                ApiLog.Error("Hooking API", $"Failed to run vanilla event &3{type.Name}&r:\n{ex.ToColoredString()}");
            }

            return returnValue;
        }
    }
}