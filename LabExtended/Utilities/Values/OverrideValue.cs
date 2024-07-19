namespace LabExtended.Utilities.Values
{
    public class OverrideValue<T>
    {
        private OptionalValue<T> _override;

        public T Value
        {
            get => _override.Value;
            set => _override = OptionalValue<T>.FromValue(value);
        }

        public bool HasValue => _override.HasValue;

        public OverrideValue()
        {
            _override = OptionalValue<T>.FromNull();
        }

        public void ClearValue()
            => _override = OptionalValue<T>.FromNull();
    }
}