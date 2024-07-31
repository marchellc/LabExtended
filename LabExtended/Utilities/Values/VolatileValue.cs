namespace LabExtended.Utilities.Values
{
    public class VolatileValue<T>
    {
        private T _value;

        public T Value
        {
            get => _value;
            set => _value = value;
        }
    }
}