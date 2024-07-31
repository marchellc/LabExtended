﻿using LabExtended.API;
using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.Core.Commands.Parsing
{
    public class PlayerParser : ICommandParser
    {
        public string Name => "Player";
        public string Description => "A player (you can specify a player's user ID, player ID, name, network ID or IP).";

        public bool TryParse(string value, out string failureMessage, out object result)
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

            result = ExPlayer.Players.FirstOrDefault(p => p.Address == value);
            return result != null;
        }
    }
}