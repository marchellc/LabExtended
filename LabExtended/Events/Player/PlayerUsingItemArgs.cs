using InventorySystem.Items.Usables;

using LabExtended.API;

namespace LabExtended.Events.Player
{
    public class PlayerUsingItemArgs
    {
        public ExPlayer Player { get; }
        public UsableItem Item { get; }

        public float RemainingCooldown { get; set; }
        public float SpeedMultiplier { get; set; }

        internal PlayerUsingItemArgs(ExPlayer player, UsableItem item, float cooldown, float multiplier)
            => (Player, Item, RemainingCooldown, SpeedMultiplier) = (player, item, cooldown, multiplier);
    }
}