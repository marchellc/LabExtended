// https://www.codeproject.com/Articles/993798/FieldInfo-PropertyInfo-GetValue-SetValue-Alternati

using System.Linq.Expressions;
using System.Reflection;

namespace LabExtended.Utilities
{
    public class MemberAccess
    {
        /// <summary>
        /// Get the helper method signature one time
        /// </summary>
        private static MethodInfo sm_valueAssignerMethod = typeof(MemberAccess).GetMethod( "ValueAssigner", BindingFlags.Static | BindingFlags.NonPublic );

        /// <summary>
        /// This is the internal method responsible for assigning one value to a member.
        /// This is required to make this class compliant with .NET 3.5 (Unity3d compatible)
        /// </summary>
        /// <typeparam name="T">The Type of the values to assign</typeparam>
        /// <param name="dest">The destination member</param>
        /// <param name="src">The value to assign</param>
        private static void ValueAssigner<T>(out T dest, T src)
        {
            dest = src;
        }

        /// <summary>
        /// The delegate for getting the value of the member
        /// </summary>
        private Func<object, object> m_getter;

        /// <summary>
        /// The delegate for setting the value of the member
        /// </summary>
        private Action<object, object> m_setter;

        /// <summary>
        /// Get the value of the member on a provided object.
        /// </summary>
        /// <param name="p_obj">The object to query for the member value</param>
        /// <returns>The value of the member on the provided object</returns>
        public object Get(object p_obj)
        {
            return m_getter(p_obj);
        }

        /// <summary>
        /// Set the value on a given object to a given value.
        /// </summary>
        /// <param name="p_obj">The object whose member value to set</param>
        /// <param name="p_value">The value to assign to the member</param>
        public void Set(object p_obj, object p_value)
        {
            m_setter(p_obj, p_value);
        }

        /// <summary>
        /// Construct a new member accessor based on a Reflection MemberInfo- either a
        /// PropertyInfo or a FieldInfo
        /// </summary>
        /// <param name="p_member">
        /// A PropertyInfo or a FieldInfo describing the member to access
        /// </param>
        public MemberAccess(MemberInfo member)
        {
            if (member == null)
                throw new ArgumentNullException("Must initialize with a non-null Field or Property");

            MemberExpression exMember = null;

            if (member is FieldInfo)
            {
                var fi = member as FieldInfo;
                var assignmentMethod = sm_valueAssignerMethod.MakeGenericMethod( fi.FieldType );

                Init(fi.DeclaringType, fi.FieldType,
                    _ex => exMember = Expression.Field(_ex, fi), // Create a Field expression, 
                                                                 // and SAVE that field expression for the Call expression
                    (_, _val) => Expression.Call(assignmentMethod,
                    exMember, _val) // We're going to call the static 
                                    // "ValueAssigner" method on this class
                );
            }
            else if (member is PropertyInfo)
            {
                var pi = member as PropertyInfo;
                var assignmentMethod = pi.GetSetMethod();

                Init(pi.DeclaringType, pi.PropertyType,
                    _ex => exMember = Expression.Property(_ex, pi), // Create a Property expression
                    (_obj, _val) => Expression.Call
                    (_obj, assignmentMethod, _val) // We're going to call 
                                                   // the SetMethod on the PropertyInfo object
                );
            }
            else
            {
                throw new ArgumentException("The member must be either a Field or a Property, not " + member.MemberType);
            }
        }

        /// <summary>
        /// Internal initialization routine. The difference between Field and Property
        /// access is extremely similar, but just different enough to require the two
        /// delegates back into the calling routine provide the specialized information.
        /// </summary>
        /// <param name="p_objectType">
        /// The Type of the objects that will have this member accessed
        /// </param>
        /// <param name="p_valueType">The Type of the member</param>
        /// <param name="p_fnGetMember">
        /// A delegate that returns the correct Expression for the member- either
        /// <see cref="Expression.Property"/> or <see cref="Expression.Field"/>
        /// </param>
        /// <param name="p_fnMakeCallExpression">
        /// Get a method that actually calls the Assignment function appropriate for the
        /// MemberType. The order of the parameters for Fields vs Properties is slightly
        /// different, as the Field assignment is static while the Property assignment is an
        /// instance method.
        /// </param>
        private void Init(Type p_objectType, Type p_valueType, Func<Expression, MemberExpression> p_fnGetMember, Func<Expression, Expression, MethodCallExpression> p_fnMakeCallExpression)
        {
            var exObjParam = Expression.Parameter(typeof(object), "theObject" );
            var exValParam = Expression.Parameter(typeof(object), "theProperty" );

            var exObjConverted = Expression.Convert(exObjParam, p_objectType);
            var exValConverted = Expression.Convert(exValParam, p_valueType);

            Expression exMember = p_fnGetMember(exObjConverted);
            Expression getterMember = p_valueType.IsValueType ? Expression.Convert(exMember, typeof(object)) : exMember;
            Expression exAssignment = p_fnMakeCallExpression(exObjConverted, exValConverted);

            m_getter = Expression.Lambda<Func<object, object>>(getterMember, exObjParam).Compile();
            m_setter = Expression.Lambda<Action<object, object>>(exAssignment, exObjParam, exValParam).Compile();
        }
    }
}