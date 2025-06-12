using LabApi.Features.Enums;

using LabExtended.API;
using LabExtended.Commands.Interfaces;
using LabExtended.Core.Pooling.Pools;
using NorthwoodLib.Pools;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.Commands;

/// <summary>
/// Represents the context of a command execution.
/// </summary>
public class CommandContext
{
    private volatile ExPlayer sender;
    
    private volatile CommandBase instance;
    private volatile CommandData command;
    private volatile CommandOverload overload;
    private volatile CommandResponse response;

    private volatile ICommandRunner runner;
    private volatile CommandType type;
    private volatile string line;

    private volatile List<ICommandToken> tokens;
    private volatile List<string> args;

    /// <summary>
    /// Gets the player who used the command.
    /// </summary>
    public ExPlayer Sender
    {
        get => sender;
        internal set => sender = value;
    }

    /// <summary>
    /// Gets the command's instance.
    /// </summary>
    public CommandBase Instance
    {
        get => instance;
        internal set => instance = value;
    }

    /// <summary>
    /// Gets the command's data.
    /// </summary>
    public CommandData Command
    {
        get => command;
        internal set => command = value;
    }

    /// <summary>
    /// Gets the overload that is being invoked.
    /// </summary>
    public CommandOverload Overload
    {
        get => overload;
        internal set => overload = value;
    }

    /// <summary>
    /// Gets the assigned runner instance.
    /// </summary>
    public ICommandRunner Runner
    {
        get => runner;
        internal set => runner = value;
    }

    /// <summary>
    /// Gets the list of tokens that were parsed.
    /// </summary>
    public List<ICommandToken> Tokens
    {
        get => tokens;
        internal set => tokens = value;
    }

    /// <summary>
    /// Gets the list of arguments (<see cref="Line"/> split by spaces).
    /// </summary>
    public List<string> Args
    {
        get => args;
        internal set => args = value;
    }

    /// <summary>
    /// Gets or sets the response to this context.
    /// </summary>
    public CommandResponse Response
    {
        get => response;
        set => response = value;
    }

    /// <summary>
    /// Gets the source of the command.
    /// </summary>
    public CommandType Type
    {
        get => type;
        internal set => type = value;
    }

    /// <summary>
    /// Gets the command line.
    /// </summary>
    public string Line
    {
        get => line;
        internal set => line = value;
    }
}