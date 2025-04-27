using Hints;

using LabExtended.API.Interfaces;

using NorthwoodLib.Pools;
using YamlDotNet.Serialization;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API.Messages;

/// <summary>
/// Represents a message in the game's hint system.
/// </summary>
public class HintMessage : IMessage, IDisposable
{
    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    public string? Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the duration of the message (in seconds).
    /// </summary>
    public ushort Duration { get; set; }
    
    /// <summary>
    /// Whether or not this message should be considered priority.
    /// </summary>
    public bool IsPriority { get; set; }

    /// <summary>
    /// Gets a list of the hint's parameters.
    /// </summary>
    [YamlIgnore]
    public List<HintParameter>? Parameters { get; private set; } = new();

    /// <summary>
    /// Creates a new <see cref="HintMessage"/> instance.
    /// </summary>
    public HintMessage() { }
    
    /// <summary>
    /// Creates a new <see cref="HintMessage"/> instance.
    /// </summary>
    /// <param name="content">The hint's content.</param>
    /// <param name="duration">The hint's duration.</param>
    /// <param name="isPriority">Whether or not the hint is considered priority.</param>
    /// <param name="parameters">A collection of the hint's parameters.</param>
    public HintMessage(string content, ushort duration, bool isPriority = false, IEnumerable<HintParameter>? parameters = null)
    {
        Content = content;
        Duration = duration;
        IsPriority = isPriority;
        
        if (parameters != null)
            Parameters.AddRange(parameters);
    }

    /// <summary>
    /// Adds a parameter to the hint.
    /// </summary>
    /// <param name="parameter">The parameter to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public HintMessage AddParameter(HintParameter parameter)
    {
        if (parameter is null)
            throw new ArgumentNullException(nameof(parameter));
        
        Parameters.Add(parameter);
        return this;
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (Parameters != null)
        {
            ListPool<HintParameter>.Shared.Return(Parameters);

            Parameters = null;
        }
    }
}