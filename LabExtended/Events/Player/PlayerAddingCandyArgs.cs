using InventorySystem.Items.Usables.Scp330;

using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    public class PlayerAddingCandyArgs : BoolCancellableEvent
    {
        public ExPlayer Player { get; }

        public bool CanAdd { get; }

        public CandyKindID CandyType { get; set; }

        internal PlayerAddingCandyArgs(ExPlayer player, CandyKindID type, bool canAdd)
            => (Player, CandyType, CanAdd) = (player, type, canAdd);
    }
}