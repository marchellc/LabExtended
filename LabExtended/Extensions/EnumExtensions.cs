using LabExtended.API.Collections.Locked;

using NorthwoodLib.Pools;

namespace LabExtended.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="Enum"/> class.
    /// </summary>
    public static class EnumExtensions
    {
        public enum EnumOperation
        {
            Combine,
            Remove
        }

        private static readonly LockedHashSet<Type> _flaggableEnums = new LockedHashSet<Type>();
        private static readonly LockedHashSet<Type> _nonFlaggableEnums = new LockedHashSet<Type>();
        private static readonly LockedDictionary<Type, Enum[]> _enumValuesCache = new LockedDictionary<Type, Enum[]>();

        public static Enum[] GetEnumValues(this Type type)
            => _enumValuesCache.TryGetValue(type, out var values) ? values : _enumValuesCache[type] = Enum.GetValues(type).CastArray<Enum>();

        public static TEnum[] GetValues<TEnum>() where TEnum : struct, Enum
        {
            if (_enumValuesCache.TryGetValue(typeof(TEnum), out var values))
                return values.CastArray<TEnum>();
            else
                return (_enumValuesCache[typeof(TEnum)] = (Enum.GetValues(typeof(TEnum)).ToArray<Enum>())).CastArray<TEnum>();
        }

        public static TEnum[] GetFlags<TEnum>(this TEnum en) where TEnum : struct, Enum
        {
            var values = GetValues<TEnum>();
            var cache = ListPool<TEnum>.Shared.Rent();

            foreach (var value in values)
            {
                if (!Any(en, value))
                    continue;

                cache.Add(value);
            }

            var array = cache.ToArray();

            ListPool<TEnum>.Shared.Return(cache);
            return array;
        }

        public static bool Any<TEnum>(this TEnum target, TEnum value) where TEnum : struct, Enum
        {
            var typeCode = target.GetTypeCode();

            switch (typeCode)
            {
                case TypeCode.Byte: return HasByte(target, value);

                case TypeCode.Int16: return HasInt16(target, value);
                case TypeCode.Int32: return HasInt32(target, value);
                case TypeCode.Int64: return HasInt64(target, value);

                case TypeCode.UInt16: return HasUInt16(target, value);
                case TypeCode.UInt32: return HasUInt32(target, value);
                case TypeCode.UInt64: return HasUInt64(target, value);

                default:
                    throw new ArgumentException($"Enum '{typeof(TEnum).FullName}' has unsupported type code: '{typeCode}'");
            }
        }

        public static TEnum Remove<TEnum>(this TEnum target, TEnum flag) where TEnum : struct, Enum
        {
            var typeCode = target.GetTypeCode();

            switch (typeCode)
            {
                case TypeCode.Byte: return (TEnum)OperateBytes(target, flag, EnumOperation.Remove);

                case TypeCode.Int16: return (TEnum)OperateInt16(target, flag, EnumOperation.Remove);
                case TypeCode.Int32: return (TEnum)OperateInt32(target, flag, EnumOperation.Remove);
                case TypeCode.Int64: return (TEnum)OperateInt64(target, flag, EnumOperation.Remove);

                case TypeCode.UInt16: return (TEnum)OperateUInt16(target, flag, EnumOperation.Remove);
                case TypeCode.UInt32: return (TEnum)OperateUInt32(target, flag, EnumOperation.Remove);
                case TypeCode.UInt64: return (TEnum)OperateUInt64(target, flag, EnumOperation.Remove);

                default:
                    throw new ArgumentException($"Enum '{typeof(TEnum).FullName}' has unsupported type code: '{typeCode}'");
            }
        }

        public static TEnum Combine<TEnum>(this TEnum target, TEnum flag) where TEnum : struct, Enum
        {
            var typeCode = target.GetTypeCode();

            switch (typeCode)
            {
                case TypeCode.Byte: return (TEnum)OperateBytes(target, flag);

                case TypeCode.Int16: return (TEnum)OperateInt16(target, flag);
                case TypeCode.Int32: return (TEnum)OperateInt32(target, flag);
                case TypeCode.Int64: return (TEnum)OperateInt64(target, flag);

                case TypeCode.UInt16: return (TEnum)OperateUInt16(target, flag);
                case TypeCode.UInt32: return (TEnum)OperateUInt32(target, flag);
                case TypeCode.UInt64: return (TEnum)OperateUInt64(target, flag);

                default:
                    throw new ArgumentException($"Enum '{typeof(TEnum).FullName}' has unsupported type code: '{typeCode}'");
            }
        }

        /// <summary>
        /// An extremely janky way of checking if an enum supports bitwise operations. This is done by checking if all of it's values are dividable by 2.
        /// </summary>
        /// <param name="enumType">Type of the enum to check.</param>
        /// <returns><see langword="true"/> if it supports bitwise operations, otherwise <see langword="false"/>.</returns>
        public static bool IsBitwiseEnum(this Type enumType)
        {
            if (_flaggableEnums.Contains(enumType))
                return true;

            if (_nonFlaggableEnums.Contains(enumType))
                return false;

            var underlyingType = Enum.GetUnderlyingType(enumType);
            var values = new List<object>(Enum.GetValues(enumType).ToArray<object>());
            var supports = false;

            if (underlyingType == typeof(byte))
                supports = CheckEnum(values.Select(val => (byte)Convert.ChangeType(val, underlyingType)).ToList());
            else if (underlyingType == typeof(sbyte))
                supports = CheckEnum(values.Select(val => (sbyte)Convert.ChangeType(val, underlyingType)).ToList());
            else if (underlyingType == typeof(short))
                supports = CheckEnum(values.Select(val => (short)Convert.ChangeType(val, underlyingType)).ToList());
            else if (underlyingType == typeof(ushort))
                supports = CheckEnum(values.Select(val => (ushort)Convert.ChangeType(val, underlyingType)).ToList());
            else if (underlyingType == typeof(int))
                supports = CheckEnum(values.Select(val => (int)Convert.ChangeType(val, underlyingType)).ToList());
            else if (underlyingType == typeof(uint))
                supports = CheckEnum(values.Select(val => (uint)Convert.ChangeType(val, underlyingType)).ToList());
            else if (underlyingType == typeof(long))
                supports = CheckEnum(values.Select(val => (long)Convert.ChangeType(val, underlyingType)).ToList());
            else if (underlyingType == typeof(ulong))
                supports = CheckEnum(values.Select(val => (ulong)Convert.ChangeType(val, underlyingType)).ToList());
            else
                throw new InvalidOperationException($"Unknown underlying enum type: {underlyingType.FullName} (in: {enumType.FullName})");

            if (supports)
            {
                _flaggableEnums.Add(enumType);
                return true;
            }

            _nonFlaggableEnums.Add(enumType);
            return false;
        }

        #region Flags - Has Flag Checks
        public static bool HasInt16(Enum target, Enum flag)
        {
            var tValue = (short)(object)target;
            var fValue = (short)(object)flag;

            return (tValue & fValue) == fValue;
        }

        public static bool HasUInt16(Enum target, Enum flag)
        {
            var tValue = (ushort)(object)target;
            var fValue = (ushort)(object)flag;

            return (tValue & fValue) == fValue;
        }

        public static bool HasInt32(Enum target, Enum flag)
        {
            var tValue = (int)(object)target;
            var fValue = (int)(object)flag;

            return (tValue & fValue) == fValue;
        }

        public static bool HasUInt32(Enum target, Enum flag)
        {
            var tValue = (uint)(object)target;
            var fValue = (uint)(object)flag;

            return (tValue & fValue) == fValue;
        }

        public static bool HasInt64(Enum target, Enum flag)
        {
            var tValue = (long)(object)target;
            var fValue = (long)(object)flag;

            return (tValue & fValue) == fValue;
        }

        public static bool HasUInt64(Enum target, Enum flag)
        {
            var tValue = (ulong)(object)target;
            var fValue = (ulong)(object)flag;

            return (tValue & fValue) == fValue;
        }

        public static bool HasByte(Enum target, Enum flag)
        {
            var tByte = (byte)(object)target;
            var fByte = (byte)(object)flag;

            return (tByte & fByte) == fByte;
        }
        #endregion

        #region Flags - Operations
        public static object OperateInt16(Enum target, Enum flag, EnumOperation operation = EnumOperation.Combine)
        {
            var tValue = (short)(object)target;
            var fValue = (short)(object)flag;

            return operation is EnumOperation.Combine ? (byte)(tValue | fValue) : (byte)(tValue & ~fValue);
        }

        public static object OperateInt32(Enum target, Enum flag, EnumOperation operation = EnumOperation.Combine)
        {
            var tValue = (int)(object)target;
            var fValue = (int)(object)flag;

            return operation is EnumOperation.Combine ? (byte)(tValue | fValue) : (byte)(tValue & ~fValue);
        }

        public static object OperateInt64(Enum target, Enum flag, EnumOperation operation = EnumOperation.Combine)
        {
            var tValue = (long)(object)target;
            var fValue = (long)(object)flag;

            return operation is EnumOperation.Combine ? (byte)(tValue | fValue) : (byte)(tValue & ~fValue);
        }

        public static object OperateUInt16(Enum target, Enum flag, EnumOperation operation = EnumOperation.Combine)
        {
            var tValue = (ushort)(object)target;
            var fValue = (ushort)(object)flag;

            return operation is EnumOperation.Combine ? (byte)(tValue | fValue) : (byte)(tValue & ~fValue);
        }

        public static object OperateUInt32(Enum target, Enum flag, EnumOperation operation = EnumOperation.Combine)
        {
            var tValue = (uint)(object)target;
            var fValue = (uint)(object)flag;

            return operation is EnumOperation.Combine ? (byte)(tValue | fValue) : (byte)(tValue & ~fValue);
        }

        public static object OperateUInt64(Enum target, Enum flag, EnumOperation operation = EnumOperation.Combine)
        {
            var tValue = (ulong)(object)target;
            var fValue = (ulong)(object)flag;

            return operation is EnumOperation.Combine ? (byte)(tValue | fValue) : (byte)(tValue & ~fValue);
        }

        public static object OperateBytes(Enum target, Enum flag, EnumOperation operation = EnumOperation.Combine)
        {
            var tByte = (byte)(object)target;
            var fByte = (byte)(object)flag;

            return operation is EnumOperation.Combine ? (byte)(tByte | fByte) : (byte)(tByte & ~fByte);
        }
        #endregion

        #region Bitwise Checks
        private static bool CheckEnum(List<byte> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (i is 0 || values[i - 1] is 0)
                    continue;

                if ((values[i] / values[i - 1]) != 2)
                    return false;
            }

            return true;
        }

        private static bool CheckEnum(List<sbyte> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (i is 0 || values[i - 1] is 0)
                    continue;

                if ((values[i] / values[i - 1]) != 2)
                    return false;
            }

            return true;
        }

        private static bool CheckEnum(List<short> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (i is 0 || values[i - 1] is 0)
                    continue;

                if ((values[i] / values[i - 1]) != 2)
                    return false;
            }

            return true;
        }

        private static bool CheckEnum(List<ushort> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (i is 0 || values[i - 1] is 0)
                    continue;

                if ((values[i] / values[i - 1]) != 2)
                    return false;
            }

            return true;
        }

        private static bool CheckEnum(List<int> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (i is 0 || values[i - 1] is 0)
                    continue;

                if ((values[i] / values[i - 1]) != 2)
                    return false;
            }

            return true;
        }

        private static bool CheckEnum(List<uint> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (i is 0 || values[i - 1] is 0)
                    continue;

                if ((values[i] / values[i - 1]) != 2)
                    return false;
            }

            return true;
        }

        private static bool CheckEnum(List<long> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (i is 0 || values[i - 1] is 0)
                    continue;

                if ((values[i] / values[i - 1]) != 2)
                    return false;
            }

            return true;
        }

        private static bool CheckEnum(List<ulong> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (i is 0 || values[i - 1] is 0)
                    continue;

                if ((values[i] / values[i - 1]) != 2)
                    return false;
            }

            return true;
        }
        #endregion
    }
}