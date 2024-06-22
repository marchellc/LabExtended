using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Searching;

using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player tries to pick up an ammo item.
    /// </summary>
    public class PlayerPickingUpAmmoArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// Gets the player who's picking the ammo pickup up.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the targeted pickup.
        /// </summary>
        public AmmoPickup Pickup { get; }

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

        /// <summary>
        /// Gets or sets the amount of ammo to take.
        /// </summary>
        public uint Amount { get; set; }

        internal PlayerPickingUpAmmoArgs(ExPlayer player, AmmoPickup pickup,
            SearchCompletor searchCompletor, SearchSessionPipe searchPipe, SearchCoordinator searchCoordinator, bool destroyPickup, uint amount) : base(true)
        {
            Player = player;
            Pickup = pickup;

            SearchCompletor = searchCompletor;
            SearchPipe = searchPipe;
            SearchCoordinator = searchCoordinator;

            DestroyPickup = destroyPickup;
            Amount = amount;
        }
    }
}