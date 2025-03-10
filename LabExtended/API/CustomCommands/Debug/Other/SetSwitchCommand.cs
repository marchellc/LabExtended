using LabExtended.API.Containers;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

namespace LabExtended.API.CustomCommands.Debug.Other
{
    public class SetSwitchCommand : CustomCommand
    {
        public override string Command => "setswitch";
        public override string Description => "Changes the value of one of your custom switches.";

        public override ArgumentDefinition[] BuildArgs()
        {
            return GetArgs(x =>
            {
                x.WithArg<string>("Name", "Name of the switch.");
                x.WithArg<bool>("Value", "The new value of the switch.");
                x.WithOptional<ExPlayer>("Target", "The player to set the switch on.");
            });
        }

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var name = args.Get<string>("Name");
            var value = args.Get<bool>("Value");
            var target = args.Get<ExPlayer>("Target") ?? sender;

            var prop = typeof(SwitchContainer).FindProperty(x => x.Name.ToLower() == name.ToLower() && x.PropertyType == typeof(bool));

            if (prop is null)
            {
                ctx.RespondFail($"Unknown switch name.\n{string.Join("\n", typeof(SwitchContainer).GetAllProperties().Where(x => x.PropertyType == typeof(bool)).Select(x => $"- {x.Name} ({x.GetValue(target.Toggles)})"))}");
                return;
            }

            prop.SetValue(target.Toggles, value);

            ctx.RespondOk($"Set switch '{prop.Name}' to {value}");
        }
    }
}