namespace LabExtended.Commands.Attributes;

/// <summary>
/// Marks a method inside a <see cref="CommandBase"/> subtype as a command overload.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CommandOverloadAttribute : Attribute
{
    /// <summary>
    /// Gets the overload's custom name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Creates a new <see cref="CommandOverloadAttribute"/> instance.
    /// </summary>
    /// <param name="customName">The custom name of the overload.</param>
    public CommandOverloadAttribute(string? customName = null)
    {
        Name = customName;
    }
}