using LabExtended.API.CustomItems;

namespace LabExtended.API.CustomUsables;

/// <summary>
/// Builds Custom Usable item configuration.
/// </summary>
public class CustomUsableBuilder : CustomItemBuilder
{
    private CustomUsableData usableData = new();

    /// <inheritdoc cref="CustomItemBuilder.Data"/>
    public override CustomItemData Data => usableData;

    /// <summary>
    /// Sets the item's use cooldown.
    /// </summary>
    /// <param name="cooldown">The item's cooldown.</param>
    /// <returns>This builder instance.</returns>
    public CustomUsableBuilder WithCooldown(float cooldown)
    {
        usableData.Cooldown = cooldown;
        return this;
    }

    /// <summary>
    /// Sets the item's use time.
    /// </summary>
    /// <param name="useTime">The item's use time.</param>
    /// <returns>This builder instance.</returns>
    public CustomUsableBuilder WithUseTime(float useTime)
    {
        usableData.UseTime = useTime;
        return this;
    }
}