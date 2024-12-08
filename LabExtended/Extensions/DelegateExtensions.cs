using LabExtended.Core;

namespace LabExtended.Extensions
{
    public static class DelegateExtensions
    {
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
}
