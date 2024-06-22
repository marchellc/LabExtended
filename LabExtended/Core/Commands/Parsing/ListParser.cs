using Common.Extensions;

using LabExtended.Core.Commands.Interfaces;

using System.Collections;

namespace LabExtended.Core.Commands.Parsing
{
    public class ListParser : ICommandParser
    {
        public string Name => $"A list of items ({ElementParser.Name})";
        public string Description => $"A list of items (string separated by a comma (item1,item2,item3))\nElement type description: {ElementParser.Description}";

        public ICommandParser ElementParser { get; }

        public Type ElementType { get; }
        public Type GenericType { get; }

        internal ListParser(ICommandParser elementParser, Type elementType)
        {
            ElementParser = elementParser;
            ElementType = elementType;

            GenericType = typeof(List<>).MakeGenericType(ElementType);
        }

        public bool TryParse(string value, out string failureMessage, out object result)
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
                    if (!ElementParser.TryParse(values[i], out var elementFailureMessage, out var elementResult))
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
