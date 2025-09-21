using LabExtended.API;
using LabExtended.Extensions;

namespace LabExtended.Utilities.Values
{
    /// <summary>
    /// Represents a field of a poolable or otherwise reusable object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    public class ReusableValue<T> : IDisposable
        where T : class
    {
        private volatile T _value;

        private volatile Func<T> _newValue;
        private volatile Action<T> _returnValue;
        private volatile Action<T> _poolValue;

        private volatile bool _busy;

        /// <summary>
        /// Whether or not the main instance is currently in use.
        /// </summary>
        public bool IsUsed => _busy;

        /// <summary>
        /// Creates a new <see cref="ReusableValue{T}"/> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public ReusableValue(T? defaultValue = null, Func<T>? newValue = null, Action<T>? returnValue = null, Action<T>? poolValue = null)
        {
            if (defaultValue is null && newValue is null)
                throw new ArgumentNullException(nameof(defaultValue));

            _value = defaultValue 
                ?? (newValue?.Invoke() 
                    ?? throw new ArgumentNullException(nameof(newValue)));

            _newValue = newValue!; 
            _poolValue = poolValue!;

            _returnValue = returnValue!;
            _returnValue.InvokeSafe(_value);

            _busy = false;
        }

        /// <summary>
        /// Rents an instance.
        /// </summary>
        /// <returns>The rented instance.</returns>
        /// <remarks>If the main instance is busy and no constructor is provided, the thread will be blocked until the main instance is free.</remarks>
        public T Rent()
        {
            if (_busy)
            {
                if (_newValue is null)
                {
                    while (_busy && ExServer.IsRunning)
                        continue;

                    _busy = true;
                    return _value;
                }
                else
                {
                    return _newValue();
                }
            }

            _busy = true;
            return _value;
        }

        /// <summary>
        /// Returns a rented instance.
        /// </summary>
        /// <param name="value">The rented instance.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Return(T value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            _returnValue.InvokeSafe(value);

            if (_busy)
            {
                _value = value;
                _busy = false;
            }
            else
            {
                _poolValue.InvokeSafe(value);
            }
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            if (_value != null)
            {
                if (!_busy)
                {
                    _returnValue.InvokeSafe(_value);
                    _poolValue.InvokeSafe(_value);
                }
            }

            _value = null;
            _busy = false;

            _newValue = null;
            _poolValue = null;
            _returnValue = null;
        }
    }
}