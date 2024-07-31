namespace LabExtended.API.Interfaces
{
    /// <summary>
    /// A wrapper class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWrapper<T>
    {
        /// <summary>
        /// Gets the wrapped value.
        /// </summary>
        public T Base { get; }
    }
}