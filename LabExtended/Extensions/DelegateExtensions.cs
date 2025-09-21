using LabExtended.Core;

namespace LabExtended.Extensions;
/// <summary>
/// Extensions for delegates.
/// </summary>
public static class DelegateExtensions
{
    /// <summary>
    /// Invokes each listener in a delegate and combines the resulting values.
    /// </summary>
    public static T? InvokeCollect<T>(this Func<T> func, Func<T?, T?, T?> combiner, T? defaultResult = default)
    {
        if (func is null)
            return defaultResult;

        if (combiner is null)
            throw new ArgumentNullException(nameof(combiner));

        var current = defaultResult;

        foreach (var listener in func.GetInvocationList())
        {
            var result = (listener as Func<T>)();

            current = combiner(current, result);
        }

        return current;
    }

    /// <summary>
    /// Invokes each listener in a delegate and combines the resulting values.
    /// </summary>
    public static T? InvokeCollect<T, T1>(this Func<T1, T> func, T1 t1, Func<T?, T?, T?> combiner, T? defaultResult = default)
    {
        if (func is null)
            return defaultResult;

        if (combiner is null)
            throw new ArgumentNullException(nameof(combiner));

        var current = defaultResult;

        foreach (var listener in func.GetInvocationList())
        {
            var result = (listener as Func<T1, T>)(t1);

            current = combiner(current, result);
        }

        return current;
    }

    /// <summary>
    /// Invokes each listener in a delegate and combines the resulting values.
    /// </summary>
    public static T? InvokeCollect<T, T1, T2>(this Func<T1, T2, T> func, T1 t1, T2 t2, Func<T?, T?, T?> combiner, T? defaultResult = default)
    {
        if (func is null)
            return defaultResult;

        if (combiner is null)
            throw new ArgumentNullException(nameof(combiner));

        var current = defaultResult;

        foreach (var listener in func.GetInvocationList())
        {
            var result = (listener as Func<T1, T2, T>)(t1, t2);

            current = combiner(current, result);
        }

        return current;
    }

    /// <summary>
    /// Invokes each listener in a delegate and combines the resulting values.
    /// </summary>
    public static T? InvokeCollect<T, T1, T2, T3>(this Func<T1, T2, T3, T> func, T1 t1, T2 t2, T3 t3, Func<T?, T?, T?> combiner, T? defaultResult = default)
    {
        if (func is null)
            return defaultResult;

        if (combiner is null)
            throw new ArgumentNullException(nameof(combiner));

        var current = defaultResult;

        foreach (var listener in func.GetInvocationList())
        {
            var result = (listener as Func<T1, T2, T3, T>)(t1, t2, t3);

            current = combiner(current, result);
        }

        return current;
    }

    /// <summary>
    /// Safely invokes a delegate.
    /// </summary>
    public static void InvokeSafe(this Action action, bool throwException = false)
    {
        if (action is null)
            return;

        try
        {
            action();
        }
        catch (Exception ex)
        {
            ApiLog.Error("LabExtended", $"Caught an error while executing delegate &3{action.Method.GetMemberName()}&r:\n{ex.ToColoredString()}");

            if (throwException)
                throw ex;
        }
    }

    /// <summary>
    /// Safely invokes a delegate.
    /// </summary>
    public static void InvokeSafe<T>(this Action<T> action, T value, bool throwException = false)
    {
        if (action is null)
            return;

        try
        {
            action(value);
        }
        catch (Exception ex)
        {
            ApiLog.Error("LabExtended", $"Caught an error while executing delegate &3{action.Method.GetMemberName()}&r:\n{ex.ToColoredString()}");

            if (throwException)
                throw ex;
        }
    }

    /// <summary>
    /// Safely invokes a delegate.
    /// </summary>
    public static void InvokeSafe<T, T2>(this Action<T, T2> action, T value1, T2 value2, bool throwException = false)
    {
        if (action is null)
            return;

        try
        {
            action(value1, value2);
        }
        catch (Exception ex)
        {
            ApiLog.Error("LabExtended", $"Caught an error while executing delegate &3{action.Method.GetMemberName()}&r:\n{ex.ToColoredString()}");

            if (throwException)
                throw ex;
        }
    }

    /// <summary>
    /// Safely invokes a delegate.
    /// </summary>
    public static void InvokeSafe<T, T2, T3>(this Action<T, T2, T3> action, T value1, T2 value2, T3 value3, bool throwException = false)
    {
        if (action is null)
            return;

        try
        {
            action(value1, value2, value3);
        }
        catch (Exception ex)
        {
            ApiLog.Error("LabExtended", $"Caught an error while executing delegate &3{action.Method.GetMemberName()}&r:\n{ex.ToColoredString()}");

            if (throwException)
                throw ex;
        }
    }

    /// <summary>
    /// Safely invokes a delegate.
    /// </summary>
    public static T InvokeSafe<T>(this Func<T> func, bool throwException = false)
    {
        if (func is null)
            return default;

        try
        {
            return func();
        }
        catch (Exception ex)
        {
            ApiLog.Error("LabExtended", $"Caught an error while executing delegate &3{func.Method.GetMemberName()}&r:\n{ex.ToColoredString()}");

            if (throwException)
                throw ex;
        }

        return default;
    }

    /// <summary>
    /// Safely invokes a delegate.
    /// </summary>
    public static TOut InvokeSafe<T1, TOut>(this Func<T1, TOut> func, T1 value1, bool throwException = false)
    {
        if (func is null)
            return default;

        try
        {
            return func(value1);
        }
        catch (Exception ex)
        {
            ApiLog.Error("LabExtended", $"Caught an error while executing delegate &3{func.Method.GetMemberName()}&r:\n{ex.ToColoredString()}");

            if (throwException)
                throw ex;
        }

        return default;
    }
}