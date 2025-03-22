using System.Collections;

using HarmonyLib;

using LabExtended.Utilities;

namespace LabExtended.Commands.Parameters.Parsers.Wrappers;

/// <summary>
/// A <see cref="List{T}"/> wrapper parser.
/// </summary>
public class ListWrapperParser : CollectionWrapperParserBase
{
    /// <inheritdoc cref="CollectionWrapperParserBase(LabExtended.Commands.Parameters.CommandParameterParser)"/>
    public ListWrapperParser(CommandParameterParser parser, Type collectionType) : base(parser)
    {
        CollectionType = collectionType;
        CollectionConstructor = FastReflection.ForConstructor(AccessTools.Constructor(collectionType, [typeof(int)]));
    }
    
    /// <summary>
    /// Gets the type of the collection.
    /// </summary>
    public Type CollectionType { get; }
    
    /// <summary>
    /// Gets the constructor of the collection.
    /// </summary>
    public Func<object[], object> CollectionConstructor { get; }

    /// <inheritdoc cref="CollectionWrapperParserBase.CreateCollection"/>
    public override object CreateCollection(int collectionSize)
        => CollectionConstructor([collectionSize]);

    /// <inheritdoc cref="CollectionWrapperParserBase.AddToCollection"/>
    public override void AddToCollection(object collection, object element, ref object state)
    {
        if (collection is not IList list)
            throw new Exception($"Invalid collection type: {collection.GetType().FullName}");
        
        list.Add(element);
    }
}