using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Commands.Attributes;

/// <summary>
/// A custom attribute used to describe command parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class CommandParameterAttribute : Attribute
{
    /// <summary>
    /// Gets the custom assigned parameter name.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets the custom assigned parameter description.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets a list of assigned arguments.
    /// </summary>
    public List<ICommandParameterRestriction> Restrictions { get; } = new();
    
    /// <summary>
    /// Creates a new <see cref="CommandParameterAttribute"/> instance.
    /// </summary>
    /// <param name="name">The argument's custom name.</param>
    /// <param name="description">The argument's custom description.</param>
    /// <param name="parameterRestrictions">The argument's parameter types.</param>
    public CommandParameterAttribute(string? name, string? description, params Type[] parameterRestrictions) : this(parameterRestrictions)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Creates a new <see cref="CommandParameterAttribute"/> instance.
    /// </summary>
    /// <param name="parameterRestrictions">The arguments for this parameter.</param>
    public CommandParameterAttribute(params Type[] parameterRestrictions)
    {
        for (var i = 0; i < parameterRestrictions.Length; i++)
        {
            var argument = parameterRestrictions[i];
            
            if (argument is null)
                throw new ArgumentException($"Parameter argument type at index {i} cannot be null", nameof(parameterRestrictions));
            
            if (!argument.InheritsType<ICommandParameterRestriction>())
                throw new ArgumentException($"Parameter arguments must implement the ICommandParameterRestriction interface (type: {argument.FullName}, index: {i})", 
                    nameof(parameterRestrictions));
            
            var staticField = argument.FindField(f => f.IsStatic && f is { IsInitOnly: true, Name: "Instance" } && f.FieldType == argument);
            
            if (staticField != null && staticField.GetValue(null) is ICommandParameterRestriction argumentInstance)
                Restrictions.Add(argumentInstance);
            else if (Activator.CreateInstance(argument) is ICommandParameterRestriction argumentInstantiated)
                Restrictions.Add(argumentInstantiated);
            else
                throw new ArgumentException(
                    $"Could not get instance of parameter argument {argument.FullName} (index: {i})");
        }
    }
}