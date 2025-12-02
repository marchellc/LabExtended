using System.Reflection;

using HarmonyLib;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Core;
using LabExtended.Extensions;

#pragma warning disable CS8618, CS9264
#pragma warning disable CS8601 // Possible null reference assignment.

namespace LabExtended.Commands.Parameters;

/// <summary>
/// Represents a command parameter.
/// </summary>
public class CommandParameter
{
    private Dictionary<CommandParameterParser, List<PropertyInfo>> cachedProperties = new();

    /// <summary>
    /// Gets the parameter's type.
    /// </summary>
    public CommandParameterType Type { get; }

    /// <summary>
    /// Gets the list of parameter arguments.
    /// </summary>
    public List<ICommandParameterRestriction> Restrictions { get; } = new();

    /// <summary>
    /// Gets a dictionary of parsers associated with this parameter.
    /// </summary>
    public Dictionary<CommandParameterParser, string> Parsers { get; } = new();
    
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    public string Name { get; internal set; }
    
    /// <summary>
    /// Gets the description of the parameter.
    /// </summary>
    public string Description { get; internal set; }
    
    /// <summary>
    /// Gets the <see cref="CommandSystem.IUsageProvider.Usage"/> alias of this parameter.
    /// </summary>
    public string? UsageAlias { get; }

    /// <summary>
    /// Gets the type's friendly alias.
    /// </summary>
    public string? FriendlyAlias { get; }

    /// <summary>
    /// Whether or not this parameter is optional.
    /// </summary>
    public bool HasDefault { get; internal set; }
    
    /// <summary>
    /// Gets the default value.
    /// </summary>
    public object? DefaultValue { get; internal set; }

    /// <summary>
    /// Creates a new <see cref="CommandParameter"/> instance.
    /// </summary>
    /// <param name="parameterInfo">The targeted parameter.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CommandParameter(ParameterInfo parameterInfo)
    {
        if (parameterInfo is null)
            throw new ArgumentNullException(nameof(parameterInfo));

        Type = new(parameterInfo);

        if (Type.Parser != null)
        {
            Parsers[Type.Parser] = string.Empty;

            UsageAlias = Type.Parser.UsageAlias;
            FriendlyAlias = Type.Parser.FriendlyAlias;
        }

        Name = parameterInfo.Name;
        HasDefault = parameterInfo.HasDefaultValue;
        DefaultValue = parameterInfo.DefaultValue;
        Description = "None";

        foreach (var optionsAttribute in parameterInfo.GetCustomAttributes<CommandParameterAttribute>())
        {
            if (optionsAttribute.Restriction != null)
                Restrictions.Add(optionsAttribute.Restriction);

            if (optionsAttribute.Name != null)
                Name = optionsAttribute.Name;

            if (optionsAttribute.Description != null)
                Description = optionsAttribute.Description;

            if (optionsAttribute.UsageAlias != null)
            {
                UsageAlias = optionsAttribute.UsageAlias;
                FriendlyAlias = optionsAttribute.UsageAlias;
            }

            if (optionsAttribute.ParserType != null)
            {
                if (CommandParameterParserUtils.TryGetParser(optionsAttribute.ParserType, out var parserInstance))
                {
                    Parsers[parserInstance] = optionsAttribute.ParserProperty ?? string.Empty;
                    continue;
                }

                if (CommandParameterParserUtils.Parsers.TryGetFirst(p => p.Value.GetType() == optionsAttribute.ParserType, out var parserPair))
                {
                    Parsers[parserPair.Value] = optionsAttribute.ParserProperty ?? string.Empty;
                    continue;
                }

                ApiLog.Error("LabExtended", $"Parser &3{optionsAttribute.ParserType.FullName}&r has not been registered!");
            }
        }

        if (Type.Parser is null && Parsers.Count < 1)
            throw new Exception($"No parsers are defined for type '{Type.Type?.ToString() ?? "(null)"}'");
    }

    /// <summary>
    /// Creates a new <see cref="CommandParameter"/> instance.
    /// </summary>
    /// <param name="type">The parameter type.</param>
    /// <param name="name">Parameter name.</param>
    /// <param name="description">Parameter description.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <param name="hasDefault">Is the parameter optional?</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CommandParameter(Type type, string name, string description, object defaultValue, bool hasDefault)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        Type = new(type);
        
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        
        DefaultValue = defaultValue;
        HasDefault = hasDefault;
    }

    internal CommandParameter()
    {
        Type = new();

        Name = "NULL";
        Description = "NULL";

        HasDefault = false;

        DefaultValue = null;
    }

    /// <summary>
    /// Resolves and returns the value of a property or nested property from the specified parser result, based on the
    /// configuration provided by the given parser.
    /// </summary>
    /// <remarks>This method supports resolving nested properties by interpreting dot-separated property paths
    /// defined by the parser. Property resolution is cached for each parser instance to improve performance on
    /// subsequent calls.</remarks>
    /// <param name="parserResult">The object representing the result from which to resolve the property value. This is typically the output of a
    /// parsing operation.</param>
    /// <param name="parser">The parser that defines which property or nested property to resolve from the parser result.</param>
    /// <returns>The value of the resolved property or nested property from the parser result. If the property cannot be
    /// resolved, returns the original parser result object.</returns>
    public object ResolveValue(object parserResult, CommandParameterParser parser)
    {
        if (this.cachedProperties.TryGetValue(parser, out var cachedProperties))
        {
            foreach (var cachedProperty in cachedProperties)
                parserResult = cachedProperty.GetValue(parserResult);

            return parserResult;
        }

        if (!Parsers.TryGetValue(parser, out var property) || property == string.Empty)
            return parserResult;

        cachedProperties = new();

        var propertyParts = property.Split('.');

        for (var i = 0; i < propertyParts.Length; i++)
        {
            if (parserResult == null)
                throw new Exception($"Cannot resolve property '{propertyParts[i]}' because the parent object is null.");

            var propertyInfo = AccessTools.DeclaredProperty(parserResult.GetType(), propertyParts[i]);

            if (propertyInfo == null)
                propertyInfo = AccessTools.Property(parserResult.GetType(), propertyParts[i]);

            if (propertyInfo == null)
                throw new Exception($"Property '{propertyParts[i]}' not found on type '{parserResult.GetType()}'.");

            cachedProperties.Add(propertyInfo);

            parserResult = propertyInfo.GetValue(parserResult);
        }

        this.cachedProperties[parser] = cachedProperties;
        return parserResult;
    }

    /// <summary>
    /// Whether or not this parameter has the specified restriction.
    /// </summary>
    /// <param name="restriction">The restriction instance.</param>
    /// <typeparam name="T">The restriction type.</typeparam>
    /// <returns>true if the restriction was found</returns>
    public bool HasRestriction<T>(out T restriction) where T : ICommandParameterRestriction
    {
        for (var i = 0; i < Restrictions.Count; i++)
        {
            if (Restrictions[i] is T value)
            {
                restriction = value;
                return true;
            }
        }
        
        restriction = default;
        return false;
    }
}