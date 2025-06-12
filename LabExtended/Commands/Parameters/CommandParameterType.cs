using System.Reflection;

using HarmonyLib;

using LabExtended.Utilities;

#pragma warning disable CS8618, CS9264

namespace LabExtended.Commands.Parameters;

/// <summary>
/// Represents the type of a command parameter.
/// </summary>
public class CommandParameterType
{
    private Type type;
    
    /// <summary>
    /// Gets the type of this parameter.
    /// </summary>
    public Type? Type
    {
        get => type;
        private set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (type is not null && type == value)
                return;

            type = value;

            IsString = value == typeof(string);
            IsArray = value.IsArray;
            IsNullable = (NullableType = Nullable.GetUnderlyingType(value)) != null;

            if (IsArray)
            {
                KeyType = value.GetElementType();
                Constructor = _ => Array.CreateInstance(KeyType, 0);
            }
            else if (value.IsConstructedGenericType && !IsNullable)
            {
                var genericDef = value.GetGenericTypeDefinition();
                var genericArgs = value.GetGenericArguments();

                if (genericArgs.Length == 1)
                {
                    KeyType = genericArgs[0];

                    if (genericDef == typeof(List<>))
                        IsList = true;
                    else if (genericDef == typeof(HashSet<>))
                        IsHashSet = true;
                    else
                        throw new Exception(
                            $"Unsupported generic type definition: {genericDef.FullName} (in {value.FullName})");
                }
                else if (genericArgs.Length == 2)
                {
                    KeyType = genericArgs[0];
                    ValueType = genericArgs[1];

                    if (genericDef == typeof(Dictionary<,>))
                        IsDictionary = true;
                    else
                        throw new Exception(
                            $"Unsupported generic type definition: {genericDef.FullName} (in {value.FullName})");
                }
                else
                {
                    throw new Exception(
                        $"Unsupported generic type definition: {genericDef.FullName} (in {value.FullName})");
                }
                
                var constructor = AccessTools.DeclaredConstructor(value, Type.EmptyTypes);

                if (constructor is null)
                    throw new Exception($"Type {value.FullName} does not have a declared empty constructor.");

                Constructor = FastReflection.ForConstructor(constructor);

                if (Constructor is null)
                    throw new Exception($"Could not generate a constructor for type {value.FullName}");
            }

            if (!CommandParameterParserUtils.TryGetParser(value, out var parser))
                throw new Exception($"No parsers are registered for type: {value.FullName}");

            Parser = parser;
        }
    }

    /// <summary>
    /// Gets the parameter's key type (value type for arrays and collections).
    /// </summary>
    public Type? KeyType { get; private set; }
    
    /// <summary>
    /// Gets the parameter's value type (only for dictionaries).
    /// </summary>
    public Type? ValueType { get; private set; }
    
    /// <summary>
    /// Gets the parameter's underlying nullable type.
    /// </summary>
    public Type? NullableType { get; private set; }

    /// <summary>
    /// Gets the parameter's info.
    /// </summary>
    public ParameterInfo Parameter { get; internal set; }
    
    /// <summary>
    /// Gets the parser for this type.
    /// </summary>
    public CommandParameterParser Parser { get; private set; }
    
    /// <summary>
    /// Whether or not the type is nullable.
    /// </summary>
    public bool IsNullable { get; private set; }
    
    /// <summary>
    /// Whether or not the parameter is a string.
    /// </summary>
    public bool IsString { get; private set; }
    
    /// <summary>
    /// Whether or not the parameter is an array.
    /// </summary>
    public bool IsArray { get; private set; }
    
    /// <summary>
    /// Whether or not the parameter is a <see cref="List{T}"/>.
    /// </summary>
    public bool IsList { get; private set; }
    
    /// <summary>
    /// Whether or not the parameter is a <see cref="HashSet{T}"/>.
    /// </summary>
    public bool IsHashSet { get; private set; }
    
    /// <summary>
    /// Whether or not the parameter is a dictionary.
    /// </summary>
    public bool IsDictionary { get; private set; }
    
    /// <summary>
    /// Gets the type's constructor.
    /// </summary>
    public Func<object[], object>? Constructor { get; private set; }

    /// <summary>
    /// Creates a new <see cref="CommandParameterType"/> instance.
    /// </summary>
    /// <param name="info">The parameter.</param>
    public CommandParameterType(ParameterInfo info)
    {
        Parameter = info ?? throw new ArgumentNullException(nameof(info));
        Type = info.ParameterType;
    }
    
    /// <summary>
    /// Creates a new <see cref="CommandParameterType"/> instance.
    /// </summary>
    /// <param name="type">The parameter's type.</param>
    public CommandParameterType(Type type)
    {
        Type = type;
    }

    internal CommandParameterType()
    {
        Parameter = null;
        type = typeof(object);
    }
}