using System.Net;

using LabExtended.API.Hints;
using LabExtended.API.Hints.Elements.Personal;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.CustomData;
using LabExtended.Commands.Interfaces;

using LabExtended.Core;
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

            x.WithArg<int>("Size", "Size of pixels (33 default).");
            x.WithArg<int>("Line Height", "Height of each line (75 default).");
            x.WithArg<int>("Height", "The height of the image.");
            x.WithArg<int>("Width", "The width of the image.");
            x.WithArg<int>("Fps", "Frames per second");
        });
    }

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        var players = args.Get<PlayerListData>("Players");
        var link = args.Get<string>("Link");
        var size = args.Get<int>("Size");
        var lineHeight = args.Get<int>("Line Height");
        var height = args.Get<int>("Height");
        var width = args.Get<int>("Width");
        var fps = args.Get<int>("Fps");
        
        ctx.RespondOk($"Downloading image from {link}...");

        Task.Run(async () =>
        {
            try
            {
                var client = new WebClient();
                var path = Path.GetTempFileName();
                
                await client.DownloadFileTaskAsync(link, path);

                var image = System.Drawing.Image.FromFile(path);
                var frames = image.ExtractFrames(width, height);

                return ImageUtils.ToHintFrames(frames, size <= 0 ? 33 : size, lineHeight <= 0 ? 75 : height);
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

                foreach (var player in players)
                {
                    if (!player)
                        continue;

                    if (!player.TryGetHintElement<PersonalImageElement>(out var personalImageElement))
                    {
                        player.AddHintElement(new PersonalImageElement());
                        personalImageElement = player.GetHintElement<PersonalImageElement>();
                    }

                    personalImageElement.Play(task.Result, fps);
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