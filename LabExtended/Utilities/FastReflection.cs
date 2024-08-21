﻿using LabExtended.API.Collections.Locked;
using LabExtended.Extensions;

using System.Linq.Expressions;
using System.Reflection;

namespace LabExtended.Utilities
{
    public static class FastReflection
    {
        public static volatile LockedDictionary<MethodInfo, Func<object, object[], object>> Methods = new LockedDictionary<MethodInfo, Func<object, object[], object>>();
        public static volatile LockedDictionary<ConstructorInfo, Func<object[], object>> Constructors = new LockedDictionary<ConstructorInfo, Func<object[], object>>();

        public static Func<object[], object> ForConstructor(ConstructorInfo constructor)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            if (Constructors.TryGetValue(constructor, out var lambda))
                return lambda;

            CreateParamsExpressions(constructor, out ParameterExpression argsExp, out Expression[] paramsExps);

            var newExp = Expression.New(constructor, paramsExps);
            var resultExp = Expression.Convert(newExp, typeof(object));

            var lambdaExp = Expression.Lambda(resultExp, argsExp);

            return Constructors[constructor] = (Func<object[], object>)lambdaExp.Compile();
        }

        public static Func<object, object[], object> ForDelegate(Type delegateType, MethodInfo invokeMethod)
            => CreateMethodWrapper(delegateType, invokeMethod, true);

        public static Func<object, object[], object> ForDelegate(Delegate del)
        {
            if (del == null)
                throw new ArgumentNullException(nameof(del));

            return CreateMethodWrapper(del.GetType(), del.GetMethodInfo(), true);
        }

        public static Func<object, object[], object> ForMethod(Type type, string methodName)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));

            return CreateMethodWrapper(type, type.FindMethod(methodName), false);
        }

        public static Func<object, object[], object> ForMethod(MethodInfo method)
            => CreateMethodWrapper(method.DeclaringType, method, false);

        public static Func<object, object[], object> ForProperty(Type type, string propertyName)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            return CreatePropertyWrapper(type, propertyName);
        }

        private static Func<object, object[], object> CreateMethodWrapper(Type type, MethodInfo method, bool isDelegate)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            if (Methods.TryGetValue(method, out var lambda))
                return lambda;

            CreateParamsExpressions(method, out ParameterExpression argsExp, out Expression[] paramsExps);

            var targetExp = Expression.Parameter(typeof(object), "target");
            var castTargetExp = Expression.Convert(targetExp, type);

            var invokeExp = isDelegate
                ? (Expression)Expression.Invoke(castTargetExp, paramsExps)
                : Expression.Call(castTargetExp, method, paramsExps);

            LambdaExpression lambdaExp;

            if (method.ReturnType != typeof(void))
            {
                var resultExp = Expression.Convert(invokeExp, typeof(object));

                lambdaExp = Expression.Lambda(resultExp, targetExp, argsExp);
            }
            else
            {
                var constExp = Expression.Constant(null, typeof(object));
                var blockExp = Expression.Block(invokeExp, constExp);

                lambdaExp = Expression.Lambda(blockExp, targetExp, argsExp);
            }

            return Methods[method] = (Func<object, object[], object>)lambdaExp.Compile();
        }

        private static void CreateParamsExpressions(MethodBase method, out ParameterExpression argsExp, out Expression[] paramsExps)
        {
            var parameters = method.GetAllParameters().Select(x => x.ParameterType).ToArray();

            argsExp = Expression.Parameter(typeof(object[]), "args");
            paramsExps = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var constExp = Expression.Constant(i, typeof(int));
                var argExp = Expression.ArrayIndex(argsExp, constExp);

                paramsExps[i] = Expression.Convert(argExp, parameters[i]);
            }
        }

        private static Func<object, object[], object> CreatePropertyWrapper(Type type, string propertyName)
        {
            var property = type.GetRuntimeProperty(propertyName);

            var targetExp = Expression.Parameter(typeof(object), "target");
            var argsExp = Expression.Parameter(typeof(object[]), "args");

            var castArgExp = Expression.Convert(targetExp, type);
            var propExp = Expression.Property(castArgExp, property);

            var castPropExp = Expression.Convert(propExp, typeof(object));
            var lambdaExp = Expression.Lambda(castPropExp, targetExp, argsExp);

            var lambda = lambdaExp.Compile();

            return (Func<object, object[], object>)lambda;
        }
    }
}