using LabExtended.Commands.Formatting;
using LabExtended.Core.Commands.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Core.Commands.Parsing
{
    public class EnumParser : ICommandParser
    {
        public string Name => $"A fixed set of selectable values ({EnumType.Name})";
        public string Description => $"A fixed set of selectable values ({EnumType.Name}) (you can specify the value's name or numeric ID, use 'formatting enum {EnumType.Name}' to see a list of valid values).";

        public Type EnumType { get; }
        public Type UnderlyingType { get; }

        public EnumParser(Type enumType)
        {
            EnumType = enumType;
            UnderlyingType = Enum.GetUnderlyingType(enumType);

            if (!FormattingEnumCommand.EnumTypes.Contains(enumType))
                FormattingEnumCommand.EnumTypes.Add(enumType);
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

            if (value.TrySplit(',', true, null, out var parts) && parts.Length > 1 && !EnumType.IsBitwiseEnum())
            {
                failureMessage = "This enum does not support bitwise operations (only a singular value can be specified).";
                return false;
            }

            try
            {
                result = Enum.Parse(EnumType, value, true);
                return true;
            }
            catch (Exception ex)
            {
                failureMessage = $"Failed to parse enum due to an error:\n{ex}";
                result = null;

                return false;
            }
        }
    }
}
