namespace LabExtended.API.FileStorage;

/// <summary>
/// Represents a property in a file storage component.
/// </summary>
/// <typeparam name="T"></typeparam>
public class FileStorageProperty<T> : FileStoragePropertyBase
{
    /// <summary>
    /// Gets called once the value is changed via code.
    /// </summary>
    public event Action<T?, T?>? Changed;

    /// <summary>
    /// Gets called once the value is modified via file.
    /// </summary>
    public event Action<T?, T?> Modified; 
    
    /// <summary>
    /// Gets called once the changes are saved.
    /// </summary>
    public event Action? Saved; 
    
    /// <summary>
    /// Gets the parent component.
    /// </summary>
    public FileStorageComponent Component { get; internal set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public T? Value
    {
        get => field;
        set
        {
            var curValue = field;
            
            if (value is null)
            {
                if (curValue is null)
                    return;
                
                field = default;

                IsDirty = true;
                
                Changed?.Invoke(curValue, field);
                return;
            }
            
            field = (T)value;
            
            IsDirty = true;
            
            Changed?.Invoke(curValue, field);
        }
    }

    internal FileStorageProperty(string name) : base(name) { }
    
    internal override Type GenericType => typeof(T);

    internal override object NonGenericValue
    {
        get => Value;
        set
        {
            if (value is null)
            {
                Value = default;
                return;
            }
            
            Value = (T)value;
        }
    }
    
    internal override void InvokeSaved()
        => Saved?.Invoke();

    internal override void InvokeModified(object newValue)
        => Modified?.Invoke(Value, newValue is T t ? t : default);
}