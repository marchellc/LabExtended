using System.Collections;

using HarmonyLib;

using LabExtended.Commands.Tokens;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Interfaces;

using LabExtended.Utilities;

namespace LabExtended.Commands.Parameters.Parsers.Wrappers;

/// <summary>
/// A <see cref="Dictionary{TKey,TValue}"/> parser.
/// </summary>
public class DictionaryWrapperParser : CommandParameterParser
{
    /// <summary>
    /// Creates a new <see cref="DictionaryWrapperParser"/> instance.
    /// </summary>
    /// <param name="dictionaryType">The dictionary type.</param>
    /// <param name="keyParser">The key parser.</param>
    /// <param name="valueParser">The value parser.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DictionaryWrapperParser(Type dictionaryType, CommandParameterParser keyParser, CommandParameterParser valueParser)
    {
        if (dictionaryType is null)
            throw new ArgumentNullException(nameof(dictionaryType));
        
        if (keyParser is null)
            throw new ArgumentNullException(nameof(keyParser));
        
        if (valueParser is null)
            throw new ArgumentNullException(nameof(valueParser));

        DictionaryConstructor = FastReflection.ForConstructor(AccessTools.Constructor(dictionaryType, [typeof(int)]));
        DictionaryType = dictionaryType;
        ValueParser = valueParser;
        KeyParser = keyParser;
        
        IsStringDictionary = dictionaryType == typeof(Dictionary<string, string>);
    }
    
    /// <summary>
    /// Gets the dictionary type.
    /// </summary>
    public Type DictionaryType { get; }
    
    /// <summary>
    /// Gets the key parser.
    /// </summary>
    public CommandParameterParser KeyParser { get; }
    
    /// <summary>
    /// Gets the value parser.
    /// </summary>
    public CommandParameterParser ValueParser { get; }
    
    /// <summary>
    /// Gets the constructor of the dictionary.
    /// </summary>
    public Func<object[], object> DictionaryConstructor { get; }
    
    /// <summary>
    /// Whether or not the dictionary has a string key and value.
    /// </summary>
    public bool IsStringDictionary { get; }

    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex, CommandContext context,
        CommandParameter parameter)
    {
        if (token is PropertyToken propertyToken
            && propertyToken.TryGet<object>(context, null, out var result))
        {
            if (result.GetType() != parameter.Type.Type)
                return new(false, null, $"Unsupported property type: {result.GetType().FullName}", parameter);
            
            return new(true, result, null, parameter);
        }
        
        if (token is not DictionaryToken dictionaryToken)
            return new(false, null, $"Unsupported token type: {token.GetType().FullName}", parameter);

        if (IsStringDictionary)
            return new(true, dictionaryToken.Values, null, parameter);
        
        var dictionaryIndex = 0;
        var dictionary = (IDictionary)(DictionaryConstructor([dictionaryToken.Values.Count]));

        var stringToken = StringToken.Instance.NewToken<StringToken>();
        
        foreach (var pair in dictionaryToken.Values)
        {
            stringToken.Value = pair.Key;
            
            var keyResult = KeyParser.Parse(tokens, stringToken, -1, context, parameter);

            if (!keyResult.Success)
                return new(false, null,
                    $"Could not convert key of pair at position {dictionaryIndex}: {keyResult.Error}", parameter);

            stringToken.Value = pair.Value;
            
            var valueResult = ValueParser.Parse(tokens, stringToken, -1, context, parameter);

            if (!valueResult.Success)
                return new(false, null,
                    $"Could not convert value of pair at position {dictionaryIndex}: {valueResult.Error}", parameter);
            
            dictionary.Add(pair.Key, valueResult.Value);
            dictionaryIndex++;
        }

        stringToken.ReturnToken();
        return new(true, dictionary, null, parameter);
    }
}