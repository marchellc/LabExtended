using LabExtended.API;
using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.Core.Commands.Arguments
{
    public class PlayerArgument : GenericArgument<ExPlayer>
    {
        public PlayerArgument(string name = "Player", string description = "A player (can be their user ID, player ID, network ID, connection ID, name)",
            ExPlayer defaultValue = null, ICommandParser parser = null)
            : base(name, description, defaultValue, parser)
        { }
    }
}