using NorthwoodLib.Pools;

namespace LabExtended.Commands.Parsing.PlayerList
{
    public static class PlayerListLexer
    {
        public static List<TextToken> LexicalAnalysis(string code)
        {
            var list = ListPool<TextToken>.Shared.Rent();
            var position = 0;

            while (GetNextToken(code, list, ref position))
                continue;

            return list;
        }

        internal static bool GetNextToken(string code, List<TextToken> tokens, ref int position)
        {
            if (position >= code.Length)
                return false;

            foreach (var playerToken in PlayerToken.DefinedTokens)
            {
                var match = playerToken.Value.Regex.Match(code.Substring(position));

                if (!string.IsNullOrWhiteSpace(match.Value))
                {
                    position += match.Value.Length;

                    if (playerToken.Value.Name != "SPACE")
                        tokens.Add(new TextToken(playerToken.Value, match.Value, position));

                    return true;
                }
            }

            throw new Exception($"Invalid position: {position} / {code.Length} ({code})");
        }
    }
}