namespace LabExtended.Utilities.Values
{
    public struct HeldValue<T>
    {
        private T _value;

        public T Value
        {
            get => _value;
            set => _value = value;
        }
    }
}