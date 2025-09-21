using LabExtended.Extensions;

using System.Linq.Expressions;
using System.Reflection;

namespace LabExtended.Utilities;

/// <summary>
/// A class that constructs delegates using expression trees.
/// </summary>
public static class FastReflection
{
    public static volatile Dictionary<MethodInfo, Func<object, object[], object>> Methods = new();
    public static volatile Dictionary<ConstructorInfo, Func<object[], object>> Constructors = new();

    /// <summary>
    /// Creates a delegate for a type constructor.
    /// </summary>
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

    /// <summary>
    /// Creates a delegate for a delegate invocation call.
    /// </summary>

    public static Func<object, object[], object> ForDelegate(Type delegateType, MethodInfo invokeMethod)
        => CreateMethodWrapper(delegateType, invokeMethod, true);

    /// <summary>
    /// Creates a delegate for a delegate invocation call.
    /// </summary>
    public static Func<object, object[], object> ForDelegate(Delegate del)
    {
        if (del == null)
            throw new ArgumentNullException(nameof(del));

        return CreateMethodWrapper(del.GetType(), del.GetMethodInfo(), true);
    }

    /// <summary>
    /// Creates a delegate for a method invocation call.
    /// </summary>
    public static Func<object, object[], object> ForMethod(Type type, string methodName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (methodName == null)
            throw new ArgumentNullException(nameof(methodName));

        return CreateMethodWrapper(type, type.FindMethod(methodName), false);
    }

    /// <summary>
    /// Creates a delegate for a method invocation call.
    /// </summary>
    public static Func<object, object[], object> ForMethod(MethodInfo method)
        => CreateMethodWrapper(method.DeclaringType, method, false);

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
            : (method.IsStatic ? Expression.Call(method, paramsExps) : Expression.Call(castTargetExp, method, paramsExps));

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
}