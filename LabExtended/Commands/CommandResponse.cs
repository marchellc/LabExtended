namespace LabExtended.Commands;

/// <summary>
/// Represents a response to a command.
/// </summary>
public struct CommandResponse
{
    /// <summary>
    /// Whether or not the execution was a success.
    /// </summary>
    public bool IsSuccess { get; }
    
    /// <summary>
    /// Whether or not the command should continue.
    /// </summary>
    public bool IsContinuted { get; }
    
    /// <summary>
    /// Gets the content of the response.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Creates a new <see cref="CommandResponse"/> instance.
    /// </summary>
    /// <param name="isSuccess">Whether or not the command was successfully executed.</param>
    /// <param name="isContinuted">Whether or not the command should be continued.</param>
    /// <param name="content">The content of the reply.</param>
    public CommandResponse(bool isSuccess, bool isContinuted, string content)
    {
        IsSuccess = isSuccess;
        IsContinuted = isContinuted;
        
        Content = content;
    }
}