using LabApi.Features.Wrappers;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when an item is about to "decay" in the Pocket Dimension.
    /// </summary>
    public class PocketDimensionDestroyingItemEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the item that is being destroyed.
        /// </summary>
        public Pickup Pickup { get; }

        /// <summary>
        /// Creates a new <see cref="PocketDimensionDestroyingItemEventArgs"/> instance.
        /// </summary>
        /// <param name="item">The item that is being destroyed.</param>
        public PocketDimensionDestroyingItemEventArgs(Pickup item)
            => Pickup = item;
    }
}