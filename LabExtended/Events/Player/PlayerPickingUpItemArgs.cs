using InventorySystem.Items.Pickups;
using InventorySystem.Searching;

using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events
{
    /// <summary>
    /// Gets called when a player finishes picking up an item.
    /// </summary>
    public class PlayerPickingUpItemArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// Gets the player who's picking the item up.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the targeted pickup.
        /// </summary>
        public ItemPickupBase Pickup { get; }

        /// <summary>
        /// Gets the <see cref="InventorySystem.Searching.SearchCompletor"/> instance associated with this event.
        /// </summary>
        public SearchCompletor SearchCompletor { get; }

        /// <summary>
        /// Gets the <see cref="SearchSessionPipe"/> instance associated with this event.
        /// </summary>
        public SearchSessionPipe SearchPipe { get; }

        /// <summary>
        /// Gets the <see cref="InventorySystem.Searching.SearchCoordinator"/> instance associated with this event.
        /// </summary>
        public SearchCoordinator SearchCoordinator { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to destroy the pickup.
        /// </summary>
        public bool DestroyPickup { get; set; }

        internal PlayerPickingUpItemArgs(ExPlayer player, ItemPickupBase pickup, SearchCompletor searchCompletor, SearchSessionPipe searchPipe, SearchCoordinator searchCoordinator, bool destroyPickup) : base(true)
        {
            Player = player;
            Pickup = pickup;
            SearchCompletor = searchCompletor;
            SearchPipe = searchPipe;
            SearchCoordinator = searchCoordinator;
            DestroyPickup = destroyPickup;
        }
    }
}
