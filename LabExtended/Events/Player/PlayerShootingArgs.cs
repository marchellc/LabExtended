using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;

using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    public class PlayerShootingArgs : BoolCancellableEvent
    {
        public ExPlayer Player { get; }

        public Firearm Firearm { get; }

        public ShotMessage Message { get; set; }

        internal PlayerShootingArgs(ExPlayer player, Firearm firearm, ShotMessage msg) => (Player, Firearm, Message) = (player, firearm, msg);
    }
}