namespace LabExtended.Commands.Attributes;

/// <summary>
/// Marks a method inside a <see cref="CommandBase"/> subtype as a command overload.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CommandOverloadAttribute : Attribute
{
    internal readonly bool isDefaultOverload = false;
    
    /// <summary>
    /// Gets the overload's custom name.
    /// </summary>
    public string? Name { get; }
    
    /// <summary>
    /// Gets the overload's description.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Creates a new <see cref="CommandOverloadAttribute"/> instance.
    /// </summary>
    public CommandOverloadAttribute() => isDefaultOverload = true;

    /// <summary>
    /// Creates a new <see cref="CommandOverloadAttribute"/> instance.
    /// </summary>
    /// <param name="name">Name of the overload.</param>
    /// <param name="description">Description of the overload.</param>
    public CommandOverloadAttribute(string name, string description = "No description")
    {
        Name = name;
        Description = description;
    }
}