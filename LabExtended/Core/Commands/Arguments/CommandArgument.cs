using Common.Extensions;

using LabExtended.Core.Commands.Interfaces;
using LabExtended.Core.Commands.Parsing;

using System.Reflection;

namespace LabExtended.Core.Commands.Arguments
{
    public class CommandArgument : ICommandArgument
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Type Type { get; set; }

        public bool IsOptional { get; set; }
        public bool IsRemainder { get; set; }

        public object DefaultValue { get; set; }

        public ICommandParser Parser { get; set; }

        public ParameterInfo Parameter { get; set; }

        public CommandArgument(string name, string description, Type type, object defaultValue = null, ICommandParser parser = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (type is null)
                throw new ArgumentNullException(nameof(type));

            Name = name;
            Type = type;
            DefaultValue = defaultValue;

            if (parser is null)
                TryFindParser();
            else
                Parser = parser;

            if (string.IsNullOrWhiteSpace(description))
                Description = Parser.Description;
            else
                Description = description;
        }

        internal void TryFindParser()
        {
            if (CommandUtils.TryGetParser(Type, out var parser))
            {
                Parser = parser;
                return;
            }

            if (Type.IsEnum)
            {
                Parser = new EnumParser(Type);
                return;
            }

            var underlyingNullable = Nullable.GetUnderlyingType(Type);

            if (underlyingNullable != null)
            {
                IsOptional = true;
                Type = underlyingNullable;
            }

            if (Type.GetTypeInfo().IsGenericType)
            {
                var definition = Type.GetGenericTypeDefinition();

                if (definition == typeof(List<>))
                {
                    var elementType = Type.GetFirstGenericType();

                    if (elementType.IsEnum)
                    {
                        Parser = new ListParser(new EnumParser(elementType), elementType);
                        return;
                    }

                    if (elementType is null || !CommandUtils.Parsers.TryGetValue(elementType, out var elementParser))
                        throw new Exception($"Missing parser for list element type: {elementType?.FullName ?? "null"} ({Type.FullName})");

                    Parser = new ListParser(elementParser, elementType);
                    return;
                }

                if (definition == typeof(Dictionary<,>))
                {
                    var genericArgs = Type.GetGenericArguments();

                    var keyParser = default(ICommandParser);
                    var valueParser = default(ICommandParser);

                    if (genericArgs[0].IsEnum)
                        keyParser = new EnumParser(genericArgs[0]);
                    else if (!CommandUtils.TryGetParser(genericArgs[0], out keyParser))
                        throw new Exception($"Missing parser for dictionary key type: {genericArgs[0].FullName} ({Type.FullName})");

                    if (genericArgs[1].IsEnum)
                        valueParser = new EnumParser(genericArgs[1]);
                    else if (!CommandUtils.TryGetParser(genericArgs[1], out valueParser))
                        throw new Exception($"Missing parser for dictionary value type: {genericArgs[1].FullName} ({Type.FullName})");

                    Parser = new DictionaryParser(keyParser, valueParser, genericArgs[0], genericArgs[1]);
                    return;
                }
            }

            throw new Exception($"Missing parser for argument type: {Type.FullName}");
        }
    }
}