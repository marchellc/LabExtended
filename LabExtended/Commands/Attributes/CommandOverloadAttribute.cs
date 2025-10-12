namespace LabExtended.Commands.Attributes;

/// <summary>
/// Marks a method inside a <see cref="CommandBase"/> subtype as a command overload.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
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
    /// Gets the overload's required permission node.
    /// </summary>
    public string? Permission { get; }

    /// <summary>
    /// Creates a new <see cref="CommandOverloadAttribute"/> instance.
    /// </summary>
    public CommandOverloadAttribute(string description = "No description", string? permission = null)
    {
        isDefaultOverload = true;

        Description = description;
        Permission = permission;
    }

    /// <summary>
    /// Creates a new <see cref="CommandOverloadAttribute"/> instance.
    /// </summary>
    /// <param name="name">Name of the overload.</param>
    /// <param name="description">Description of the overload.</param>
    /// <param name="permission">Sets the permission required to invoke the overload.</param>
    public CommandOverloadAttribute(string name, string description = "No description", string? permission = null)
    {
        Name = name;
        Description = description;
        Permission = permission;
    }
}