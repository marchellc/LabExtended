using LabExtended.Extensions;
using LabExtended.API;

using System.Collections;

namespace LabExtended.Commands.Parsing
{
    public class ListParser : Interfaces.ICommandParser
    {
        public string Name => $"A list of items ({ElementParser.Name})";
        public string Description => $"A list of items (string separated by a comma (item1,item2,item3))";
        
        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }

        public Interfaces.ICommandParser ElementParser { get; }

        public Type ElementType { get; }
        public Type GenericType { get; }

        internal ListParser(Interfaces.ICommandParser elementParser, Type elementType)
        {
            ElementParser = elementParser;
            ElementType = elementType;

            GenericType = typeof(List<>).MakeGenericType(ElementType);
        }

        public bool TryParse(ExPlayer sender, string value, out string failureMessage, out object result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = null;
                failureMessage = $"String cannot be empty or white-spaced.";
                return false;
            }

            var values = value.Split(',');
            var list = GenericType.Construct<IList>();

            for (int i = 0; i < values.Length; i++)
            {
                try
                {
                    if (!CommandPropertyParser.TryParse(sender, ElementParser, values[i], out var elementResult)
                        && !ElementParser.TryParse(sender, values[i], out var elementFailureMessage, out elementResult))
                    {
                        failureMessage = $"Internal parser failed to parse element at index {i}: {elementFailureMessage}";
                        result = null;

                        return false;
                    }

                    list.Add(elementResult);
                }
                catch (Exception ex)
                {
                    failureMessage = $"Internal parser caught an error while trying to parse element at index {i}:\n{ex}";
                    result = null;
                    return false;
                }
            }

            failureMessage = null;
            result = list;
            return true;
        }
    }
}
