using System.Net;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using LabExtended.Core;

using LabExtended.Extensions;
using LabExtended.Utilities;

namespace LabExtended.API.CustomCommands.Image.Play;

public class PlayCommand : CustomCommand
{
    public override string Command { get; } = "play";
    public override string Description { get; } = "Starts image playback.";

    public override ArgumentDefinition[] BuildArgs()
    {
        return GetArgs(x =>
        {
            x.WithArg<int>("ID", "ID of the primitive to display the image on.");
            x.WithArg<string>("Link", "Link to the image.");
            x.WithArg<int>("Fps", "Frames per second");
            x.WithOptional("Clear on Finish", "Whether or not to set a white frame when playback is finished.", true);
        });
    }

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        var id = args.Get<int>("ID");
        var link = args.Get<string>("Link");
        var fps = args.Get<int>("Fps");
        var clear = args.Get<bool>("Clear on Finish");

        if (!ImageCommand.SpawnedImages.TryGetValue(id, out var image))
        {
            ctx.RespondFail($"No image with ID {id}");
            return;
        }
        
        ctx.RespondOk($"Downloading image from {link}...");

        Task.Run(async () =>
        {
            try
            {
                var client = new WebClient();
                var path = Path.GetTempFileName();
                
                await client.DownloadFileTaskAsync(link, path);

                var image = System.Drawing.Image.FromFile(path);
                var frames = image.ExtractFrames(image.Width, image.Height);

                return ImageUtils.ToPrimitiveFrames(frames);
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
                ctx.Message($"Extracted {task.Result.Count} frame(s)");

                image.ClearOnFinish = clear;
                image.Play(task.Result, fps <= 0 ? null : fps);
                
                ctx.Message($"Started playback.");
            }
            catch (Exception ex)
            {
                ctx.Message(ex);
            }
        });
    }
}