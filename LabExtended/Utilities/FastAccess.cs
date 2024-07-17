using LabExtended.API.Collections.Locked;
using LabExtended.Core;
using LabExtended.Extensions;

using System.Linq.Expressions;
using System.Reflection;

namespace LabExtended.Utilities
{
    public static class FastAccess
    {
        #region Method Cache
        private static readonly LockedDictionary<MethodInfo, Func<object, object[], object>> _nonVoidInstanceMethods = new LockedDictionary<MethodInfo, Func<object, object[], object>>();
        private static readonly LockedDictionary<MethodInfo, Func<object[], object>> _nonVoidStaticMethods = new LockedDictionary<MethodInfo, Func<object[], object>>();

        private static readonly LockedDictionary<MethodInfo, Action<object, object[]>> _voidInstanceMethods = new LockedDictionary<MethodInfo, Action<object, object[]>>();
        private static readonly LockedDictionary<MethodInfo, Action<object[]>> _voidStaticMethods = new LockedDictionary<MethodInfo, Action<object[]>>();

        private static readonly LockedDictionary<MethodInfo, Func<object, object[], object>> _wrappers = new LockedDictionary<MethodInfo, Func<object, object[], object>>();
        #endregion

        private static readonly LockedDictionary<MemberInfo, MemberAccess> _accessCache = new LockedDictionary<MemberInfo, MemberAccess>();
        private static readonly LockedDictionary<Type, Func<object[], object>> _constructors = new LockedDictionary<Type, Func<object[], object>>();

        #region Method Invocation
        public static object InvokeMethod(this MethodInfo method, object instance, params object[] args)
            => GetWrappedCaller(method)(instance, args);

        public static T InvokeMethod<T>(this MethodInfo method, object instance, params object[] args)
        {
            var result = GetWrappedCaller(method)(instance, args);

            if (result != null)
                return (T)result;

            return default;
        }

        public static object InvokeMethodSafe(this MethodInfo method, object instance, params object[] args)
        {
            try
            {
                return GetWrappedCaller(method)(instance, args);
            }
            catch (Exception ex)
            {
                ExLoader.Error("LabExtended.FastAccess", $"A method call caught an exception:\n{ex}");
                return null;
            }
        }

        public static T InvokeMethodSafe<T>(this MethodInfo method, object instance, params object[] args)
        {
            try
            {
                var result = GetWrappedCaller(method)(instance, args);

                if (result != null)
                    return (T)result;

                return default;
            }
            catch (Exception ex)
            {
                ExLoader.Error("LabExtended.FastAccess", $"A method call caught an exception:\n{ex}");
                return default;
            }
        }
        #endregion

        #region Member Access
        public static object Get(this MemberInfo member, object instance = null)
            => GetMemberAccess(member).Get(instance);

        public static T Get<T>(this MemberInfo member, object instance = null)
        {
            var result = GetMemberAccess(member).Get(instance);

            if (result != null)
                return (T)result;

            return default;
        }

        public static void Set(this MemberInfo member, object instance, object value)
            => GetMemberAccess(member).Set(instance, value);

        public static MemberAccess GetMemberAccess(this MemberInfo member)
        {
            if (_accessCache.TryGetValue(member, out var memberAccess))
                return memberAccess;

            return _accessCache[member] = new MemberAccess(member);
        }
        #endregion

        #region Method Creation
        public static Action<object[]> GetStaticVoidMethodLamba(this MethodInfo method)
        {
            if (_voidStaticMethods.TryGetValue(method, out var cachedLambda))
                return cachedLambda;

            var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            var call = Expression.Call(method, CreateParameterExpressions(method, argumentsParameter));
            var lambda = Expression.Lambda<Action<object[]>>(call, argumentsParameter);

            return _voidStaticMethods[method] = lambda.Compile();
        }

        public static Func<object[], object> GetStaticNonVoidMethodLambda(this MethodInfo method)
        {
            if (!_nonVoidStaticMethods.TryGetValue(method, out var cachedLambda))
                return cachedLambda;

            var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            var call = Expression.Call(method, CreateParameterExpressions(method, argumentsParameter));
            var lambda = Expression.Lambda<Func<object[], object>>(Expression.Convert(call, typeof(object)), argumentsParameter);

            return _nonVoidStaticMethods[method] = lambda.Compile();
        }

        public static Action<object, object[]> GetInstanceVoidMethodLambda(this MethodInfo method)
        {
            if (_voidInstanceMethods.TryGetValue(method, out var cachedLambda))
                return cachedLambda;

            var instanceParameter = Expression.Parameter(typeof(Object), "target");
            var argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            var call = Expression.Call(Expression.Convert(instanceParameter, method.DeclaringType), method, CreateParameterExpressions(method, argumentsParameter));
            var lambda = Expression.Lambda<Action<object, object[]>>(call, instanceParameter, argumentsParameter);

            return _voidInstanceMethods[method] = lambda.Compile();
        }

        public static Func<object, object[], object> GetInstanceNonVoidMethodLambda(this MethodInfo method)
        {
            if (_nonVoidInstanceMethods.TryGetValue(method, out var cachedLambda))
                return cachedLambda;

            var instanceParameter = Expression.Parameter(typeof(object), "target");
            var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            var call = Expression.Call(Expression.Convert(instanceParameter, method.DeclaringType), method, CreateParameterExpressions(method, argumentsParameter));
            var lambda = Expression.Lambda<Func<object, object[], object>>(Expression.Convert(call, typeof(object)), instanceParameter, argumentsParameter);

            return lambda.Compile();
        }

        public static Func<object, object[], object> GetWrappedCaller(this MethodInfo method)
        {
            if (_wrappers.TryGetValue(method, out var wrapper))
                return wrapper;

            if (method.IsStatic && method.ReturnType == typeof(void))
            {
                var caller = method.GetStaticVoidMethodLamba();
                return _wrappers[method] = (_, parameters) => { caller(parameters); return null; };
            }
            else if (method.IsStatic && method.ReturnType != typeof(void))
            {
                var caller = method.GetStaticNonVoidMethodLambda();
                return _wrappers[method] = (_, parameters) => caller(parameters);
            }
            else if (!method.IsStatic && method.ReturnType == typeof(void))
            {
                var caller = method.GetInstanceVoidMethodLambda();
                return _wrappers[method] = (target, parameters) => { caller(target, parameters); return null; };
            }
            else if (!method.IsStatic && method.ReturnType != typeof(void))
            {
                var caller = method.GetInstanceNonVoidMethodLambda();
                return _wrappers[method] = (target, parameters) => caller(target, parameters);
            }
            else
            {
                throw new Exception($"Unknown error");
            }
        }
        #endregion

        public static Expression[] CreateParameterExpressions(this MethodBase method, Expression argumentsParameter)
            => method.GetAllParameters().Select((parameter, index) => Expression.Convert(Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).Cast<Expression>().ToArray();
    }
}
