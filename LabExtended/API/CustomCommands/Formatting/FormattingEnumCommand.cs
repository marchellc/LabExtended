using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.Extensions;

using System.Text;

namespace LabExtended.API.CustomCommands.Formatting
{
    public class FormattingEnumCommand : CommandInfo
    {
        public static List<Type> EnumTypes { get; } = new List<Type>();

        public override string Command => "enum";
        public override string Description => "Displays all values of an enum.";

        public object OnCalled(ExPlayer sender, string enumName)
        {
            if (!EnumTypes.TryGetFirst<Type>(t => t.Name.ToLower() == enumName.ToLower(), out var foundEnum))
                return $"Unknown enum type: {enumName}";

            var values = Enum.GetValues(foundEnum);
            var type = Enum.GetUnderlyingType(foundEnum);
            var builder = new StringBuilder();

            builder.AppendLine($"Values of enum '{foundEnum.Name}':");

            foreach (Enum value in values)
                builder.AppendLine($"[{Convert.ChangeType(value, type)}]: {value}");

            return builder.ToString();
        }
    }
}