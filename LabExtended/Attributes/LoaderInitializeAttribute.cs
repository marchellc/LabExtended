namespace LabExtended.Attributes;

/// <summary>
/// Tells the loader that this method should be called upon load.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class LoaderInitializeAttribute : Attribute
{
    /// <summary>
    /// The priority of the method.
    /// <remarks>Negative values means that the method will be called BEFORE LabAPI starts loading plugins, positive will be called AFTER.</remarks>
    /// </summary>
    public short Priority { get; }

    /// <summary>
    /// Creates a new <see cref="LoaderInitializeAttribute"/> instance.
    /// </summary>
    /// <param name="priority">The loading priority. Negative values means that the method will be called BEFORE LabAPI starts loading plugins, positive will be called AFTER.</param>
    public LoaderInitializeAttribute(short priority)
        => Priority = priority;
}