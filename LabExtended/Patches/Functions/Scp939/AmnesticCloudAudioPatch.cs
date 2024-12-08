﻿using HarmonyLib;

using LabExtended.API;
using LabExtended.Core.Networking;

using Mirror;

using PlayerRoles.PlayableScps.Scp939;

namespace LabExtended.Patches.Functions.Scp939
{
    public static class AmnesticCloudAudioPatch
    {
        public const string FunctionName = "System.Void PlayerRoles.PlayableScps.Scp939.Scp939AmnesticCloudInstance::RpcPlayCreateSound()";
        public const int FunctionHash = -193115792;

        [HarmonyPatch(typeof(Scp939AmnesticCloudInstance), nameof(Scp939AmnesticCloudInstance.RpcPlayCreateSound))]
        public static bool Prefix(Scp939AmnesticCloudInstance __instance)
        {
            __instance.SendRpc(FunctionName, FunctionHash, (NetworkWriter)null, 0, true, true, ExPlayer.Get(x => x.Switches.CanHearAmnesticCloudSpawn));
            return false;
        }
    }
}