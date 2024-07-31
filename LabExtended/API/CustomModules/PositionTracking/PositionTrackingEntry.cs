using InventorySystem.Items.Pickups;

using UnityEngine;

namespace LabExtended.API.Modules.PositionTracking
{
    /// <summary>
    /// An entry for the positional tracking module.
    /// </summary>
    public class PositionTrackingEntry
    {
        internal DateTime _lastSet = DateTime.MinValue;

        /// <summary>
        /// The method used to update the objects position and rotation. Should return <see langword="true"/> if the object is still valid, otherwise <see langword="false"/> to remove it.
        /// </summary>
        public Func<Vector3, Quaternion, bool> UpdateObject { get; }

        /// <summary>
        /// Gets the custom position getter.
        /// </summary>
        public Func<ExPlayer, Vector3?> CustomPosition { get; }

        /// <summary>
        /// Gets the custom rotation getter.
        /// </summary>
        public Func<ExPlayer, Quaternion?> CustomRotation { get; }

        /// <summary>
        /// The delay to add between each update.
        /// </summary>
        public int UpdateDelay { get; }

        /// <summary>
        /// Creates a new <see cref="PositionTrackingEntry"/> with custom setters.
        /// </summary>
        /// <param name="updateObject">The object's position updater.</param>
        /// <param name="updateDelay">The object's update delay.</param>
        public PositionTrackingEntry(Func<Vector3, Quaternion, bool> updateObject, Func<ExPlayer, Vector3?> customPosition, Func<ExPlayer, Quaternion?> customRotation, int updateDelay = 0)
        {
            UpdateObject = updateObject;
            UpdateDelay = updateDelay;

            CustomPosition = customPosition;
            CustomRotation = customRotation;
        }

        /// <summary>
        /// Creates a new <see cref="PositionTrackingEntry"/> with an <see cref="ItemPickupBase"/> setter.
        /// </summary>
        /// <param name="pickupBase">The pickup to update.</param>
        /// <param name="updateDelay">The object's update delay.</param>
        public PositionTrackingEntry(ItemPickupBase pickupBase, Func<ExPlayer, Vector3?> customPosition, Func<ExPlayer, Quaternion?> customRotation, int updateDelay = 0)
        {
            UpdateObject = (position, rotation) => UpdatePickup(pickupBase, position, rotation);
            UpdateDelay = updateDelay;

            CustomPosition = customPosition;
            CustomRotation = customRotation;
        }

        private static bool UpdatePickup(ItemPickupBase pickup, Vector3 position, Quaternion rotation)
        {
            try
            {
                pickup.Position = position;
                pickup.Rotation = rotation;

                return true;
            }
            catch { return false; }
        }
    }
}