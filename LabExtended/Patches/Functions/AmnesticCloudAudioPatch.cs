using HarmonyLib;

using LabExtended.API;
using LabExtended.Utilities;

using Mirror;

using PlayerRoles.PlayableScps.Scp939;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(Scp939AmnesticCloudInstance), nameof(Scp939AmnesticCloudInstance.RpcPlayCreateSound))]
    public static class AmnesticCloudAudioPatch
    {
        public const string FunctionName = "System.Void PlayerRoles.PlayableScps.Scp939.Scp939AmnesticCloudInstance::RpcPlayCreateSound()";
        public const int FunctionHash = -193115792;

        public static bool Prefix(Scp939AmnesticCloudInstance __instance)
        {
            using (var writer = NetworkWriterPool.Get())
                NetworkUtils.SendRpc(__instance, FunctionName, FunctionHash, writer, 0, true, true, ExPlayer.Get(x => x.Switches.CanHearAmnesticCloudSpawn));

            return false;
        }
    }
}