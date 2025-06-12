namespace LabExtended.Commands;

/// <summary>
/// Represents a response to a command.
/// </summary>
public class CommandResponse
{
    private volatile bool success;
    private volatile bool continued;
    private volatile bool input;

    private volatile string text;

    internal volatile Action<string> onInput;

    /// <summary>
    /// Whether or not the execution was a success.
    /// </summary>
    public bool IsSuccess => success;

    /// <summary>
    /// Whether or not the command should continue.
    /// </summary>
    public bool IsContinued => continued;
    
    /// <summary>
    /// Whether or not the command requires more input.
    /// </summary>
    public bool IsInput => input;

    /// <summary>
    /// Gets the content of the response.
    /// </summary>
    public string Content => text;

    /// <summary>
    /// Creates a new <see cref="CommandResponse"/> instance.
    /// </summary>
    /// <param name="isSuccess">Whether or not the command was successfully executed.</param>
    /// <param name="isContinued">Whether or not the command should be continued.</param>
    /// <param name="isInput">Whether or not more input is needed.</param>
    /// <param name="onInput">The target input delegate.</param>
    /// <param name="content">The content of the reply.</param>
    public CommandResponse(bool isSuccess, bool isContinued, bool isInput, Action<string> onInput, string content)
    {
        success = isSuccess;
        continued = isContinued;
        input = isInput;
        text = content;

        this.onInput = onInput;
    }
}