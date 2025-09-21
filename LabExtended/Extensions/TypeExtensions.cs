using HarmonyLib;

using LabExtended.Core;
using LabExtended.Utilities;

using System.Reflection;

namespace LabExtended.Extensions;

/// <summary>
/// Extensions targeting reflection types.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Binding flags including public / private and static / instance.
    /// </summary>
    public const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    private static readonly Dictionary<Type, ConstructorInfo[]> _constructors = new();
    private static readonly Dictionary<Type, PropertyInfo[]> _properties = new();
    private static readonly Dictionary<Type, MethodInfo[]> _methods = new();
    private static readonly Dictionary<Type, FieldInfo[]> _fields = new();
    private static readonly Dictionary<Type, EventInfo[]> _events = new();

    /// <summary>
    /// Gets all constructors in a type (using a cache).
    /// </summary>
    public static ConstructorInfo[] GetAllConstructors(this Type type)
    {
        if (_constructors.TryGetValue(type, out var constructors))
            return constructors;

        return _constructors[type] = type.GetConstructors(Flags);
    }

    /// <summary>
    /// Gets all properties in a type (using a cache).
    /// </summary>
    public static PropertyInfo[] GetAllProperties(this Type type)
    {
        if (_properties.TryGetValue(type, out var properties))
            return properties;

        return _properties[type] = type.GetProperties(Flags);
    }

    /// <summary>
    /// Gets all fields in a type (using a cache).
    /// </summary>
    public static FieldInfo[] GetAllFields(this Type type)
    {
        if (_fields.TryGetValue(type, out var fields))
            return fields;

        return _fields[type] = type.GetFields(Flags);
    }

    /// <summary>
    /// Gets all methods in a type (using a cache).
    /// </summary>
    public static MethodInfo[] GetAllMethods(this Type type)
    {
        if (_methods.TryGetValue(type, out var methods))
            return methods;

        return _methods[type] = type.GetMethods(Flags);
    }

    /// <summary>
    /// Gets all events in a type (using a cache).
    /// </summary>
    public static EventInfo[] GetAllEvents(this Type type)
    {
        if (_events.TryGetValue(type, out var events))
            return events;

        return _events[type] = type.GetEvents(Flags);
    }

    /// <summary>
    /// Finds a method matching a predicate.
    /// </summary>
    public static MethodInfo FindMethod(this Type type, Func<MethodInfo, bool> predicate)
        => GetAllMethods(type).FirstOrDefault(m => predicate(m));

    /// <summary>
    /// Finds a method with a specific name.
    /// </summary>
    public static MethodInfo FindMethod(this Type type, string methodName)
        => FindMethod(type, method => method.Name == methodName);

    /// <summary>
    /// Finds a field matching a predicate.
    /// </summary>
    public static FieldInfo FindField(this Type type, Func<FieldInfo, bool> predicate)
        => GetAllFields(type).FirstOrDefault(f => predicate(f));

    /// <summary>
    /// Finds a field with a specific name.
    /// </summary>
    public static FieldInfo FindField(this Type type, string fieldName)
        => FindField(type, field => field.Name == fieldName);

    /// <summary>
    /// Finds all fields matching a predicate.
    /// </summary>
    public static IEnumerable<FieldInfo> FindFields(this Type type, Predicate<FieldInfo> predicate)
        => GetAllFields(type).Where(x => predicate(x));

    /// <summary>
    /// Finds all fields of a specific type.
    /// </summary>
    public static IEnumerable<FieldInfo> FindFieldsOfType(this Type type, Type fieldType)
        => FindFields(type, x => x.FieldType == fieldType);

    /// <summary>
    /// Finds a property matching a predicate.
    /// </summary>
    public static PropertyInfo FindProperty(this Type type, Func<PropertyInfo, bool> predicate)
        => GetAllProperties(type).FirstOrDefault(p => predicate(p));

    /// <summary>
    /// Finds a property with a specific name.
    /// </summary>
    public static PropertyInfo FindProperty(this Type type, string propertyName)
        => FindProperty(type, p => p.Name == propertyName);

    /// <summary>
    /// Finds all properties matching a predicate.
    /// </summary>
    public static IEnumerable<PropertyInfo> FindProperties(this Type type, Predicate<PropertyInfo> predicate)
        => GetAllProperties(type).Where(x => predicate(x));

    /// <summary>
    /// Finds all properties of a specific type.
    /// </summary>
    public static IEnumerable<PropertyInfo> FindPropertiesOfType(this Type type, Type propertyType)
        => FindProperties(type, x => x.PropertyType == propertyType);

    /// <summary>
    /// Finds an event.
    /// </summary>
    public static EventInfo FindEvent(this Type type, Func<EventInfo, bool> predicate)
        => GetAllEvents(type).FirstOrDefault(ev => predicate(ev));

    /// <summary>
    /// Finds an event.
    /// </summary>
    public static EventInfo FindEvent(this Type type, string eventName)
        => FindEvent(type, ev => ev.Name == eventName);

    /// <summary>
    /// Finds an event.
    /// </summary>
    public static EventInfo FindEvent(this Type type, Type eventType)
        => FindEvent(type, ev => ev.EventHandlerType == eventType);

    /// <summary>
    /// Finds an event.
    /// </summary>
    public static EventInfo FindEvent<THandler>(this Type type) where THandler : Delegate
        => FindEvent(type, typeof(THandler));

    /// <summary>
    /// Constructs a type instance.
    /// </summary>
    public static object Construct(this Type type)
    {
        var constructor = AccessTools.Constructor(type);

        if (constructor is null)
            throw new Exception("No constructors were found");

        var invoker = FastReflection.ForConstructor(constructor);
        return invoker(null);
    }

    /// <summary>
    /// Constructs a type instance.
    /// </summary>
    public static T Construct<T>(this Type type)
        => (T)Construct(type);

    /// <summary>
    /// Whether or not an object inherits a specific type.
    /// </summary>
    public static bool InheritsType(this Type type, Type checkType)
    {
        if (checkType.IsInterface)
            return checkType.IsAssignableFrom(type);
        else
            return type.IsSubclassOf(checkType);
    }

    /// <summary>
    /// Whether or not an object inherits a specific type.
    /// </summary>
    public static bool InheritsType<T>(this Type type)
        => InheritsType(type, typeof(T));

    /// <summary>
    /// Whether or not an object is an instance of a specific type.
    /// </summary>
    public static bool IsTypeInstance(this Type type, object instance)
    {
        if (instance is null)
            return false;

        var instanceType = instance.GetType();

        if (instanceType != type)
            return false;

        return instanceType == type || instanceType.InheritsType(type);
    }

    /// <summary>
    /// Executes a delegate for each currently loaded type.
    /// </summary>
    public static void ForEachLoadedType(Action<Type> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        try
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        try
                        {
                            action(type);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Safely invokes a static method.
    /// </summary>
    public static void InvokeStaticMethod(this Type type, Func<MethodInfo, bool> predicate, params object[] args)
    {
        var method = type.FindMethod(predicate);

        if (method is null || !method.IsStatic)
            return;

        var invoker = FastReflection.ForMethod(method);

        if (invoker is null)
            return;

        try
        {
            invoker(null, args);
        }
        catch (Exception ex)
        {
            ApiLog.Error("Extended API", $"Failed to invoke static method &3{method.GetMemberName()}&r due to an exception:\n{ex.ToColoredString()}");
        }
    }
}