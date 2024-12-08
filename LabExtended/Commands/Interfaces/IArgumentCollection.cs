using LabExtended.API;
using LabExtended.Commands.Arguments;

namespace LabExtended.Commands.Interfaces
{
    public interface IArgumentCollection
    {
        void Initialize(ExPlayer player, ICommandContext ctx, ArgumentCollection args);
    }
}