using LabExtended.Extensions;

namespace LabExtended.Utilities.Values;

public class LockedValue<T> : IDisposable
    where T : class
{
    private volatile AutoResetEvent _lock;
    private volatile Action<T> _setter;
    private volatile T _value;

    public LockedValue()
    {
        _lock = new AutoResetEvent(true);
        _setter = value => _value = value;
    }

    public LockedValue(T value)
    {
        _lock = new AutoResetEvent(true);
        _value = value;
        _setter = val => _value = val;
    }

    public void Access(Action<Action<T>, T> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        _lock.WaitOne();

        action.InvokeSafe(_setter, _value);

        _lock.Set();
    }

    public void Dispose()
    {
        if (_lock != null)
        {
            _lock.Dispose();
            _lock = null;
        }

        _value = null;
        _setter = null;
    }
}