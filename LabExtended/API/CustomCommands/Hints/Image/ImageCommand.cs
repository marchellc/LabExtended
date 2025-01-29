using System.Net;

using LabApi.Loader.Features.Paths;

using LabExtended.API.Hints;
using LabExtended.API.Hints.Elements;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.CustomData;
using LabExtended.Commands.Interfaces;

using LabExtended.Core;
using LabExtended.Core.Threading;

using LabExtended.Extensions;
using LabExtended.Utilities;

namespace LabExtended.API.CustomCommands.Hints.Image;

public class ImageCommand : CustomCommand
{
    public override string Command { get; } = "image";
    public override string Description { get; } = "Shows an image.";

    public override ArgumentDefinition[] BuildArgs()
    {
        return GetArgs(x =>
        {
            x.WithArg<PlayerListData>("Players", "The players to show the image.");
            
            x.WithArg<string>("Link", "Link to the image.");
            
            x.WithArg<int>("Height", "The height of the image.");
            x.WithArg<int>("Width", "The width of the image.");
            x.WithArg<int>("Fps", "Frames per second");
            
            x.WithOptional<int>("Duration", "The duration of the image.", -1);
        });
    }

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        var players = args.Get<PlayerListData>("Players");
        var link = args.Get<string>("Link");
        var height = args.Get<int>("Height");
        var width = args.Get<int>("Width");
        var fps = args.Get<int>("Fps");
        var duration = args.Get<int>("Duration");
        
        ctx.RespondOk($"Downloading image from {link}...");

        Task.Run(async () =>
        {
            ApiLog.Debug("ImageCommand", $"Downloading image from {link}...");
            
            try
            {
                var client = new WebClient();
                
                ApiLog.Debug("ImageCommand", $"Sending GET");
                
                var path = Path.GetTempFileName();
                
                await client.DownloadFileTaskAsync(link, path);
                
                ApiLog.Debug("ImageCommand", "Successfully downloaded image.");

                var image = System.Drawing.Image.FromFile(path);
                
                ApiLog.Debug("ImageCommand", $"Image: {image.Height}x{image.Width} ({image.HorizontalResolution}h x {image.VerticalResolution}v) Raw={image.RawFormat} Pixel={image.PixelFormat} Dimensions={image.FrameDimensionsList.Length}");

                var frames = image.ExtractFrames(width, height);
                
                ApiLog.Debug("ImageCommand", $"Extracted {frames.Length} frame(s)");
                return ImageUtils.ToHintFrames(frames);
            }
            catch (Exception ex)
            {
                ApiLog.Error("ImageCommand", ex);

                throw ex;
            }
        }).ContinueWithOnMain(task =>
        {
            if (task.Exception != null)
            {
                ctx.Message(task.Exception);
                return;
            }

            try
            {
                ctx.Message($"Extracted {task.Result.Length} frame(s)");

                var directory = Path.Combine(PathManager.LabApi.FullName, "Downloaded Images");
                
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var index = 0;
                
                foreach (var frame in task.Result)
                    File.WriteAllText(Path.Combine(directory, $"Frame {index++}.txt"), frame);

                foreach (var player in players)
                {
                    if (!player)
                        continue;

                    if (!HintController.TryGet<PersonalImageElement>(player, out var personalImageElement))
                    {
                        ApiLog.Debug("ImageCommand", $"Player {player.Name} has no image element");
                        continue;
                    }

                    ApiLog.Debug("ImageCommand", personalImageElement);

                    if (duration != -1)
                        personalImageElement.CustomFrameDelay = duration;
                    
                    personalImageElement.Play(task.Result, fps);
                    
                    ApiLog.Debug("ImageCommand", personalImageElement);
                }

                ctx.Message($"Image shown to {players.Count} player(s).");
            }
            catch (Exception ex)
            {
                ctx.Message(ex);
            }
        });
    }
}