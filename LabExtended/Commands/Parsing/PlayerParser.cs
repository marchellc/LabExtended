using LabExtended.API;

namespace LabExtended.Commands.Parsing
{
    public class PlayerParser : Interfaces.ICommandParser
    {
        public string Name => "Player";
        public string Description => "A player (you can specify a player's user ID, player ID, name, network ID or IP).";

        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }
        
        public bool TryParse(ExPlayer sender, string value, out string failureMessage, out object result)
        {
            failureMessage = null;
            result = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                failureMessage = "Value must be a non-empty & non-white-spaced character.";
                return false;
            }

            var player = ExPlayer.Get(value);

            if (player != null)
            {
                result = player;
                return true;
            }

            if (int.TryParse(value, out var playerId))
            {
                player = ExPlayer.Get(playerId);

                if (player != null)
                {
                    result = player;
                    return true;
                }

                if (playerId >= 0)
                {
                    player = ExPlayer.Get((uint)playerId);

                    if (player != null)
                    {
                        result = player;
                        return true;
                    }
                }
            }

            result = ExPlayer.Players.FirstOrDefault(p => p.IpAddress == value);
            return result != null;
        }
    }
}