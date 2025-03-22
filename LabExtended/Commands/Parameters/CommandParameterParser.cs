using LabExtended.Commands.Contexts;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters;

/// <summary>
/// Represents a parameter parser.
/// </summary>
public abstract class CommandParameterParser
{
    /// <summary>
    /// Gets the override usage alias for <see cref="CommandSystem.IUsageProvider.Usage"/>.
    /// </summary>
    public virtual string? UsageAlias { get; } = string.Empty;
    
    /// <summary>
    /// Gets the type's friendly alias.
    /// </summary>
    public virtual string? FriendlyAlias { get; } = string.Empty;
    
    /// <summary>
    /// Gets a value indicating whether or not the token can be accepted by this parameter.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>true if the token is acceptable</returns>
    public virtual bool AcceptsToken(ICommandToken token)
    {
        if (token is null)
            throw new ArgumentNullException(nameof(token));

        return true;
    }

    /// <summary>
    /// Parses a token into an argument.
    /// </summary>
    /// <param name="tokens">All parsed tokens.</param>
    /// <param name="token">The current token to parse.</param>
    /// <param name="tokenIndex">The index of the current token.</param>
    /// <param name="context">The command context.</param>
    /// <param name="parameter">The parameter to parse.</param>
    /// <returns>The result of parsing.</returns>
    public abstract CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex, 
        CommandContext context, CommandParameter parameter);
}