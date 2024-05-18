namespace LabExtended.API.Interfaces
{
    public interface IItemProperties
    {
        ushort Serial { get; }

        ItemType Type { get; }
    }
}