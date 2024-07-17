namespace LabExtended.Utilities.Values
{
    public class OverrideValue<T>
    {
        private Func<T> _getter;
        private OptionalValue<T> _override;

        public T Value
        {
            get
            {
                if (_override.HasValue)
                    return _override.Value;

                return _getter();
            }
            set
            {
                _override = OptionalValue<T>.FromValue(value);
            }
        }

        public OverrideValue(Func<T> getter)
        {
            _getter = getter;
            _override = OptionalValue<T>.FromNull();
        }

        public void ClearValue()
            => _override = OptionalValue<T>.FromNull();
    }
}