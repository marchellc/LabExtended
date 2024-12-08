namespace LabExtended.Commands.Parsing.PlayerList.Nodes
{
    public class TextNode : IExpressionNode
    {
        public TextToken Token { get; internal set; }
        public TextNodeType NodeType { get; }

        internal TextNode(TextToken token, TextNodeType nodeType)
        {
            Token = token;
            NodeType = nodeType;
        }
    }
}