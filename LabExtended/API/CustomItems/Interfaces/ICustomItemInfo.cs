using LabExtended.API.CustomItems.Info;

namespace LabExtended.API.CustomItems.Interfaces
{
    public interface ICustomItemInfo
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }

        ItemType InventoryType { get; }
        CustomItemFlags ItemFlags { get; }

        CustomItemPickupInfo PickupInfo { get; }

        Type Type { get; }
    }
}