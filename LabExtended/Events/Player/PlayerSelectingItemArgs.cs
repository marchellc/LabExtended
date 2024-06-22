using InventorySystem.Items;

using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player tries changing tries currently held item.
    /// </summary>
    public class PlayerSelectingItemArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// Gets the player who's attempting to switch their item.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the currently selected item.
        /// </summary>
        public ItemBase Current { get; }

        /// <summary>
        /// Gets or sets the new item's serial.
        /// </summary>
        public ushort NextSerial { get; set; }

        /// <summary>
        /// Gets or sets the new item.
        /// </summary>
        public ItemBase NextItem
        {
            get => Player.Items.FirstOrDefault(item => item.ItemSerial == NextSerial);
            set => NextSerial = value?.ItemSerial ?? 0;
        }

        internal PlayerSelectingItemArgs(ExPlayer player, ItemBase current, ushort nextSerial)
        {
            Player = player;
            Current = current;
            NextSerial = nextSerial;
        }
    }
}