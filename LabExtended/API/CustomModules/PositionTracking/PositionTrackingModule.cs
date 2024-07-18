using LabExtended.API.Collections.Locked;
using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Core.Ticking;

using UnityEngine;

namespace LabExtended.API.Modules.PositionTracking
{
    /// <summary>
    /// A module used to set custom object's positions to the player's position.
    /// </summary>
    public class PositionTrackingModule : GenericModule<ExPlayer>
    {
        private readonly LockedHashSet<PositionTrackingEntry> _positionTrackers = new LockedHashSet<PositionTrackingEntry>(); // Entry cache.
        private readonly LockedHashSet<PositionTrackingEntry> _removeNextTick = new LockedHashSet<PositionTrackingEntry>(); // Used to avoid collection exceptions.

        /// <inheritdoc/>
        public override TickTimer TickTimer { get; } = TickTimer.None;

        /// <summary>
        /// Gets a list of all active entries.
        /// </summary>
        public IEnumerable<PositionTrackingEntry> Entries => _positionTrackers;

        /// <summary>
        /// Gets a count of all active entries.
        /// </summary>
        public int EntryCount => _positionTrackers.Count;

        /// <summary>
        /// Gets the player's position.
        /// </summary>
        public Vector3 Position => CastParent.Position;

        /// <summary>
        /// Gets the player's rotation.
        /// </summary>
        public Quaternion Rotation => CastParent.Rotation;

        /// <summary>
        /// Gets a value indicating whether the player is alive or not.
        /// </summary>
        public bool IsAlive => CastParent.Role.IsAlive;

        /// <summary>
        /// Adds a new position tracking entry.
        /// </summary>
        /// <param name="positionTrackingEntry">The entry to add.</param>
        /// <returns><see langword="true"/> if the entry was successfully added, otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Add(PositionTrackingEntry positionTrackingEntry)
        {
            if (positionTrackingEntry is null)
                throw new ArgumentNullException(nameof(positionTrackingEntry));

            if (_positionTrackers.Contains(positionTrackingEntry))
                return false;

            _positionTrackers.Add(positionTrackingEntry);
            return true;
        }

        /// <summary>
        /// Removes a position tracking entry.
        /// </summary>
        /// <param name="positionTrackingEntry">The entry to remove.</param>
        /// <returns><see langword="true"/> if the entry was successfully removed., otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Remove(PositionTrackingEntry positionTrackingEntry)
        {
            if (positionTrackingEntry is null)
                throw new ArgumentNullException(nameof(positionTrackingEntry));

            return _positionTrackers.Remove(positionTrackingEntry);
        }

        /// <inheritdoc/>
        public override void OnTick()
        {
            base.OnTick();

            foreach (var entry in _removeNextTick)
                try { _positionTrackers.Remove(entry); } catch { }

            _removeNextTick.Clear();

            if (CastParent is null || !IsAlive)
                return;

            foreach (var entry in _positionTrackers)
            {
                if (entry.UpdateDelay > 0 && (DateTime.Now - entry._lastSet).TotalMilliseconds < entry.UpdateDelay)
                    continue;

                try
                {
                    var position = Position;
                    var rotation = Rotation;

                    if (entry.CustomPosition != null)
                    {
                        var customPos = entry.CustomPosition(CastParent);

                        if (customPos.HasValue)
                            position = customPos.Value;
                    }

                    if (entry.CustomRotation != null)
                    {
                        var customRot = entry.CustomRotation(CastParent);

                        if (customRot.HasValue)
                            rotation = customRot.Value;
                    }

                    entry.UpdateObject(position, rotation);
                }
                catch (Exception ex)
                {
                    ExLoader.Error("Position Tracking Module", $"Failed to update positional tracking for an object:\n{ex.ToColoredString()}");
                }
            }
        }
    }
}