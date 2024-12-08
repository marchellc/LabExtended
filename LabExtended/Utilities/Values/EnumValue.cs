using LabExtended.Extensions;

namespace LabExtended.Utilities.Values
{
    /// <summary>
    /// A helper class used for managing flag enums.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    public struct EnumValue<TEnum> where TEnum : struct, Enum
    {
        private Func<TEnum> _getter;
        private Action<TEnum> _setter;

        /// <summary>
        /// Creates a new <see cref="EnumValue{TEnum}"/> instance.
        /// </summary>
        /// <param name="getter">The lambda function used to retrieve the current value.</param>
        /// <param name="setter">The lambda function used to set a new value.</param>
        public EnumValue(Func<TEnum> getter, Action<TEnum> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        /// <summary>
        /// Gets or sets the enum's current value.
        /// </summary>
        public TEnum Value
        {
            get => _getter();
            set => _setter(value);
        }

        /// <summary>
        /// Gets a value indicating whether or not a <paramref name="flag"/> is present in the enum's current value.
        /// </summary>
        /// <param name="flag">The flag to check for.</param>
        /// <returns>true if the flag is present, otherwise false.</returns>
        public bool HasFlag(TEnum flag)
            => Value.Any(flag);

        /// <summary>
        /// Adds a flag.
        /// </summary>
        /// <param name="flag">The flag to add.</param>
        public EnumValue<TEnum> AddFlag(TEnum flag)
        {
            if (Value.Any(flag))
                return this;

            Value = Value.Combine(flag);
            return this;
        }

        /// <summary>
        /// Removes a flag.
        /// </summary>
        /// <param name="flag">The flag to remove.</param>
        public EnumValue<TEnum> RemoveFlag(TEnum flag)
        {
            if (!Value.Any(flag))
                return this;

            Value = Value.Remove(flag);
            return this;
        }

        /// <summary>
        /// Sets a status of a flag.
        /// </summary>
        /// <param name="flag">The flag to set.</param>
        /// <param name="value">Whether to add or remove it.</param>
        public EnumValue<TEnum> SetFlag(TEnum flag, bool value)
            => (value ? AddFlag(flag) : RemoveFlag(flag));
    }
}