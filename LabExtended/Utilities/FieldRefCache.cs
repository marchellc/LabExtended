using HarmonyLib;

using LabExtended.API.Collections.Locked;

using System.Reflection;

namespace LabExtended.Utilities
{
    public static class FieldRefCache<TOut>
    {
        public static volatile LockedDictionary<FieldInfo, AccessTools.FieldRef<object, TOut>> Fields = new LockedDictionary<FieldInfo, AccessTools.FieldRef<object, TOut>>();

        public static AccessTools.FieldRef<object, TOut> Get(FieldInfo field)
        {
            if (Fields.TryGetValue(field, out var fieldRef))
                return fieldRef;

            return Fields[field] = AccessTools.FieldRefAccess<TOut>(field.DeclaringType, field.Name);
        }
    }
}
