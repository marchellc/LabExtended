using System.Collections;

namespace LabExtended.API.Collections.Unsafe;

public struct UnsafeEnumerator<T> : IEnumerator<T>, IEnumerator
{
    internal static volatile IEnumerator<T> emptyEnumerator = new UnsafeEnumerator<T>();

    private volatile UnsafeList<T> list;
    private volatile int index;
    private T? current;

    public UnsafeEnumerator(UnsafeList<T> list)
    {
        if (list is null)
            throw new ArgumentNullException(nameof(list));
        
        this.list = list;
        this.index = 0;
        this.current = default;
    }
    
    /// <inheritdoc cref="IEnumerator{T}.Current"/>
    public T Current => current!;
    
    /// <inheritdoc cref="IEnumerator{T}.Current"/>
    object? IEnumerator.Current
    {
        get
        {
            if (index == 0 || index == list.size + 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            return Current;
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose() { }

    /// <inheritdoc cref="IEnumerator{T}.MoveNext"/>
    public bool MoveNext()
    {
        if (((uint)index < (uint)list.size))
        {
            var curIndex = index;

            index = curIndex + 1;
            
            current = list.array[curIndex];
            return true;
        }

        return MoveNextRare();
    }
    
    /// <inheritdoc cref="IEnumerator.Reset"/>
    void IEnumerator.Reset()
    {
        index = 0;
        current = default;
    }

    private bool MoveNextRare()
    {
        index = list.size + 1;
        current = default;
        
        return false;
    }
}