using LabApi.Features.Enums;

using LabExtended.API;

using LabExtended.Commands.Contexts;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parameters;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.Commands;

/// <summary>
/// Represents the base class for a command.
/// </summary>
public class CommandBase
{
    /// <summary>
    /// Gets the context of the current command execution.
    /// </summary>
    public CommandContext Context { get; internal set; }
    
    /// <summary>
    /// Gets the sender of the command.
    /// </summary>
    public ExPlayer Sender => Context.Sender;
    
    /// <summary>
    /// Gets information about the currently executing command.
    /// </summary>
    public CommandInstance CommandData => Context.Command;
    
    /// <summary>
    /// Gets the overload that is being executed.
    /// </summary>
    public CommandOverload Overload => Context.Overload;
    
    /// <summary>
    /// Gets a list of arguments split by space.
    /// </summary>
    public List<string> Args  => Context.Args;
    
    /// <summary>
    /// Gets a list of parsed command tokens.
    /// </summary>
    public List<ICommandToken> Tokens => Context.Tokens;
    
    /// <summary>
    /// Gets the type of the executing command.
    /// </summary>
    public CommandType CommandType => Context.Type;
    
    /// <summary>
    /// Gets the full command line.
    /// </summary>
    public string Line => Context.Line;

    /// <summary>
    /// Gets or sets the context's response.
    /// </summary>
    public CommandResponse? Response
    {
        get => Context.Response;
        set => Context.Response = value;
    }

    /// <summary>
    /// Called when an overload is called for the first time.
    /// </summary>
    /// <param name="overloadName">The name of the overload.</param>
    /// <param name="parameters">The overload's parameters.</param>
    public virtual void OnInitializeOverload(string overloadName,
        Dictionary<string, CommandParameterBuilder> parameters) { }

    /// <summary>
    /// Responds to the command.
    /// </summary>
    /// <param name="content">The message to respond with.</param>
    /// <param name="isSuccess">Whether or not the command was succesfully executed.</param>
    public void Respond(object content, bool isSuccess = true)
        => Response = new(isSuccess, false, content.ToString());
    
    /// <summary>
    /// Responds to the command with a success.
    /// </summary>
    /// <param name="content">The message to respond with.</param>
    public void Ok(object content)
        => Response = new(true, false, content.ToString());
    
    /// <summary>
    /// Responds to the command with a failure.
    /// </summary>
    /// <param name="content">The message to respond with.</param>
    public void Fail(object content)
        => Response = new(false, false, content.ToString());

    /// <summary>
    /// Writes a message into the sender's console.
    /// </summary>
    /// <param name="content">The message to show.</param>
    public void Write(object content)
        => Sender.SendRemoteAdminMessage(content, true, true, CommandData.Name);
}