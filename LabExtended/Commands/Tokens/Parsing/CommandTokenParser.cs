using LabExtended.Commands.Enums;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Interfaces;

using LabExtended.Core;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8605 // Unboxing a possibly null value.

namespace LabExtended.Commands.Tokens.Parsing;

/// <summary>
/// Used to parse command argument values.
/// </summary>
public static class CommandTokenParser
{
    /// <summary>
    /// Gets or sets the character used to identify escape tokens.
    /// </summary>
    public static char EscapeToken { get; set; } = '\\';

    /// <summary>
    /// Parsers all tokens in a string.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <param name="list">The list of tokens to save parsed tokens to.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static CommandTokenParserResult ParseTokens(string input, List<ICommandToken> list, int parameterCount)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentNullException(nameof(input));
        
        if (list is null)
            throw new ArgumentNullException(nameof(list));
        
        var currentToken = default(ICommandToken);
        var currentStage = TokenParserStage.None;

        var previousChar = default(char?);
        var nextChar = default(char?);

        var isTokenStart = false;
        var isAppended = false;
        
        ApiLog.Debug("Command Token Parser", $"Parsing input: &1{input}&r");

        for (int i = 0; i < input.Length; i++)
        {
            var currentChar = input[i];

            if (i + 1 < input.Length)
                nextChar = input[i + 1];
            else
                nextChar = null;

            if (i - 1 >= 0)
                previousChar = input[i - 1];
            else
                previousChar = null;

            ApiLog.Debug("Command Token Parser", $"\nLooping Start" +
                                               $"\n&3Index&r: &6{i}&r" +
                                               $"\n&3Current&r: &6{currentChar}&r" +
                                               $"\n&3Previous&r: &6{previousChar}&r" +
                                               $"\n&3Next&r: &6{nextChar}&r" +
                                               $"\n&3Is Token Start&r: &6{isTokenStart}&r" +
                                               $"\n&3Stage&r: &6{currentStage}&r" +
                                               $"\n&3Token&r: &6{currentToken?.GetType().Name ?? "null"}&r" +
                                               $"\n&3Token Count&r: &6{list.Count}&r");

            // Handle string tokens inside collections
            if (isTokenStart)
            {
                ApiLog.Debug("Command Token Parser", $"IsTokenStart is true");

                // Collection tokens
                if (currentToken is CollectionToken stringCollectionToken)
                {
                    ApiLog.Debug("Command Token Parser", $"Current Token is Collection");

                    if (stringCollectionToken.Value is null)
                        stringCollectionToken.Value = string.Empty;

                    stringCollectionToken.Value += currentChar;

                    ApiLog.Debug("Command Token Parser", $"Appended to collection");
                    continue;
                }

                // Dictionary tokens
                if (currentToken is DictionaryToken stringDictionaryToken)
                {
                    // Keys
                    if (currentStage is TokenParserStage.DictionaryKey)
                    {
                        ApiLog.Debug("Command Token Parser", $"Current Stage is DictionaryKey");

                        if (stringDictionaryToken.CurKey is null)
                            stringDictionaryToken.CurKey = string.Empty;

                        stringDictionaryToken.CurKey += currentChar;

                        ApiLog.Debug("Command Token Parser", $"Appended to dictionary key");
                        continue;
                    }

                    // Values
                    if (currentStage is TokenParserStage.DictionaryValue)
                    {
                        ApiLog.Debug("Command Token Parser", $"Current Stage is DictionaryValue");

                        if (stringDictionaryToken.CurValue is null)
                            stringDictionaryToken.CurValue = string.Empty;

                        stringDictionaryToken.CurValue += currentChar;

                        ApiLog.Debug("Command Token Parser", $"Appended to dictionary value");
                        continue;
                    }
                }
            }

            // Handle string
            if (parameterCount > list.Count && currentChar == StringToken.Token && previousChar != EscapeToken)
            {
                ApiLog.Debug("Command Token Parser", $"Current Char is StringToken");

                // End token if already active
                if (currentStage is TokenParserStage.StringFull)
                {
                    ApiLog.Debug("Command Token Parser", $"Ending current string token");
                    
                    if (currentToken != null)
                        list.Add(currentToken);

                    currentToken = null;
                    currentStage = TokenParserStage.None;
                    
                    continue;
                }

                // Handle collections
                if (currentStage is TokenParserStage.Collection
                    or TokenParserStage.DictionaryKey
                    or TokenParserStage.DictionaryValue)
                {
                    ApiLog.Debug("Command Token Parser", $"Starting IsTokenStart {currentStage}");

                    // End the collection token
                    if (isTokenStart)
                    {
                        ApiLog.Debug("Command Token Parser", $"Disabled IsTokenStart");

                        isTokenStart = false;
                        continue;
                    }

                    ApiLog.Debug("Command Token Parser", $"Enabled IsTokenStart");

                    // Start the collection token
                    isTokenStart = true;
                    continue;
                }

                StringToken stringToken = null;

                if (currentToken != null)
                {
                    if (currentToken is not StringToken currentStringToken)
                    {
                        list.Add(currentToken);

                        stringToken = StringToken.Instance.NewToken<StringToken>();
                    }
                    else
                    {
                        stringToken = currentStringToken;
                    }
                }
                else
                {
                    stringToken = StringToken.Instance.NewToken<StringToken>();
                }

                currentToken = stringToken;
                currentStage = TokenParserStage.StringFull;

                ApiLog.Debug("Command Token Parser", $"Started StringToken");
                continue;
            }
            
            if (parameterCount > list.Count && currentStage is TokenParserStage.StringFull && currentToken is StringToken stringFullToken)
            {
                stringFullToken.Value += currentChar;
                continue;
            }

            // Handle the custom property token ($)
            if (currentChar == PropertyToken.StartToken && previousChar != EscapeToken
                                                   && nextChar == PropertyToken.BracketStartToken)
            {
                ApiLog.Debug("Command Token Parser", $"Starting property token");
                
                if (currentToken != null)
                    list.Add(currentToken);

                currentStage = TokenParserStage.PropertyKey;
                currentToken = PropertyToken.Instance.NewToken();

                continue;
            }

            // Switch property
            if (currentChar == PropertyToken.SplitToken
                && previousChar != EscapeToken
                && currentStage is TokenParserStage.PropertyKey)
            {
                ApiLog.Debug("Command Token Parser",
                    $"Switching to property name (key: {(currentToken as PropertyToken)?.Key ?? "null"}");

                currentStage = TokenParserStage.PropertyName;
                continue;
            }

            // Handle properties
            if (currentToken is PropertyToken propertyToken)
            {
                ApiLog.Debug("Command Token Parser", $"Handling property token");

                if (currentStage is TokenParserStage.PropertyKey)
                {
                    ApiLog.Debug("Command Token Parser", $"Stage is PropertyKey");

                    if (propertyToken.Key is null)
                        propertyToken.Key = string.Empty;

                    propertyToken.Key += currentChar;

                    ApiLog.Debug("Command Token Parser", $"Appended to PropertyKey");
                    continue;
                }

                if (currentStage is TokenParserStage.PropertyName)
                {
                    ApiLog.Debug("Command Token Parser", $"Stage is PropertyName");

                    if (propertyToken.Name is null)
                        propertyToken.Name = string.Empty;

                    propertyToken.Name += currentChar;

                    ApiLog.Debug("Command Token Parser", $"Appended to PropertyName");
                    continue;
                }
            }

            if (currentChar == DictionaryToken.StartToken
                && previousChar != EscapeToken
                && previousChar != PropertyToken.StartToken)
            {
                ApiLog.Debug("Command Token Parser", $"Handling dictionary start token");

                if (currentToken != null)
                    list.Add(currentToken);

                currentStage = TokenParserStage.DictionaryKey;
                currentToken = DictionaryToken.Instance.NewToken();

                continue;
            }

            if ((currentChar == DictionaryToken.EndToken || currentChar == PropertyToken.BracketEndToken) 
                && previousChar != EscapeToken)
            {
                ApiLog.Debug("Command Token Parser", $"Ending dictionary / property token");
                
                if (currentToken != null)
                    list.Add(currentToken);
                
                continue;
            }

            // Handle the start of a collection ([)
            if (currentChar == CollectionToken.StartToken && previousChar != EscapeToken)
            {
                ApiLog.Debug("Command Token Parser", $"Starting collection token");

                if (currentToken != null)
                    list.Add(currentToken);

                currentStage = TokenParserStage.Collection;
                currentToken = CollectionToken.Instance.NewToken();

                continue;
            }

            // End the collection token if we detect a collection end token (])
            if (currentChar == CollectionToken.EndToken
                && previousChar != EscapeToken
                && currentStage is TokenParserStage.Collection)
            {
                ApiLog.Debug("Command Token Parser", $"Ending collection token");

                if (currentToken != null)
                    list.Add(currentToken);
                
                continue;
            }

            // Handle dictionary keys ({key = value, key = value, key = value})
            if (currentStage is TokenParserStage.DictionaryKey && currentToken is DictionaryToken dictionaryToken)
            {
                ApiLog.Debug("Command Token Parser", $"Handling dictionary");

                if (dictionaryToken.CurKey is null)
                {
                    if (char.IsWhiteSpace(currentChar))
                    {
                        ApiLog.Debug("Command Token Parser", $"Current char is whitespace");
                        continue;
                    }

                    dictionaryToken.CurKey = string.Empty;
                }

                ApiLog.Debug("Command Token Parser", $"Appended to dictionary key");

                dictionaryToken.CurKey += currentChar;

                // Handle dictionary splitter
                if (nextChar == DictionaryToken.SplitToken)
                {
                    ApiLog.Debug("Command Token Parser", $"Next char is splitter");
                    currentStage = TokenParserStage.DictionaryValue;
                }

                continue;
            }

            // Handle dictionary values ([key: value, key: value, key: value])
            if (currentStage is TokenParserStage.DictionaryValue &&
                currentToken is DictionaryToken valueDictionaryToken)
            {
                // Handle dictionary splitter
                if (currentChar == DictionaryToken.SplitToken && previousChar != EscapeToken)
                {
                    ApiLog.Debug("Command Token Parser", $"Current char is splitter");
                    continue;
                }

                // Handle next dictionary pair
                if (currentChar == CollectionToken.SplitToken && previousChar != EscapeToken)
                {
                    ApiLog.Debug("Command Token Parser", $"Current char is collection splitter");

                    if (string.IsNullOrWhiteSpace(valueDictionaryToken.CurKey))
                        return new(input, "Dictionary keys cannot be white-spaced or empty!",
                            currentChar, i, currentStage);

                    if (string.IsNullOrWhiteSpace(valueDictionaryToken.CurValue))
                        return new(input, "Dictionary values cannot be white-spaced or empty!",
                            currentChar, i, currentStage);

                    valueDictionaryToken.Values.Add(valueDictionaryToken.CurKey, valueDictionaryToken.CurValue);

                    valueDictionaryToken.CurValue = null;
                    valueDictionaryToken.CurKey = null;

                    currentStage = TokenParserStage.DictionaryKey;

                    ApiLog.Debug("Command Token Parser", $"Set stage to DictionaryKey");
                    continue;
                }

                // Prevent leading whitespace
                if (char.IsWhiteSpace(currentChar) && valueDictionaryToken.CurValue is null)
                    continue;

                if (valueDictionaryToken.CurValue is null)
                    valueDictionaryToken.CurValue = string.Empty;

                valueDictionaryToken.CurValue += currentChar;

                ApiLog.Debug("Command Token Parser", $"Appended to dictionary value");
                continue;
            }

            // Handle collection items
            if (currentStage is TokenParserStage.Collection && currentToken is CollectionToken collectionToken)
            {
                ApiLog.Debug("Command Token Parser", $"Handling collection");

                // Handle collection item splitting
                if (currentChar == CollectionToken.SplitToken && previousChar != EscapeToken && collectionToken.Value != null)
                {
                    ApiLog.Debug("Command Token Parser",
                        $"Current char is collection splitter (value: {collectionToken.Value})");

                    collectionToken.Values.Add(collectionToken.Value);
                    collectionToken.Value = null;
                }
                else
                {
                    // Prevent a leading whitespace
                    if (char.IsWhiteSpace(currentChar) && collectionToken.Value is null)
                        continue;

                    if (collectionToken.Value is null)
                        collectionToken.Value = string.Empty;

                    collectionToken.Value += currentChar;

                    ApiLog.Debug("Command Token Parser", $"Appended to collection value");
                    continue;
                }
            }

            // No cases matched, just append
            // TODO: For some odd reason the second string only has a trailing whitespace
            if ((!char.IsWhiteSpace(currentChar) ||
                currentStage is TokenParserStage.String) && parameterCount > list.Count) // Prevent a leading whitespace
            {
                ApiLog.Debug("Command Token Parser", $"Appending current char to string");

                StringToken stringToken = null;

                if (currentToken != null)
                {
                    if (currentToken is not StringToken currentStringToken)
                    {
                        list.Add(currentToken);

                        stringToken = StringToken.Instance.NewToken<StringToken>();
                    }
                    else
                    {
                        stringToken = currentStringToken;
                    }
                }
                else
                {
                    stringToken = StringToken.Instance.NewToken<StringToken>();
                }

                currentToken = stringToken;
                currentStage = TokenParserStage.String;
                
                stringToken.Value += currentChar;
                
                if (nextChar.HasValue && char.IsWhiteSpace(nextChar.Value))
                {
                    list.Add(stringToken);

                    currentToken = null;
                    currentStage = TokenParserStage.None;
                }
                
                // End the current token if the next char is null (we're at the end)
                if (!nextChar.HasValue)
                {
                    ApiLog.Debug("Command Token Parser", $"End reached");

                    if (currentToken != null)
                        list.Add(currentToken);
                }
                
                continue;
            }
            if (list.Count > 0 && parameterCount == list.Count && list[list.Count - 1] is StringToken previousToken)
            {
                if (isAppended)
                    previousToken.Value += currentChar;
                
                isAppended = true;
            }

            // End the current token if the next char is null (we're at the end)
            if (!nextChar.HasValue)
            {
                ApiLog.Debug("Command Token Parser", $"End reached");
                
                if (currentToken != null)
                    list.Add(currentToken);
            }
        }

        return new(input);
    }
}