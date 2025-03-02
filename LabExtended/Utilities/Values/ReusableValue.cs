using LabExtended.API;
using LabExtended.Extensions;

namespace LabExtended.Utilities.Values
{
    public class ReusableValue<T> : IDisposable
        where T : class
    {
        private volatile T _value;

        private volatile Func<T> _newValue;
        private volatile Action<T> _returnValue;
        private volatile Action<T> _poolValue;

        private volatile bool _busy;

        public bool IsUsed => _busy;

        public ReusableValue(T defaultValue = null, Func<T> newValue = null, Action<T> returnValue = null, Action<T> poolValue = null)
        {
            if (defaultValue is null && newValue is null)
                throw new ArgumentNullException(nameof(defaultValue));

            _value = defaultValue is null ? newValue() : defaultValue;
            _newValue = newValue; 
            _poolValue = poolValue;

            _returnValue = returnValue;
            _returnValue.InvokeSafe(_value);

            _busy = false;
        }

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

        public void Do(Action<T> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            var value = Rent();

            action.InvokeSafe(value);

            Return(value);
        }

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