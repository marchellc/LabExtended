using System.ComponentModel;

using LabExtended.Commands.Tokens;

namespace LabExtended.Core.Configs.Sections;

public class CommandSection
{
    [Description("Whether or not to show debug from the token parser.")]
    public bool TokenParserDebug { get; set; }
    
    [Description("Whether or not to allow pooling command instances.")]
    public bool AllowInstancePooling { get; set; } = true;
    
    [Description("Whether or not to allow custom command to override vanilla commands.")]
    public bool AllowOverride { get; set; }

    [Description("The token used to start a collection.")]
    public char CollectionStartToken
    {
        get => CollectionToken.StartToken;
        set => CollectionToken.StartToken = value;
    }

    [Description("The token used to end a collection.")]
    public char CollectionEndToken
    {
        get => CollectionToken.EndToken;
        set => CollectionToken.EndToken = value;
    }

    [Description("The token used to split a collection item.")]
    public char CollectionSplitToken
    {
        get => CollectionToken.SplitToken;
        set => CollectionToken.SplitToken = value;
    }

    [Description("The token used to start a dictionary.")]
    public char DictionaryStartToken
    {
        get => DictionaryToken.StartToken;
        set => DictionaryToken.StartToken = value;
    }

    [Description("The token used to end a dictionary.")]
    public char DictionaryEndToken
    {
        get => DictionaryToken.EndToken;
        set => DictionaryToken.EndToken = value;
    }

    [Description("The token used to split between a key and value.")]
    public char DictionarySplitToken
    {
        get => DictionaryToken.SplitToken;
        set => DictionaryToken.SplitToken = value;
    }

    [Description("The token used to start a property.")]
    public char PropertyStartToken
    {
        get => PropertyToken.StartToken;
        set => PropertyToken.StartToken = value;
    }

    [Description("The bracket token used to contain the name of the property.")]
    public char PropertyBracketOpenToken
    {
        get => PropertyToken.BracketStartToken;
        set => PropertyToken.BracketStartToken = value;
    }

    [Description("The bracket close token used for properties.")]
    public char PropertyBracketCloseToken
    {
        get => PropertyToken.BracketEndToken;
        set => PropertyToken.BracketEndToken = value;
    }

    [Description("The token used to identify strings.")]
    public char StringToken
    {
        get => Commands.Tokens.StringToken.Token;
        set => Commands.Tokens.StringToken.Token = value;
    }
}