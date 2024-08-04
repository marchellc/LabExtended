namespace LabExtended.Commands.Parsing.PlayerList
{
    public class TextToken
    {
        public PlayerToken Token { get; internal set; }

        public string Text { get; internal set; }
        public int Position { get; }

        internal TextToken(PlayerToken token, string text, int position)
        {
            Token = token;
            Text = text;
            Position = position;
        }
    }
}