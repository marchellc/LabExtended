using LabApi.Features.Wrappers;

using UnityEngine;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when an item is dropped from the Pocket Dimension.
    /// </summary>
    public class PocketDimensionDroppingItemEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the item that is being dropped.
        /// </summary>
        public Pickup Pickup { get; }

        /// <summary>
        /// Gets or sets the item's drop position.
        /// </summary>
        public Vector3 Position { get; set; }
        
        /// <summary>
        /// Gets or sets the item's velocity.
        /// </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Creates a new <see cref="PocketDimensionDroppingItemEventArgs"/> instance.
        /// </summary>
        /// <param name="item">The item being drop.</param>
        /// <param name="pos">The drop position.</param>
        /// <param name="vel">The item's velocity.</param>
        public PocketDimensionDroppingItemEventArgs(Pickup item, Vector3 pos, Vector3 vel)
            => (Pickup, Position, Velocity) = (item, pos, vel);
    }
}