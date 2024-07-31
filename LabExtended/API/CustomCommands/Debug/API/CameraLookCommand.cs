using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;

using MEC;

namespace LabExtended.API.CustomCommands.Debug.API
{
    public class CameraLookCommand : CustomCommand
    {
        private CoroutineHandle _coroutine;

        public override string Command => "camlook";
        public override string Description => "Forces a camera to look at you.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            if (Timing.IsRunning(_coroutine))
                Timing.KillCoroutines(_coroutine);

            _coroutine = Timing.RunCoroutine(Coroutine(sender));

            ctx.RespondOk($"Forcing cams to look at you.");
        }

        private static IEnumerator<float> Coroutine(ExPlayer player)
        {
            var curCam = default(Camera);

            while (true)
            {
                var closestCam = player.ClosestCamera;

                if (closestCam != null && (curCam is null || curCam != closestCam))
                    curCam = closestCam;

                curCam?.LookAt(player);

                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}