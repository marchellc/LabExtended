using HarmonyLib;

using LabExtended.Utilities;

namespace LabExtended.Commands.Parameters.Parsers.Wrappers;

/// <summary>
/// A <see cref="HashSet{T}"/> wrapper parser.
/// </summary>
public class HashSetWrapperParser : CollectionWrapperParserBase
{
    /// <inheritdoc cref="CollectionWrapperParserBase(LabExtended.Commands.Parameters.CommandParameterParser)"/>
    public HashSetWrapperParser(CommandParameterParser parser, Type collectionType) : base(parser)
    {
        CollectionType = collectionType;
        CollectionAddMethod = FastReflection.ForMethod(AccessTools.Method(collectionType, "Add"));
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
    
    /// <summary>
    /// Gets the Add method for the collection.
    /// </summary>
    public Func<object, object[], object> CollectionAddMethod { get; }

    /// <inheritdoc cref="CollectionWrapperParserBase.CreateCollection"/>
    public override object CreateCollection(int collectionSize)
        => CollectionConstructor([collectionSize]);

    /// <inheritdoc cref="CollectionWrapperParserBase.AddToCollection"/>
    public override void AddToCollection(object collection, object element, ref object state)
    {
        if (collection.GetType() != CollectionType)
            throw new Exception($"Invalid collection type: {collection.GetType().FullName}");

        CollectionAddMethod(collection, [element]);
    }
}