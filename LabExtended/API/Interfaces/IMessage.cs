namespace LabExtended.API.Interfaces;

/// <summary>
/// Represents common properties for displayable messages.
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    string? Content { get; set; }

    /// <summary>
    /// Gets or sets the duration of the message (in seconds).
    /// </summary>
    ushort Duration { get; set; }
}