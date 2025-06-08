using Newtonsoft.Json.Linq;

namespace LabExtended.API.FileStorage;

/// <summary>
/// Base class for a storage property.
/// </summary>
public abstract class FileStoragePropertyBase : IDisposable
{
    internal volatile JObject propertyObject;
    
    internal volatile bool isDirty;
    internal volatile bool isLoaded;
    
    internal volatile string name;

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string Name => name;

    /// <summary>
    /// Whether or not the property is dirty.
    /// </summary>
    public bool IsDirty
    {
        get => isDirty;
        set => isDirty = value;
    }
    
    internal FileStoragePropertyBase(string name)
        => this.name = name;
    
    internal abstract object NonGenericValue { get; set; }
    internal abstract Type GenericType { get; }

    internal abstract void InvokeSaved();

    internal abstract void InvokeModified(object newValue);
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public virtual void Dispose()
    {
        
    }
}