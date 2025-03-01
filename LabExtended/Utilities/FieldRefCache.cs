using HarmonyLib;

using System.Reflection;

namespace LabExtended.Utilities
{
    public static class FieldRefCache<TOut>
    {
        public static volatile Dictionary<FieldInfo, AccessTools.FieldRef<object, TOut>> Fields = new();

        public static AccessTools.FieldRef<object, TOut> Get(FieldInfo field)
        {
            if (Fields.TryGetValue(field, out var fieldRef))
                return fieldRef;

            return Fields[field] = AccessTools.FieldRefAccess<TOut>(field.DeclaringType, field.Name);
        }
    }
}
