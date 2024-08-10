using LabExtended.API.Collections.Locked;
using LabExtended.Core;

using System.Reflection;

namespace LabExtended.Extensions
{
    public static class TypeExtensions
    {
        public const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        private static readonly LockedDictionary<Type, ConstructorInfo[]> _constructors = new LockedDictionary<Type, ConstructorInfo[]>();
        private static readonly LockedDictionary<Type, PropertyInfo[]> _properties = new LockedDictionary<Type, PropertyInfo[]>();
        private static readonly LockedDictionary<Type, MethodInfo[]> _methods = new LockedDictionary<Type, MethodInfo[]>();
        private static readonly LockedDictionary<Type, FieldInfo[]> _fields = new LockedDictionary<Type, FieldInfo[]>();
        private static readonly LockedDictionary<Type, EventInfo[]> _events = new LockedDictionary<Type, EventInfo[]>();

        public static ConstructorInfo[] GetAllConstructors(this Type type)
        {
            if (_constructors.TryGetValue(type, out var constructors))
                return constructors;

            return _constructors[type] = type.GetConstructors(Flags);
        }

        public static PropertyInfo[] GetAllProperties(this Type type)
        {
            if (_properties.TryGetValue(type, out var properties))
                return properties;

            return _properties[type] = type.GetProperties(Flags);
        }

        public static FieldInfo[] GetAllFields(this Type type)
        {
            if (_fields.TryGetValue(type, out var fields))
                return fields;

            return _fields[type] = type.GetFields(Flags);
        }

        public static MethodInfo[] GetAllMethods(this Type type)
        {
            if (_methods.TryGetValue(type, out var methods))
                return methods;

            return _methods[type] = type.GetMethods(Flags);
        }

        public static EventInfo[] GetAllEvents(this Type type)
        {
            if (_events.TryGetValue(type, out var events))
                return events;

            return _events[type] = type.GetEvents(Flags);
        }

        public static MethodInfo FindMethod(this Type type, Func<MethodInfo, bool> predicate)
            => GetAllMethods(type).FirstOrDefault(m => predicate(m));

        public static MethodInfo FindMethod(this Type type, string methodName)
            => FindMethod(type, method => method.Name == methodName);

        public static FieldInfo FindField(this Type type, Func<FieldInfo, bool> predicate)
            => GetAllFields(type).FirstOrDefault(f => predicate(f));

        public static FieldInfo FindField(this Type type, string fieldName)
            => FindField(type, field => field.Name == fieldName);

        public static IEnumerable<FieldInfo> FindFields(this Type type, Predicate<FieldInfo> predicate)
            => GetAllFields(type).Where(x => predicate(x));

        public static IEnumerable<FieldInfo> FindFieldsOfType(this Type type, Type fieldType)
            => FindFields(type, x => x.FieldType == fieldType);

        public static PropertyInfo FindProperty(this Type type, Func<PropertyInfo, bool> predicate)
            => GetAllProperties(type).FirstOrDefault(p => predicate(p));

        public static PropertyInfo FindProperty(this Type type, string propertyName)
            => FindProperty(type, p => p.Name == propertyName);

        public static IEnumerable<PropertyInfo> FindProperties(this Type type, Predicate<PropertyInfo> predicate)
            => GetAllProperties(type).Where(x => predicate(x));

        public static IEnumerable<PropertyInfo> FindPropertiesOfType(this Type type, Type propertyType)
            => FindProperties(type, x => x.PropertyType == propertyType);

        public static EventInfo FindEvent(this Type type, Func<EventInfo, bool> predicate)
            => GetAllEvents(type).FirstOrDefault(ev => predicate(ev));

        public static EventInfo FindEvent(this Type type, string eventName)
            => FindEvent(type, ev => ev.Name == eventName);

        public static EventInfo FindEvent(this Type type, Type eventType)
            => FindEvent(type, ev => ev.EventHandlerType == eventType);

        public static EventInfo FindEvent<THandler>(this Type type) where THandler : Delegate
            => FindEvent(type, typeof(THandler));

        public static void RaiseEvent(this Type type, string eventName, object target, params object[] args)
        {
            var ev = FindEvent(type, eventName);

            if (ev is null)
                throw new Exception($"Event '{eventName}' has not been found in type '{type.FullName}'");

            var field = FindField(type, ev.Name);

            if (field is null)
                throw new Exception($"Event '{eventName}' does not have a backing field");

            var delValue = field.GetValue(target);

            if (delValue is null || delValue is not Delegate del)
                return;

            del.DynamicInvoke(args);
        }

        public static object Construct(this Type type)
            => Activator.CreateInstance(type);

        public static T Construct<T>(this Type type)
            => (T)Activator.CreateInstance(type);

        public static bool InheritsType(this Type type, Type checkType)
        {
            if (checkType.IsInterface)
                return checkType.IsAssignableFrom(type);
            else
                return type.IsSubclassOf(checkType);
        }

        public static bool InheritsType<T>(this Type type)
            => InheritsType(type, typeof(T));

        public static bool IsTypeInstance(this Type type, object instance)
        {
            if (instance is null)
                return false;

            if (instance.GetType() != type)
                return false;

            return true;
        }

        public static void InvokeStaticMethod(this Type type, Func<MethodInfo, bool> predicate, params object[] args)
        {
            var method = type.FindMethod(predicate);

            if (method is null || !method.IsStatic)
                return;

            try
            {
                method.Invoke(null, args);
            }
            catch (Exception ex)
            {
                ApiLoader.Error("Extended API", $"Failed to invoke static method &3{method.GetMemberName()}&r due to an exception:\n{ex.ToColoredString()}");
            }
        }
    }
}