using LabExtended.API;

using LabExtended.Core.Commands;

using LabExtended.Utilities.Async;
using LabExtended.Utilities.Image;

using MEC;

namespace LabExtended.Commands.Debug.RaImages
{
    public class ShowRaImageCommand : CommandInfo
    {
        public override string Command => "raimage";
        public override string Description => "Displays an image in the Remote Admin panel.";

        public object OnCalled(ExPlayer player, string url)
        {
            Timing.CallDelayed(3f, () =>
            {
                AsyncMethods.GetStringAsync(url).Await(imageData =>
                {
                    ImageDisplay.DisplayImage(imageData, player.RemoteAdminInfo);
                });
            });

            return "Displaying ..";
        }
    }
}
