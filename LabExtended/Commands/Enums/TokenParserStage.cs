namespace LabExtended.Commands.Enums;

/// <summary>
/// Represents a stage in the <see cref="CommandTokenParser"/>.
/// </summary>
public enum TokenParserStage
{
    None,
    
    Collection,
    
    DictionaryKey,
    DictionaryValue,
    
    PropertyKey,
    PropertyName,
    
    StringLiteralPropertyKey,
    StringLiteralPropertyName,
    
    StringFull,
    String
}