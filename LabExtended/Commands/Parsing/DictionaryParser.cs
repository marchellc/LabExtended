using LabExtended.Extensions;
using LabExtended.API;

using System.Collections;

namespace LabExtended.Commands.Parsing
{
    public class DictionaryParser : Interfaces.ICommandParser
    {
        public string Name => $"Dictionary (a list of key [{KeyParser.Name}] and value [{ValueParser.Name}] pairs)";
        public string Description => $"A list of key and value pairs (formatting: key=value,key2=value2,key3=value3)";

        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }

        public Interfaces.ICommandParser KeyParser { get; }
        public Interfaces.ICommandParser ValueParser { get; }

        public Type KeyType { get; }
        public Type ValueType { get; }
        public Type GenericType { get; }

        public DictionaryParser(Interfaces.ICommandParser keyParser, Interfaces.ICommandParser valueParser, Type keyType, Type valueType)
        {
            KeyParser = keyParser;
            ValueParser = valueParser;

            KeyType = keyType;
            ValueType = valueType;

            GenericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        }

        public bool TryParse(ExPlayer sender, string value, out string failureMessage, out object result)
        {
            result = null;
            failureMessage = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                failureMessage = $"String cannot be empty or white-spaced.";
                return false;
            }

            var pairs = value.Split(',');
            var dict = GenericType.Construct<IDictionary>();

            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i].Split('=');

                if (pair.Length != 2)
                {
                    failureMessage = $"Pair at index {i} has invalid formatting ({pairs[i]})";
                    return false;
                }

                var key = pair[0].Trim();
                var text = pair[1].Trim();
                
                if (!CommandPropertyParser.TryParse(sender, KeyParser, key, out var keyElement) 
                    && !KeyParser.TryParse(sender, key, out var keyFailureMessage, out keyElement))
                {
                    failureMessage = $"Internal parser failed to parse pair key at index {i} ({pairs[i]}): {keyFailureMessage}";
                    return false;
                }

                if (!CommandPropertyParser.TryParse(sender, ValueParser, text, out var valueElement)
                    && !ValueParser.TryParse(sender, text, out var valueFailureMessage, out valueElement))
                {
                    failureMessage = $"Internal parser failed to parse pair value at index {i} ({pairs[i]}): {valueFailureMessage}";
                    return false;
                }

                dict[keyElement] = valueElement;
            }

            result = dict;
            return true;
        }
    }
}