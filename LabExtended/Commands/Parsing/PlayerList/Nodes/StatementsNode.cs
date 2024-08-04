namespace LabExtended.Commands.Parsing.PlayerList.Nodes
{
    public class StatementsNode : IExpressionNode
    {
        public List<IExpressionNode> Expressions { get; } = new List<IExpressionNode>();
    }
}