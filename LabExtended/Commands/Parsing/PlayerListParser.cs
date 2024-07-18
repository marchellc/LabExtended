using LabExtended.API;

using LabExtended.Core.Commands.Interfaces;
using LabExtended.Core.Commands.Parsing.PlayerList;
using LabExtended.Core.Commands.Parsing.PlayerList.Nodes;

namespace LabExtended.Core.Commands.Parsing
{
    public class PlayerListParser : ICommandParser
    {
        public string Name => "A list of players.";
        public string Description => "A list of players, to view the full usage use the 'formatting playerlist' command.";

        public bool TryParse(string value, out string failureMessage, out object result)
        {
            result = null;
            failureMessage = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                failureMessage = $"String cannot be empty or white-spaced.";
                return false;
            }

            try
            {
                var position = 0;
                var tokens = PlayerListLexer.LexicalAnalysis(value);
                var players = TokenParser.Run(TokenParser.ParseCode(ref position, tokens), (list, tag) =>
                                        list.Where(p => ServerStatic.PermissionsHandler != null &&
                                                        ServerStatic.PermissionsHandler._members.TryGetValue(p.UserId, out var playerTag) &&
                                                            !string.IsNullOrWhiteSpace(playerTag) && playerTag == tag));

                var list = new CustomData.PlayerList(players.ToList());

                result = list;
                return true;
            }
            catch (Exception ex)
            {
                failureMessage = $"Player list parsing failed: {ex.Message}";
                return false;
            }
        }

        public static List<ExPlayer> SelectPlayers(string selector)
        {
            var position = 0;
            var tokens = PlayerListLexer.LexicalAnalysis(selector);
            var players = TokenParser.Run(TokenParser.ParseCode(ref position, tokens), (list, tag) =>
                                        list.Where(p => ServerStatic.PermissionsHandler != null &&
                                                        ServerStatic.PermissionsHandler._members.TryGetValue(p.UserId, out var playerTag) &&
                                                            !string.IsNullOrWhiteSpace(playerTag) && playerTag == tag));

            return players.ToList();
        }
    }
}