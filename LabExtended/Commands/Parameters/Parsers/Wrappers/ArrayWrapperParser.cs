namespace LabExtended.Commands.Parameters.Parsers.Wrappers;

/// <summary>
/// A <see cref="Array"/> wrapper parser.
/// </summary>
public class ArrayWrapperParser : CollectionWrapperParserBase
{
    /// <inheritdoc cref="CollectionWrapperParserBase(LabExtended.Commands.Parameters.CommandParameterParser)"/>
    public ArrayWrapperParser(CommandParameterParser parser, Type collectionType) : base(parser) => CollectionType = collectionType;
    
    /// <summary>
    /// Gets the type of the collection.
    /// </summary>
    public Type CollectionType { get; }

    /// <inheritdoc cref="CollectionWrapperParserBase.CreateCollection"/>
    public override object CreateCollection(int collectionSize)
        => Array.CreateInstance(CollectionType, collectionSize);

    /// <inheritdoc cref="CollectionWrapperParserBase.AddToCollection"/>
    public override void AddToCollection(object collection, object element, ref object state)
    {
        if (collection is not Array array)
            throw new Exception($"Invalid collection type: {collection.GetType().FullName}");

        var index = 0;

        if (state is null)
            state = index;
        else
            index = (int)state;

        array.SetValue(element, index++);
        state = index;
    }
}