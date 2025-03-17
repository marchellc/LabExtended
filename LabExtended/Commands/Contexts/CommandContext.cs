using LabApi.Features.Enums;

using LabExtended.API;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Contexts;

/// <summary>
/// Represents the context of a command execution.
/// </summary>
public struct CommandContext
{
    /// <summary>
    /// Gets the player who used the command.
    /// </summary>
    public ExPlayer Sender { get; }
    
    /// <summary>
    /// Gets the command's instance.
    /// </summary>
    public CommandBase Instance { get; }
    
    /// <summary>
    /// Gets the command's data.
    /// </summary>
    public CommandInstance Command { get; }
    
    /// <summary>
    /// Gets the overload that is being invoked.
    /// </summary>
    public CommandOverload Overload { get; }
    
    /// <summary>
    /// Gets the list of tokens that were parsed.
    /// </summary>
    public List<ICommandToken> Tokens { get; }
    
    /// <summary>
    /// Gets the list of arguments (<see cref="Line"/> split by spaces).
    /// </summary>
    public List<string> Args { get; }
    
    /// <summary>
    /// Gets or sets the response to this context.
    /// </summary>
    public CommandResponse? Response { get; set; }
    
    /// <summary>
    /// Gets the source of the command.
    /// </summary>
    public CommandType Type { get; }
    
    /// <summary>
    /// Gets the command line.
    /// </summary>
    public string Line { get; }

    /// <summary>
    /// Creates a new <see cref="CommandContext"/> instance.
    /// </summary>
    /// <param name="sender">Player who sent the command.</param>
    /// <param name="instance">Instance of the command handler.</param>
    /// <param name="data">Data of the command.</param>
    /// <param name="overload">Overload of the command.</param>
    /// <param name="tokens">The parsed tokens.</param>
    /// <param name="args">The parsed arguments.</param>
    /// <param name="type">The command source type.</param>
    /// <param name="line">The command's full line.</param>
    public CommandContext(ExPlayer sender, CommandBase instance, CommandInstance data, CommandOverload overload,
        List<ICommandToken> tokens, List<string> args, CommandType type, string line)
    {
        Sender = sender;
        Instance = instance;
        Command = data;
        Overload = overload;
        Tokens = tokens;
        Args = args;
        Type = type;
        Line = line;
    }
}