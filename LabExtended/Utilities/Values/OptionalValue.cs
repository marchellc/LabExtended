namespace LabExtended.Utilities.Values
{
    public struct OptionalValue<T>
    {
        internal T _value;
        internal bool _hasValue;

        public T Value
        {
            get
            {
                if (!_hasValue)
                    throw new InvalidOperationException($"No value has been provided.");

                return _value;
            }
        }

        public bool HasValue => _hasValue;

        public static OptionalValue<T> FromValue(T value)
            => new OptionalValue<T>() { _value = value, _hasValue = true };

        public static OptionalValue<T> FromNull()
            => new OptionalValue<T>() { _value = default, _hasValue = false };
    }
}
