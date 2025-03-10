using LabExtended.API;
using LabExtended.API.Items.Candies;

using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    public class PlayerDroppingCandyArgs : BoolCancellableEvent
    {
        public ExPlayer Player { get; }
        public CandyItem Candy { get; }

        internal PlayerDroppingCandyArgs(ExPlayer? player, CandyItem candy)
            => (Player, Candy) = (player, candy);
    }
}