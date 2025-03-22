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
    public List<ICommandParameterArgument> Arguments { get; } = new();
    
    /// <summary>
    /// Creates a new <see cref="CommandParameterAttribute"/> instance.
    /// </summary>
    /// <param name="name">The argument's custom name.</param>
    /// <param name="description">The argument's custom description.</param>
    /// <param name="parameterArguments">The argument's parameter types.</param>
    public CommandParameterAttribute(string? name, string? description, params Type[] parameterArguments) : this(parameterArguments)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Creates a new <see cref="CommandParameterAttribute"/> instance.
    /// </summary>
    /// <param name="parameterArguments">The arguments for this parameter.</param>
    public CommandParameterAttribute(params Type[] parameterArguments)
    {
        for (var i = 0; i < parameterArguments.Length; i++)
        {
            var argument = parameterArguments[i];
            
            if (argument is null)
                throw new ArgumentException($"Parameter argument type at index {i} cannot be null", nameof(parameterArguments));
            
            if (!argument.InheritsType<ICommandParameterArgument>())
                throw new ArgumentException($"Parameter arguments must implement the ICommandParameterArgument interface (type: {argument.FullName}, index: {i})", 
                    nameof(parameterArguments));
            
            var staticField = argument.FindField(f => f.IsStatic && f is { IsInitOnly: true, Name: "Instance" } && f.FieldType == argument);
            
            if (staticField != null && staticField.GetValue(null) is ICommandParameterArgument argumentInstance)
                Arguments.Add(argumentInstance);
            else if (Activator.CreateInstance(argument) is ICommandParameterArgument argumentInstantiated)
                Arguments.Add(argumentInstantiated);
            else
                throw new ArgumentException(
                    $"Could not get instance of parameter argument {argument.FullName} (index: {i})");
        }
    }
}