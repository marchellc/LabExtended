using LabExtended.Core.Commands.Interfaces;
using LabExtended.Extensions;
using System.Collections;

namespace LabExtended.Core.Commands.Parsing
{
    public class DictionaryParser : ICommandParser
    {
        public string Name => $"Dictionary (a list of key [{KeyParser.Name}] and value [{ValueParser.Name}] pairs)";
        public string Description => $"A list of key and value pairs (formatting: key=value,key2=value2,key3=value3)\nKey description: {KeyParser.Description}\nValue description: {ValueParser.Description}";

        public ICommandParser KeyParser { get; }
        public ICommandParser ValueParser { get; }

        public Type KeyType { get; }
        public Type ValueType { get; }
        public Type GenericType { get; }

        public DictionaryParser(ICommandParser keyParser, ICommandParser valueParser, Type keyType, Type valueType)
        {
            KeyParser = keyParser;
            ValueParser = valueParser;

            KeyType = keyType;
            ValueType = valueType;

            GenericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        }

        public bool TryParse(string value, out string failureMessage, out object result)
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

                if (!KeyParser.TryParse(pair[0].Trim(), out var keyFailureMessage, out var keyElement))
                {
                    failureMessage = $"Internal parser failed to parse pair key at index {i} ({pairs[i]}): {keyFailureMessage}";
                    return false;
                }

                if (!ValueParser.TryParse(pair[1].Trim(), out var valueFailureMessage, out var valueElement))
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