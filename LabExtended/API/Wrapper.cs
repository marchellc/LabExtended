using LabExtended.API.Interfaces;

namespace LabExtended.API
{
    /// <summary>
    /// A wrapper class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Wrapper<T> : IWrapper<T>
    {
        /// <summary>
        /// Creates a new wrapper instance.
        /// </summary>
        /// <param name="baseValue">The wrapped value.</param>
        public Wrapper(T baseValue)
            => Base = baseValue;

        /// <inheritdoc/>
        public virtual T Base { get; }
    }
}