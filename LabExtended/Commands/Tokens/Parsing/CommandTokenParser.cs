using LabExtended.Commands.Enums;
using LabExtended.Commands.Interfaces;

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
    public static CommandTokenParserResult ParseTokens(string input, List<ICommandToken> list)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentNullException(nameof(input));
        
        if (list is null)
            throw new ArgumentNullException(nameof(list));
        
        var currentToken = default(ICommandToken);
        var currentStage = TokenParserStage.None;

        var previousChar = default(char?);
        var nextChar = default(char?);
        
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

            // A helper function used to append to the current string token or start a new one
            void AppendToStringToken(bool endCurrentToken, bool endCurrentStage)
            {
                if (currentToken is not StringToken stringToken)
                    stringToken = (StringToken)(currentToken = StringToken.Instance.NewToken());
                
                stringToken.Value += currentChar;
                
                if (endCurrentToken)
                    EndCurrentToken(endCurrentStage);
            }

            // A helper function used to end the current stage (and add the current token to the list if present)
            void EndCurrentToken(bool endStage)
            {
                if (currentToken != null)
                    list.Add(currentToken);

                currentToken = null;
                
                if (endStage)
                    currentStage = TokenParserStage.None;
            }

            // Handle string
            if (currentChar == StringToken.Token && previousChar != EscapeToken)
            {
                // End token if already active
                if (currentStage is TokenParserStage.StringFull)
                {
                    AppendToStringToken(true, true);
                    continue;
                }
                
                // Append or start a new token
                AppendToStringToken(false, false);
                
                // Start the StringFull stage
                currentStage = TokenParserStage.StringFull;
                continue;
            }

            // Handle the custom property token ($)
            if (currentChar == PropertyToken.Token && previousChar != EscapeToken)
            {
                // End the token if we finished the name.
                if (currentStage is TokenParserStage.PropertyName)
                {
                    EndCurrentToken(true);
                    continue;
                }
                
                EndCurrentToken(false);

                // Set the current stage & token to property
                currentStage = TokenParserStage.PropertyKey;
                currentToken = PropertyToken.Instance.NewToken();

                continue;
            }

            switch (currentChar)
            {
                // Handle property splitter
                case '.' when currentStage is TokenParserStage.PropertyKey:
                    // Switch to property name
                    currentStage = TokenParserStage.PropertyName;
                    continue;
            }

            // Handle property name / key.
            if (currentToken is PropertyToken propertyToken)
            {
                switch (currentStage)
                {
                    // Append to key if the current stage is the key
                    case TokenParserStage.PropertyKey:
                        propertyToken.Key += currentChar;
                        continue;
                    
                    // Append to name if the current stage is the name
                    case TokenParserStage.PropertyName:
                        propertyToken.Name += currentChar;
                        continue;
                }
            }

            // Handle the start of a collection ([)
            if (currentChar == CollectionToken.StartToken && previousChar != EscapeToken)
            {
                if (currentStage is TokenParserStage.Collection or TokenParserStage.DictionaryKey or TokenParserStage.DictionaryValue)
                    return new(input, "Found another collection start token while already in a collection!",
                        currentChar, i, currentStage);

                // Handle a dictionary start ([{)
                if (nextChar == DictionaryToken.StartToken)
                {
                    EndCurrentToken(true);

                    currentStage = TokenParserStage.DictionaryKey;
                    currentToken = DictionaryToken.Instance.NewToken();
                    
                    continue;
                }
                
                // Handle a collection start
                EndCurrentToken(false);
                
                currentStage = TokenParserStage.Collection;
                currentToken = CollectionToken.Instance.NewToken();
                
                continue;
            }

            // End the collection token if we detect a collection end token (] or }])
            if (currentChar == CollectionToken.EndToken 
                && previousChar != EscapeToken
                && (currentStage is TokenParserStage.Collection || previousChar == DictionaryToken.EndToken))
            {
                EndCurrentToken(true);
                continue;
            }

            // Handle dictionary keys ([key = value, key = value, key = value])
            if (currentStage is TokenParserStage.DictionaryKey && currentToken is DictionaryToken dictionaryToken)
            {
                // Handle dictionary splitter
                if (nextChar == '=')
                {
                    currentStage = TokenParserStage.DictionaryValue;
                    continue;
                }

                if (dictionaryToken.CurKey is null)
                    dictionaryToken.CurKey = string.Empty;
                
                dictionaryToken.CurKey += currentChar;
                continue;
            }

            // Handle dictionary values ([key = value, key = value, key = value])
            if (currentStage is TokenParserStage.DictionaryValue && currentToken is DictionaryToken valueDictionaryToken)
            {
                // Handle dictionary splitter
                if (currentChar == '=' && previousChar != EscapeToken)
                    continue;

                // Handle next dictionary pair
                if (currentChar is ',' && previousChar != EscapeToken)
                {
                    if (string.IsNullOrWhiteSpace(valueDictionaryToken.CurKey))
                        return new(input, "Dictionary keys cannot be white-spaced or empty!", 
                            currentChar, i, currentStage);
                    
                    if (string.IsNullOrWhiteSpace(valueDictionaryToken.CurValue))
                        return new(input, "Dictionary values cannot be white-spaced or empty!", 
                            currentChar, i, currentStage);
                    
                    valueDictionaryToken.Values[valueDictionaryToken.CurKey] = valueDictionaryToken.CurValue;
                    
                    valueDictionaryToken.CurValue = null;
                    valueDictionaryToken.CurKey = null;

                    currentStage = TokenParserStage.DictionaryKey;
                    continue;
                }
                
                // Prevent leading whitespace
                if (char.IsWhiteSpace(currentChar) && valueDictionaryToken.CurValue is null)
                    continue;
                
                if (valueDictionaryToken.CurValue is null)
                    valueDictionaryToken.CurValue = string.Empty;
                
                valueDictionaryToken.CurValue += currentChar;
                continue;
            }

            // Handle collection items
            if (currentStage is TokenParserStage.Collection && currentToken is CollectionToken collectionToken)
            {
                // Handle collection item splitting
                if (currentChar is ',' && previousChar != EscapeToken && collectionToken.Value != null)
                {
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
                    continue;
                }
            }
            
            // No cases matched, just append
            if (!char.IsWhiteSpace(currentChar) || currentStage is TokenParserStage.String) // Prevent a leading whitespace
            {
                AppendToStringToken(false, false);

                currentStage = TokenParserStage.String;
            }

            // End the current token if the next char is null (we're at the end)
            if (!nextChar.HasValue)
                EndCurrentToken(true);
        }

        return new(input);
    }
}