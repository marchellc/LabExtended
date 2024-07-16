using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.Utilities.Video;

namespace LabExtended.Commands.Debug.PrimitiveImages
{
    public class PrimitiveImageCommand : CommandInfo
    {
        private static readonly PrimitiveVideoDisplay _display = new PrimitiveVideoDisplay();

        public override string Command => throw new NotImplementedException();
        public override string Description => throw new NotImplementedException();

        public object OnCalled(ExPlayer player, string url, int width, int height)
        {
            return "Downloading ..";
        }
    }
}
