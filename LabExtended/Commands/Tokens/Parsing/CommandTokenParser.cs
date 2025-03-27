using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Tokens.Parsing;

/// <summary>
/// Represents a token parser.
/// </summary>
public abstract class CommandTokenParser
{
    /// <summary>
    /// Whether or not the parser should be terminated.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public virtual bool ShouldTerminate(CommandTokenParserContext context) => false;
    
    /// <summary>
    /// Whether or not the parser should be started.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    /// <returns>true if the parser should be started.</returns>
    public virtual bool ShouldStart(CommandTokenParserContext context) => false;

    /// <summary>
    /// Called once per each character before <see cref="ShouldTerminate"/> and after <see cref="ShouldStart"/>.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    /// <returns>true if the loop should be allowed to continue</returns>
    public virtual bool ProcessContext(CommandTokenParserContext context) => false;
    
    /// <summary>
    /// Called before a token is terminated.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    /// <returns>true if the token should be terminated.</returns>
    public virtual bool OnTerminating(CommandTokenParserContext context) => true;
    
    /// <summary>
    /// Called after a token is terminated.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    public virtual void OnTerminated(CommandTokenParserContext context) { }
    
    /// <summary>
    /// Gets a new instance of the parser's token.
    /// </summary>
    /// <param name="context">The current parsing context.</param>
    /// <returns>The created token instance.</returns>
    public abstract ICommandToken CreateToken(CommandTokenParserContext context);
}