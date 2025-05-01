using HarmonyLib;

using LabExtended.Core;
using LabExtended.Core.Networking;

using Mirror;

using PlayerRoles.PlayableScps.Scp939;

namespace LabExtended.Patches.Functions.Scp939
{
    public static class AmnesticCloudAudioPatch
    {
        public const int FunctionHash = -193115792;

        [HarmonyPatch(typeof(Scp939AmnesticCloudInstance), nameof(Scp939AmnesticCloudInstance.RpcPlayCreateSound))]
        public static bool Prefix(Scp939AmnesticCloudInstance __instance)
        {
            try
            {
                MirrorMethods.WriteToWhere(p => p.Toggles.CanHearAmnesticCloudSpawn,
                    w =>
                    {
                        w.WriteMessageId<RpcMessage>();
                        w.WriteUInt(__instance.netId);
                        w.WriteByte(__instance.ComponentIndex);
                        w.WriteUShort(unchecked((ushort)FunctionHash));
                        w.WriteUInt(0);
                    });
                return false;
            }
            catch (Exception ex)
            {
                ApiLog.Error("AmnesticCloudAudioPatch", ex);
                return true;
            }
        }
    }
}