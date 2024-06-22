using Common.Extensions;

using LabExtended.API;

using PlayerRoles;

namespace LabExtended.Core.Commands.Parsing.PlayerList.Nodes
{
    public static class TokenParser
    {
        public static IEnumerable<ExPlayer> Run(IExpressionNode expressionNode, Func<IEnumerable<ExPlayer>, string, IEnumerable<ExPlayer>> tagPredicate)
        {
            if (expressionNode is StatementsNode statementsNode)
                expressionNode = statementsNode.Expressions[0];

            if (expressionNode is BinaryOperationNode binaryOperationNode)
            {
                if (binaryOperationNode.Operation.Token.Name == "ROLE")
                {
                    var roleType = RoleTypeId.None;

                    if (int.TryParse(((TextNode)binaryOperationNode.RightOperand).Token.Text, out var enumNumeric)
                        && Enum.IsDefined(typeof(RoleTypeId), enumNumeric))
                        roleType = (RoleTypeId)enumNumeric;
                    else if (Enum.TryParse<RoleTypeId>(((TextNode)binaryOperationNode.RightOperand).Token.Text, true, out var parsedRole))
                        roleType = parsedRole;
                    else
                        throw new Exception($"Unable to parse RoleType: {((TextNode)binaryOperationNode.RightOperand).Token.Text}");

                    return Run(binaryOperationNode.LeftOperand, tagPredicate).Where(p => p.Role.Type == roleType);
                }
                else if (binaryOperationNode.Operation.Token.Name == "TEAM")
                {
                    var teamType = Team.OtherAlive;

                    if (int.TryParse(((TextNode)binaryOperationNode.RightOperand).Token.Text, out var enumNumeric)
                        && Enum.IsDefined(typeof(Team), enumNumeric))
                        teamType = (Team)enumNumeric;
                    else if (Enum.TryParse<Team>(((TextNode)binaryOperationNode.RightOperand).Token.Text, true, out var parsedTeam))
                        teamType = parsedTeam;
                    else
                        throw new Exception($"Unable to parse RoleType: {((TextNode)binaryOperationNode.RightOperand).Token.Text}");

                    return Run(binaryOperationNode.LeftOperand, tagPredicate).Where(p => p.Role.Team == teamType);
                }
                else if (binaryOperationNode.Operation.Token.Name == "TAG")
                {
                    if (tagPredicate is null)
                        throw new ArgumentNullException(nameof(tagPredicate));

                    return tagPredicate(Run(binaryOperationNode.LeftOperand, tagPredicate), ((TextNode)binaryOperationNode.RightOperand).Token.Text);
                }
                else if (binaryOperationNode.Operation.Token.Name == "RAND")
                {
                    if (!int.TryParse(((TextNode)binaryOperationNode.RightOperand).Token.Text, out var playerCount))
                        throw new Exception("Failed to parse player count.");

                    var list = Run(binaryOperationNode.LeftOperand, tagPredicate);

                    if (list.Count() < playerCount)
                        throw new Exception("Not enough players for random selection.");

                    var output = new List<ExPlayer>(playerCount);

                    while (output.Count != playerCount)
                    {
                        var randomItem = list.FirstOrDefault(p => UnityEngine.Random.Range(0, 1) == 1);

                        if (randomItem is null)
                            continue;

                        if (output.Contains(randomItem))
                            continue;

                        output.Add(randomItem);
                    }

                    return output;
                }
                else if (binaryOperationNode.Operation.Token.Name == "RANK")
                {
                    var text = ((TextNode)binaryOperationNode.RightOperand).Token.Text;

                    if (string.IsNullOrWhiteSpace(text))
                        throw new ArgumentNullException(nameof(text));

                    if (text == "*")
                        return Run(binaryOperationNode.LeftOperand, tagPredicate).Where(p => !string.IsNullOrWhiteSpace(p.Hub.serverRoles.Network_myText));
                    else if (text == "!*")
                        return Run(binaryOperationNode.LeftOperand, tagPredicate).Where(p => string.IsNullOrWhiteSpace(p.Hub.serverRoles.Network_myText));
                    else if (text.StartsWith("!"))
                        return Run(binaryOperationNode.LeftOperand, tagPredicate).Where(p => !string.IsNullOrWhiteSpace(p.Hub.serverRoles.Network_myText) && p.Hub.serverRoles.Network_myText.GetSimilarity(text.Substring(1)) < 0.8);
                    else
                        return Run(binaryOperationNode.LeftOperand, tagPredicate).Where(p => !string.IsNullOrWhiteSpace(p.Hub.serverRoles.Network_myText) && p.Hub.serverRoles.Network_myText.GetSimilarity(text.Substring(1)) >= 0.8);
                }
                else
                    throw new Exception($"Unkown binary operand");
            }
            else if (expressionNode is TextNode textNode)
            {
                if (textNode.NodeType is TextNodeType.All)
                    return ExPlayer.Players;
                else if (textNode.NodeType is TextNodeType.Player)
                    return ExPlayer.Players.Where(p => p.Name.GetSimilarity(textNode.Token.Text) >= 0.85 || p.UserId == textNode.Token.Text);
                else if (textNode.NodeType is TextNodeType.Number && int.TryParse(textNode.Token.Text, out var playerId))
                    return ExPlayer.Players.Where(p => p.PlayerId == playerId);
                else
                    throw new Exception("Unknown text node");
            }
            else if (expressionNode is UnOpNode unOpNode)
            {
                if (unOpNode.TextToken.Token.Name == "NAME")
                    return ExPlayer.Players.Where(p => p.Name.GetSimilarity(((TextNode)unOpNode.Operand).Token.Text) >= 0.85);
                else
                    throw new Exception("Unknown node");
            }
            else
                throw new Exception($"Unknown node");
        }

        public static TextToken Match(ref int position, List<TextToken> tokens, params PlayerToken[] expectedTokens)
        {
            if (position < tokens.Count)
            {
                var currentToken = tokens[position];

                if (expectedTokens.Any(token => token.Name == currentToken.Token.Name))
                {
                    position++;
                    return currentToken;
                }
            }

            return null;
        }

        public static TextToken Require(ref int position, List<TextToken> tokens, params PlayerToken[] expectedTokens)
        {
            var token = Match(ref position, tokens, expectedTokens);

            if (token is null)
                throw new Exception($"{expectedTokens.First().Name} is expected at position {position}");

            return token;
        }

        public static IExpressionNode ParseCode(ref int position, List<TextToken> tokens)
        {
            var statementsNode = new StatementsNode();

            while (position < tokens.Count)
                statementsNode.Expressions.Add(ParseExpression(ref position, tokens));

            return statementsNode;
        }

        internal static IExpressionNode ParseExpression(ref int position, List<TextToken> tokens)
        {
            if (Match(ref position, tokens, PlayerToken.DefinedTokens["NUMBER"]) != null)
            {
                position -= 1;
                return ParseNumberNode(ref position, tokens);
            }
            else if (Match(ref position, tokens, PlayerToken.DefinedTokens["PLAYER"]) != null)
            {
                position -= 1;
                return ParsePlayerNode(ref position, tokens);
            }
            else if (Match(ref position, tokens, PlayerToken.DefinedTokens["ALL"]) != null)
            {
                position -= 1;
                return ParseAllNode(ref position, tokens);
            }
            else if (Match(ref position, tokens, PlayerToken.DefinedTokens["ANY"]) != null)
            {
                position -= 1;
                return ParseAnyNode(ref position, tokens);
            }

            return ParseFormula(ref position, tokens);
        }

        internal static IExpressionNode ParseFormula(ref int position, List<TextToken> tokens)
        {
            var operand = Match(ref position, tokens, PlayerToken.DefinedTokens["NAME"], PlayerToken.DefinedTokens["RAND"], PlayerToken.DefinedTokens["RANK"], PlayerToken.DefinedTokens["ROLE"], PlayerToken.DefinedTokens["TEAM"], PlayerToken.DefinedTokens["TAG"]);

            if (operand != null)
            {
                if (Require(ref position, tokens, PlayerToken.DefinedTokens["LPAR"]) != null)
                {
                    if (Match(ref position, tokens, PlayerToken.DefinedTokens["NAME"]) != null)
                        return new UnOpNode(operand, ParseAnyNode(ref position, tokens));

                    var leftOperand = ParseExpression(ref position, tokens);

                    Require(ref position, tokens, PlayerToken.DefinedTokens["COMMA"]);

                    if (Match(ref position, tokens, PlayerToken.DefinedTokens["TAG"]) != null)
                        return new BinaryOperationNode(operand, leftOperand, ParseAnyNode(ref position, tokens));

                    if (Match(ref position, tokens, PlayerToken.DefinedTokens["RANK"]) != null)
                        return new BinaryOperationNode(operand, leftOperand, ParseAnyNode(ref position, tokens));

                    var rightOperand = ParseExpression(ref position, tokens);

                    Require(ref position, tokens, PlayerToken.DefinedTokens["RPAR"]);
                    return new BinaryOperationNode(operand, leftOperand, rightOperand);
                }
            }

            throw new Exception($"Operator expected at {position} position");
        }

        internal static IExpressionNode ParseNumberNode(ref int position, List<TextToken> tokens)
        {
            var token = Match(ref position, tokens, PlayerToken.DefinedTokens["NUMBER"]);

            if (token != null)
                return new TextNode(token, TextNodeType.Number);

            throw new Exception($"Regex '[^) ]+' match was expected at {position}");
        }

        internal static IExpressionNode ParsePlayerNode(ref int position, List<TextToken> tokens)
        {
            var token = Match(ref position, tokens, PlayerToken.DefinedTokens["PLAYER"]);

            if (token != null)
                return new TextNode(token, TextNodeType.Player);

            throw new Exception($"Regex '[^) ]+' match was expected at {position}");
        }

        internal static IExpressionNode ParseAllNode(ref int position, List<TextToken> tokens)
        {
            var token = Match(ref position, tokens, PlayerToken.DefinedTokens["ALL"]);

            if (token != null)
                return new TextNode(token, TextNodeType.All);

            throw new Exception($"Regex '*' match was expected at {position}");
        }

        internal static IExpressionNode ParseAnyNode(ref int position, List<TextToken> tokens)
        {
            var token = Match(ref position, tokens, PlayerToken.DefinedTokens["ANY"]);

            if (token != null)
                return new TextNode(token, TextNodeType.Any);

            throw new Exception($"Regex '[^) ]+' match was expected at {position}");
        }
    }
}