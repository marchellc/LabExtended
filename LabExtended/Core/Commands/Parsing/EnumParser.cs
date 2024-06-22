using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.Core.Commands.Parsing
{
    public class EnumParser : ICommandParser
    {
        public string Name => $"A fixed set of selectable values ({EnumType.Name})";
        public string Description => $"A fixed set of selectable values ({EnumType.Name}) (you can specify the value's name or numeric ID, use 'formatting {EnumType.Name}' to see a list of valid values).";

        public Type EnumType { get; }
        public Type UnderlyingType { get; }

        public EnumParser(Type enumType)
        {
            EnumType = enumType;
            UnderlyingType = Enum.GetUnderlyingType(enumType);
        }

        public bool TryParse(string value, out string failureMessage, out object result)
        {
            ExLoader.Debug("Enum Parser", $"value={value}");

            if (string.IsNullOrWhiteSpace(value))
            {
                result = null;
                failureMessage = $"String cannot be empty or white-spaced.";
                return false;
            }

            if (int.TryParse(value, out var numeric))
            {
                ExLoader.Debug("Enum Parser", $"Parsed numeric: {numeric}");

                if (Enum.IsDefined(EnumType, numeric))
                {
                    ExLoader.Debug("Enum Parser", $"Numeric is defined");

                    result = Convert.ChangeType(numeric, EnumType);
                    failureMessage = null;

                    return true;
                }
            }

            try
            {
                result = Enum.Parse(EnumType, value, true);
            }
            catch (Exception ex)
            {
                failureMessage = $"Failed to parse enum due to an error:\n{ex}";
                result = null;

                return false;
            }

            ExLoader.Debug("Enum Parser", $"Parsed result: {result}");

            failureMessage = null;
            return true;
        }
    }
}
