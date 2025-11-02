using System.ComponentModel;

using LabExtended.Commands.Tokens;

namespace LabExtended.Core.Configs.Sections;

/// <summary>
/// Represents configuration settings for command parsing, including token definitions and parser behavior options.
/// </summary>
public class CommandSection
{
    /// <summary>
    /// Gets or sets a value indicating whether command instance pooling is allowed.
    /// </summary>
    [Description("Whether or not to allow pooling command instances.")]
    public bool AllowInstancePooling { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether custom commands can override vanilla commands.
    /// </summary>
    [Description("Whether or not to allow custom commands to override vanilla commands.")]
    public bool AllowOverride { get; set; }

    /// <summary>
    /// Gets or sets the character token that indicates the start of a collection.
    /// </summary>
    [Description("The token used to start a collection.")]
    public char CollectionStartToken
    {
        get => CollectionToken.StartToken;
        set => CollectionToken.StartToken = value;
    }

    /// <summary>
    /// Gets or sets the character token that marks the end of a collection.
    /// </summary>
    [Description("The token used to end a collection.")]
    public char CollectionEndToken
    {
        get => CollectionToken.EndToken;
        set => CollectionToken.EndToken = value;
    }

    /// <summary>
    /// Gets or sets the character used to split items in a collection.
    /// </summary>
    [Description("The token used to split a collection item.")]
    public char CollectionSplitToken
    {
        get => CollectionToken.SplitToken;
        set => CollectionToken.SplitToken = value;
    }

    /// <summary>
    /// Gets or sets the character token that indicates the start of a dictionary structure.
    /// </summary>
    [Description("The token used to start a dictionary.")]
    public char DictionaryStartToken
    {
        get => DictionaryToken.StartToken;
        set => DictionaryToken.StartToken = value;
    }

    /// <summary>
    /// Gets or sets the character token that indicates the end of a dictionary structure.
    /// </summary>
    [Description("The token used to end a dictionary.")]
    public char DictionaryEndToken
    {
        get => DictionaryToken.EndToken;
        set => DictionaryToken.EndToken = value;
    }

    /// <summary>
    /// Gets or sets the character used to separate keys and values in dictionary representations.
    /// </summary>
    [Description("The token used to split between a key and value.")]
    public char DictionarySplitToken
    {
        get => DictionaryToken.SplitToken;
        set => DictionaryToken.SplitToken = value;
    }

    /// <summary>
    /// Gets or sets the character token that indicates the start of a property.
    /// </summary>
    [Description("The token used to start a property.")]
    public char PropertyStartToken
    {
        get => PropertyToken.StartToken;
        set => PropertyToken.StartToken = value;
    }

    /// <summary>
    /// Gets or sets the character used to open a property name bracket.
    /// </summary>
    [Description("The bracket token used to contain the name of the property.")]
    public char PropertyBracketOpenToken
    {
        get => PropertyToken.BracketStartToken;
        set => PropertyToken.BracketStartToken = value;
    }

    /// <summary>
    /// Gets or sets the character used to represent the closing bracket for properties.
    /// </summary>
    [Description("The bracket close token used for properties.")]
    public char PropertyBracketCloseToken
    {
        get => PropertyToken.BracketEndToken;
        set => PropertyToken.BracketEndToken = value;
    }

    /// <summary>
    /// Gets or sets the character used to identify string tokens in command parsing.
    /// </summary>
    [Description("The token used to identify strings.")]
    public char StringToken
    {
        get => Commands.Tokens.StringToken.Token;
        set => Commands.Tokens.StringToken.Token = value;
    }
}