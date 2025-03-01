using LabExtended.Commands.Interfaces;

using System.ComponentModel;
using System.Reflection;

namespace LabExtended.Commands.Arguments
{
    public class ArgumentDefinition
    {
        internal List<ICommandValidator> _validators = new();
        internal ParameterInfo _bindParameter;

        public Type Type { get; private set; }

        public string Name { get; private set; }
        public string Description { get; private set; } = "No description.";

        public object Default { get; private set; }

        public ICommandParser Parser { get; private set; }
        public IReadOnlyList<ICommandValidator> Validators => _validators;

        public bool IsOptional { get; private set; }

        public ArgumentDefinition() { }
        public ArgumentDefinition(Type type, string name, string description = "No description", ICommandParser parser = null, object defaultValue = null, bool isOptional = false)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            Type = type;
            Name = name;

            Default = defaultValue;
            Description = description;

            IsOptional = isOptional;

            Parser = parser ?? CommandParser.FromType(type);
        }

        public ArgumentDefinition WithType(Type type, ICommandParser parser = null)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            Type = type;

            if (parser != null)
                Parser = parser;
            else if (Parser is null)
                Parser = CommandParser.FromType(type);

            return this;
        }

        public ArgumentDefinition WithType<T>(ICommandParser parser = null)
            => WithType(typeof(T), parser);

        public ArgumentDefinition WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            return this;
        }

        public ArgumentDefinition WithDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));

            Description = description;
            return this;
        }

        public ArgumentDefinition WithDefaultValue(object defaultValue)
        {
            Default = defaultValue;
            IsOptional = true;
            return this;
        }

        public ArgumentDefinition WithValidator(ICommandValidator validator)
        {
            if (validator is null)
                throw new ArgumentNullException(nameof(validator));

            if (_validators.Contains(validator))
                throw new Exception($"This validator has already been added.");

            _validators.Add(validator);
            return this;
        }

        public void ValidateArgument()
        {
            Description ??= "No description.";

            if (string.IsNullOrWhiteSpace(Name))
                throw new Exception($"Argument's name cannot be empty.");

            if (Type is null)
                throw new Exception($"Argument's type cannot be null.");

            if (Parser is null)
                throw new Exception($"Argument's parser cannot be null.");
        }

        public static ArgumentDefinition FromType<T>(string name, string description = "No description", T defaultValue = default, bool isOptional = false, ICommandParser parser = null)
            => new ArgumentDefinition(typeof(T), name, description, parser, defaultValue, isOptional);

        public static ArgumentDefinition FromParameter(ParameterInfo parameter)
        {
            var definition = new ArgumentDefinition() { _bindParameter = parameter };

            var descAttribute = parameter.GetCustomAttribute<DescriptionAttribute>();

            definition.WithName(parameter.Name);
            definition.WithType(parameter.ParameterType);

            if (descAttribute != null && !string.IsNullOrWhiteSpace(descAttribute.Description))
                definition.WithDescription(descAttribute.Description);

            if (parameter.IsOptional)
                definition.WithDefaultValue(parameter.DefaultValue);

            return definition;
        }
    }
}