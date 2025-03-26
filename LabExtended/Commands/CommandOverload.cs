using System.Reflection;

using LabExtended.Extensions;

using LabExtended.Utilities;
using LabExtended.Utilities.Values;

namespace LabExtended.Commands;

using Parameters;

/// <summary>
/// Represents a method of a command.
/// </summary>
public class CommandOverload
{
    /// <summary>
    /// Gets the targeted method.
    /// </summary>
    public MethodInfo Target { get; }
    
    /// <summary>
    /// Gets all parameters from this overload.
    /// </summary>
    public List<CommandParameter> Parameters { get; } = new();
    
    /// <summary>
    /// Gets all parameter builders from this overload.
    /// </summary>
    public Dictionary<string, CommandParameterBuilder> ParameterBuilders { get; } = new();
    
    /// <summary>
    /// Gets the compiled method delegate.
    /// </summary>
    public Func<object, object[], object> Method { get; internal set; }
    
    /// <summary>
    /// Whether or not this overload is a coroutine.
    /// </summary>
    public bool IsCoroutine { get; }
    
    /// <summary>
    /// Whether or not this overload has been initialized.
    /// </summary>
    public bool IsInitialized { get; internal set; }
    
    /// <summary>
    /// Gets the amount of required parameters.
    /// </summary>
    public int RequiredParameters { get; internal set; }
    
    /// <summary>
    /// Gets the amount of parameters.
    /// </summary>
    public int ParameterCount { get; }
    
    /// <summary>
    /// Gets the overload's name.
    /// </summary>
    public string Name { get; internal set; }
    
    /// <summary>
    /// Gets the overload's description.
    /// </summary>
    public string Description { get; internal set; }
    
    /// <summary>
    /// Gets the overload's buffer.
    /// </summary>
    public ReusableValue<object[]> Buffer { get; }

    /// <summary>
    /// Creates a new <see cref="CommandOverload"/> instance.
    /// <param name="target">The method that this overload targets.</param>
    /// </summary>
    public CommandOverload(MethodInfo target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        var parameters = target.GetAllParameters();
        
        Target = target;
        ParameterCount = parameters.Length;
        RequiredParameters = parameters.Count(x => !x.HasDefaultValue);

        IsCoroutine = target.ReturnType == typeof(IEnumerator<float>);
        
        Method = FastReflection.ForMethod(Target);

        foreach (var parameter in parameters)
        {
            var builder = new CommandParameterBuilder(parameter);
            
            ParameterBuilders.Add(parameter.Name, builder);
            Parameters.Add(builder.Result);
        }

        Buffer = new(new object[ParameterCount], () => new object[ParameterCount]);
    }
}