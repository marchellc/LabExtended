using LabExtended.Commands;
using LabExtended.Commands.Arguments;



using LabExtended.Extensions;

using System.Text;

namespace LabExtended.API.CustomCommands.Formatting
{
    public class FormattingEnumCommand : CustomCommand
    {
        public static List<Type> EnumTypes { get; } = new List<Type>();

        public override string Command => "enum";
        public override string Description => "Displays all values of an enum.";

        public override ArgumentDefinition[] BuildArgs()
        {
            return ArgumentBuilder.Get(x =>
            {
                x.WithArg<string>("Name", "Name of the enum.");
            });
        }

        public override void OnCommand(ExPlayer sender, Commands.Interfaces.ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var enumName = args.Get<string>("Name");

            if (!EnumTypes.TryGetFirst<Type>(t => t.Name.ToLower() == enumName.ToLower(), out var foundEnum))
            {
                ctx.RespondFail($"Unknown enum type: {enumName}");
                return;
            }

            var values = Enum.GetValues(foundEnum);
            var type = Enum.GetUnderlyingType(foundEnum);
            var builder = new StringBuilder();

            builder.AppendLine($"Values of enum '{foundEnum.Name}':");

            foreach (Enum value in values)
                builder.AppendLine($"[{Convert.ChangeType(value, type)}]: {value}");

            ctx.RespondOk(builder);
        }
    }
}