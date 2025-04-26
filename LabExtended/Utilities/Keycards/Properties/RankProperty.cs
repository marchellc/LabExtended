using InventorySystem.Items.Keycards;

using Mirror;

using UnityEngine;

namespace LabExtended.Utilities.Keycards.Properties;

/// <summary>
/// A keycard rank property.
/// </summary>
public class RankProperty : KeycardValue
{
    /// <summary>
    /// Creates a new <see cref="RankProperty"/> instance.
    /// </summary>
    public RankProperty() : base(typeof(CustomRankDetail)) { }

    /// <summary>
    /// Gets or sets the custom rank index.
    /// </summary>
    public int RankIndex { get; set; }

    /// <inheritdoc cref="KeycardValue.Write"/>
    public override void Write(NetworkWriter writer, KeycardItem item)
    {
        if (!item.TryGetDetail<CustomRankDetail>(out var detail))
            return;
        
        writer.WriteByte((byte)(Mathf.Abs(CustomRankDetail._index) % detail._options.Length));
    }

    /// <inheritdoc cref="KeycardValue.Reset"/>
    public override void Reset()
        => RankIndex = 0;
}