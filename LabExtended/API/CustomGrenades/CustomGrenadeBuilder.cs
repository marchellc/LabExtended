using LabExtended.API.CustomItems;

namespace LabExtended.API.CustomGrenades;

/// <summary>
/// Used to configure Custom Grenade data.
/// </summary>
public class CustomGrenadeBuilder : CustomItemBuilder
{
    private CustomGrenadeData grenadeData = new();

    /// <inheritdoc cref="CustomItemBuilder.Data"/>
    public override CustomItemData Data => grenadeData;
}