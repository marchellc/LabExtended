using HarmonyLib;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Scp3114;

using Mirror;

using PlayerRoles.PlayableScps.Scp3114;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ServerProcessCmd))]
    public static class Scp3114StranglePatch
    {
        public static bool Prefix(Scp3114Strangle __instance, NetworkReader reader)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            var target = __instance.ProcessAttackRequest(reader);
            var player = target.HasValue ? ExPlayer.Get(target.Value.Target) : null;

            if (player != null && ((!scp.Switches.CanStrangleAs3114 || !player.Switches.CanBeStrangledBy3114) || !HookRunner.RunEvent(new Scp3114StranglingArgs(scp, player), true)))
            {
                __instance.SyncTarget = null;

                __instance._rpcType = Scp3114Strangle.RpcType.OutOfRange;
                __instance.ServerSendRpc(true);

                return false;
            }

            if (target != null && __instance.SyncTarget is null)
                scp.Subroutines.Scp3114VoiceLines.ServerPlayConditionally(Scp3114VoiceLines.VoiceLinesName.StartStrangle);

            __instance.SyncTarget = target;

            __instance._rpcType = Scp3114Strangle.RpcType.TargetResync;
            __instance.ServerSendRpc(true);

            return false;
        }
    }
}