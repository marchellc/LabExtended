using LabApi.Features.Enums;

using LabExtended.API;
using LabExtended.Commands.Interfaces;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.Commands.Contexts;

/// <summary>
/// Represents the context of a command execution.
/// </summary>
public class CommandContext
{
    /// <summary>
    /// Gets the player who used the command.
    /// </summary>
    public ExPlayer Sender { get; internal set; }
    
    /// <summary>
    /// Gets the command's instance.
    /// </summary>
    public CommandBase Instance { get; internal set; }
    
    /// <summary>
    /// Gets the command's data.
    /// </summary>
    public CommandInstance Command { get; internal set; }
    
    /// <summary>
    /// Gets the overload that is being invoked.
    /// </summary>
    public CommandOverload Overload { get; internal set; }
    
    /// <summary>
    /// Gets the list of tokens that were parsed.
    /// </summary>
    public List<ICommandToken> Tokens { get; internal set; }
    
    /// <summary>
    /// Gets the list of arguments (<see cref="Line"/> split by spaces).
    /// </summary>
    public List<string> Args { get; internal set; }
    
    /// <summary>
    /// Gets or sets the response to this context.
    /// </summary>
    public CommandResponse? Response { get; set; }
    
    /// <summary>
    /// Gets the source of the command.
    /// </summary>
    public CommandType Type { get; internal set; }
    
    /// <summary>
    /// Gets the command line.
    /// </summary>
    public string Line { get; internal set; }
}