using LabExtended.Commands.Tokens;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters.Parsers.Wrappers;

/// <summary>
/// A wrapping parser used for <see cref="List{T}"/> collections.
/// </summary>
public abstract class CollectionWrapperParserBase : CommandParameterParser
{
    /// <summary>
    /// Creates a new <see cref="CollectionWrapperParserBase"/> instance.
    /// </summary>
    /// <param name="parser">The element parser.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CollectionWrapperParserBase(CommandParameterParser parser)
    {
        if (parser is null)
            throw new ArgumentNullException(nameof(parser));

        Parser = parser;
    }
    
    /// <summary>
    /// Gets the element parser.
    /// </summary>
    public CommandParameterParser Parser { get; }
    
    /// <summary>
    /// Whether or not the target type is a string list.
    /// </summary>
    public virtual bool IsStringList { get; }

    /// <inheritdoc cref="CommandParameterParser.UsageAlias"/>
    public override string? UsageAlias => Parser.UsageAlias;

    /// <inheritdoc cref="CommandParameterParser.FriendlyAlias"/>
    public override string? FriendlyAlias => Parser.FriendlyAlias;

    /// <inheritdoc cref="CommandParameterParser.AcceptsToken"/>
    public override bool AcceptsToken(ICommandToken token) => token is PropertyToken or CollectionToken;

    /// <summary>
    /// Creates a new instance of targeted collection.
    /// </summary>
    /// <param name="collectionSize">The collection's initial size.</param>
    /// <returns>The collection instance.</returns>
    public abstract object CreateCollection(int collectionSize);

    /// <summary>
    /// Adds a new item to a collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="element">The item.</param>
    /// <param name="state">State of the collection.</param>
    public abstract void AddToCollection(object collection, object element, ref object state);

    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex, CommandContext context,
        CommandParameter parameter)
    {
        if (token.TryProcessProperty(context, out var property))
        {
            if (property.GetType() == parameter.Type.Type)
                return new(true, property, null, parameter);

            return new(false, null,
                $"Property \"{property.GetType().FullName}\" could not be converted to \"{parameter.Type.Type.FullName}\"",
                parameter);
        }
        
        var collectionToken = token as CollectionToken;

        if (IsStringList)
            return new(true, collectionToken.Values, null, parameter);
        
        var collectionState = default(object);
        var collection = CreateCollection(collectionToken.Values.Count);

        var index = 0;
        var stringToken = StringToken.Instance.NewToken<StringToken>();
        
        foreach (var stringElement in collectionToken.Values)
        {
            stringToken.Value = stringElement;
            
            var elementResult = Parser.Parse(tokens, stringToken, -1, context, parameter);

            if (!elementResult.Success)
                return new(false, null, $"Could not parse element at position {index}: {elementResult.Error}", 
                    parameter);
            
            AddToCollection(collection, elementResult.Value, ref collectionState);
        }
        
        stringToken.ReturnToken();
        return new(true, collection, null, parameter);
    }
}