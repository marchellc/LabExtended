using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Commands.Arguments
{
    public class ArgumentBuilder
    {
        public static ArgumentBuilder New => new ArgumentBuilder();

        private ArgumentDefinition[] _args;
        private List<ArgumentDefinition> _build;

        private ArgumentBuilder()
            => _build = ListPool<ArgumentDefinition>.Shared.Rent();

        public ArgumentBuilder WithArg<T>(string name, ICommandParser parser = null)
        {
            _build.Add(ArgumentDefinition.FromType<T>(name, "No description.", ArgumentFlags.None, default, parser));
            return this;
        }

        public ArgumentBuilder WithArg<T>(string name, string description, ICommandParser parser = null)
        {
            _build.Add(ArgumentDefinition.FromType<T>(name, description, ArgumentFlags.None, default, parser));
            return this;
        }

        public ArgumentBuilder WithOptional<T>(string name, T defaultValue = default, ICommandParser parser = null)
        {
            _build.Add(ArgumentDefinition.FromType(name, "No description.", ArgumentFlags.Optional, defaultValue, parser));
            return this;
        }

        public ArgumentBuilder WithOptional<T>(string name, string description, T defaultValue = default, ICommandParser parser = null)
        {
            _build.Add(ArgumentDefinition.FromType(name, description, ArgumentFlags.Optional, defaultValue, parser));
            return this;
        }

        public ArgumentDefinition[] Build()
        {
            if (_args != null)
                return _args;

            _args = ListPool<ArgumentDefinition>.Shared.ToArrayReturn(_build);
            _build = null;

            return _args;
        }

        public static ArgumentDefinition[] Get(Action<ArgumentBuilder> builder)
        {
            var argBuilder = New;

            builder(argBuilder);
            return argBuilder.Build();
        }
    }
}
