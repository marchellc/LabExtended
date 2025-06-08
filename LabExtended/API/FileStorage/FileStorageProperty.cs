namespace LabExtended.API.FileStorage;

/// <summary>
/// Represents a property in a file storage component.
/// </summary>
/// <typeparam name="T"></typeparam>
public class FileStorageProperty<T> : FileStoragePropertyBase
{
    internal volatile object value;
    
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
        get
        {
            if (value is null)
                return default;

            return (T)value;
        }
        set
        {
            if (!isLoaded)
            {
                this.value = value;

                isLoaded = true;
                return;
            }

            var curValue = this.value is null ? default(T) : (T)this.value;
            
            if (value is null)
            {
                if (curValue is null)
                    return;
                
                this.value = null;

                IsDirty = true;
                
                Changed?.Invoke(curValue, default);
                return;
            }
            
            this.value = (T)value;
            
            IsDirty = true;
            
            Changed?.Invoke(curValue, value);
        }
    }

    internal FileStorageProperty(string name) : base(name) { }
    
    internal override Type GenericType => typeof(T);

    internal override object NonGenericValue
    {
        get => value;
        set
        {
            if (value is null)
            {
                this.value = null;
                return;
            }
            
            this.value = (T)value;
        }
    }
    
    internal override void InvokeSaved()
        => Saved?.Invoke();

    internal override void InvokeModified(object newValue)
        => Modified?.Invoke(Value, newValue is T t ? t : default);
}