namespace LabExtended.Interfaces
{
    /// <summary>
    /// Represents properties of an item.
    /// </summary>
    public interface IItemProperties
    {
        /// <summary>
        /// Gets or sets the item's serial.
        /// </summary>
        ushort Serial { get; set; }

        /// <summary>
        /// Gets or sets the item's type.
        /// </summary>
        ItemType Type { get; set; }
    }
}