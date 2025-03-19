namespace LabExtended.Commands.Enums;

using Tokens.Parsing;

/// <summary>
/// Represents a stage in the <see cref="CommandTokenParser"/>.
/// </summary>
public enum TokenParserStage
{
    /// <summary>
    /// No formatting is currently ongoing.
    /// </summary>
    None,
    
    /// <summary>
    /// A collection is being formatted.
    /// </summary>
    Collection,
    
    /// <summary>
    /// A dictionary key is being formatted.
    /// </summary>
    DictionaryKey,
    
    /// <summary>
    /// A dictionary value is being formatted.
    /// </summary>
    DictionaryValue,
    
    /// <summary>
    /// A property key is being formatted.
    /// </summary>
    PropertyKey,
    
    /// <summary>
    /// A property value is being formatted.
    /// </summary>
    PropertyName,
    
    /// <summary>
    /// An enclosed string is being formatted.
    /// </summary>
    StringFull,
    
    /// <summary>
    /// A string is being formatted.
    /// </summary>
    String
}