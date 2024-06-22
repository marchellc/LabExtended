namespace LabExtended.Core.Commands.Parsing.PlayerList.Nodes
{
    public class UnOpNode : IExpressionNode
    {
        public TextToken TextToken { get; internal set; }
        public IExpressionNode Operand { get; internal set; }

        internal UnOpNode(TextToken textToken, IExpressionNode operand)
        {
            TextToken = textToken;
            Operand = operand;
        }
    }
}