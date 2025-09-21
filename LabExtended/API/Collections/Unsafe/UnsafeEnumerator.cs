using System.Collections;

namespace LabExtended.API.Collections.Unsafe;

/// <summary>
/// Provides an enumerator for iterating over the elements of an UnsafeList{T} without thread safety guarantees.
/// </summary>
/// <remarks>This enumerator does not provide thread safety. Modifying the underlying UnsafeList{T} during
/// enumeration may result in undefined behavior. The enumerator is intended for performance-critical scenarios where
/// thread safety is managed externally or is not required.</remarks>
/// <typeparam name="T">The type of elements in the collection to enumerate.</typeparam>
public struct UnsafeEnumerator<T> : IEnumerator<T>, IEnumerator
{
    internal static volatile IEnumerator<T> emptyEnumerator = new UnsafeEnumerator<T>();

    private volatile UnsafeList<T> list;
    private volatile int index;
    private T? current;

    /// <summary>
    /// Initializes a new instance of the UnsafeEnumerator{T} class for the specified list.
    /// </summary>
    /// <param name="list">The UnsafeList{T} to enumerate. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if the list parameter is null.</exception>
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

    /// <inheritdoc cref="IEnumerator.MoveNext"/>
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