using InventorySystem.Items.Firearms;

using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    public class PlayerAuthorizingShotArgs : BoolCancellableEvent
    {
        public ExPlayer Player { get; }

        public Firearm Firearm { get; }

        public byte SubstractAmmo { get; set; }

        internal PlayerAuthorizingShotArgs(ExPlayer player, Firearm firearm, byte ammo) => (Player, Firearm, SubstractAmmo) = (player, firearm, ammo);
    }
}