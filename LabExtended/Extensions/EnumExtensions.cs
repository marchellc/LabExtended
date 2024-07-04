using Common.Extensions;

namespace LabExtended.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="Enum"/> class.
    /// </summary>
    public static class EnumExtensions
    {
        private static readonly List<Type> _flaggableEnums = new List<Type>();
        private static readonly List<Type> _nonFlaggableEnums = new List<Type>();

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

            if (supports)
            {
                _flaggableEnums.Add(enumType);
                return true;
            }

            _nonFlaggableEnums.Add(enumType);
            return false;
        }

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
    }
}