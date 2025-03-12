using LabExtended.API.CustomItems;

namespace LabExtended.API.CustomFirearms;

/// <summary>
/// Configuration builder for Custom Firearms.
/// </summary>
public class CustomFirearmBuilder : CustomItemBuilder
{
    private CustomFirearmData firearmData;

    public override CustomItemData Data => firearmData;
}