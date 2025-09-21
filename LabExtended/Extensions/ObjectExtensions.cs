namespace LabExtended.Extensions;

/// <summary>
/// Extensions targeting anonymous objects.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Whether or not the object is of a specific type.
    /// </summary>
    public static bool Is<T>(this object instance)
        => instance != null && instance is T;

    /// <summary>
    /// Whether or not the object is of a specific type.
    /// </summary>
    public static bool Is<T>(this object instance, out T result)
    {
        result = default!;

        if (instance is null || instance is not T cast)
            return false;

        result = cast;
        return true;
    }

    /// <summary>
    /// Whether or not an object is equal to another.
    /// </summary>
    /// <remarks>Will always return false if one of these objects is null, <paramref name="countNull"/> controls what to return if both objects are null.</remarks>
    public static bool IsEqualTo(this object instance, object otherInstance, bool countNull = false)
    {
        if (instance is null && otherInstance is null)
            return countNull;

        if ((instance is null && otherInstance != null) || (instance != null && otherInstance is null))
            return false;

        return instance == otherInstance;
    }

    /// <summary>
    /// Copies all properties that have a setter and a getter from one object instance to another.
    /// </summary>
    public static void CopyPropertiesFrom(this object target, object instance)
        => CopyPropertiesTo(instance, target);

    /// <summary>
    /// Copies all properties that have a setter and a getter from one object instance to another.
    /// </summary>
    public static void CopyPropertiesTo(this object instance, object target)
    {
        if (instance is null || target is null)
            return;

        var props = instance.GetType().GetAllProperties();

        foreach (var prop in props)
        {
            if (prop.GetSetMethod(true) is null)
                continue;

            if (prop.GetGetMethod(true) is null)
                continue;

            prop.SetValue(target, prop.GetValue(instance));
        }
    }
}