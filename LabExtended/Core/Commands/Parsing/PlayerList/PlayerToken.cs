using System.Text.RegularExpressions;

namespace LabExtended.Core.Commands.Parsing.PlayerList
{
    public struct PlayerToken
    {
        public static IReadOnlyDictionary<string, PlayerToken> DefinedTokens { get; } = new Dictionary<string, PlayerToken>()
        {
            { "PLAYER", new PlayerToken("PLAYER", "[0-9]{17}@steam|[0-9]{18}@discord") },
            { "NUMBER", new PlayerToken("NUMBER", "[0-9]*") },
            { "LPAR",   new PlayerToken("LPAR", "\\(") },
            { "RPAR",   new PlayerToken("RPAR", "\\)") },
            { "COMMA",  new PlayerToken("COMMA", ",") },
            { "ALL",    new PlayerToken("ALL", "\\*") },
            { "SPACE",  new PlayerToken("SPACE", " ") },
            { "RAND",   new PlayerToken("RAND", "rand") },
            { "RANK",   new PlayerToken("RANK", "rank") },
            { "ROLE",   new PlayerToken("ROLE", "role") },
            { "TEAM",   new PlayerToken("TEAM", "team") },
            { "NAME",   new PlayerToken("NAME", "name") },
            { "TAG",    new PlayerToken("TAG", "tag") },
            { "ANY",    new PlayerToken("ANY", "[^) ]+") }
        };

        public string Name { get; }
        public Regex Regex { get; }

        internal PlayerToken(string name, string regex)
        {
            Name = name;
            Regex = new Regex("^" + regex);
        }
    }
}