namespace LabExtended.Commands.Attributes;

/// <summary>
/// Marks a class as a command handler.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the command.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description of the command.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether or not this command should use a static instance.
    /// </summary>
    public bool IsStatic { get; set; } = false;

    /// <summary>
    /// Gets a list of the command's aliases.
    /// </summary>
    public List<string> Aliases { get; } = new();
    
    /// <summary>
    /// Creates a new <see cref="CommandAttribute"/> instance.
    /// </summary>
    public CommandAttribute() { }

    /// <summary>
    /// Creates a new <see cref="CommandAttribute"/> instance.
    /// </summary>
    /// <param name="name">The command's name.</param>
    /// <param name="description">The command's description.</param>
    /// <param name="aliases">The command's aliases.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public CommandAttribute(string name, string description, params string[] aliases)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentNullException(nameof(description));
        
        Name = name;
        Description = description;

        for (int i = 0; i < aliases.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(aliases[i]))
                throw new ArgumentNullException(nameof(aliases));
            
            if (Aliases.Contains(aliases[i]))
                throw new Exception($"Duplicate alias: {aliases[i]}");
            
            Aliases.Add(aliases[i]);
        }
    }
}