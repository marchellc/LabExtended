namespace LabExtended.Commands.Parsing.PlayerList.Nodes
{
    public class BinaryOperationNode : IExpressionNode
    {
        public TextToken Operation { get; internal set; }

        public IExpressionNode LeftOperand { get; internal set; }
        public IExpressionNode RightOperand { get; internal set; }

        internal BinaryOperationNode(TextToken operation, IExpressionNode leftOperand, IExpressionNode rightOperand)
        {
            Operation = operation;
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }
    }
}