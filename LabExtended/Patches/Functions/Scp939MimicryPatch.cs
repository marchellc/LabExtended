using HarmonyLib;

using LabExtended.API;

using Mirror;

using PlayerRoles.PlayableScps.Scp939.Mimicry;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(EnvironmentalMimicry), nameof(EnvironmentalMimicry.ServerProcessCmd))]
    public static class Scp939MimicryPatch
    {
        public static bool Prefix(EnvironmentalMimicry __instance, NetworkReader reader)
        {
            if (!__instance.Cooldown.IsReady)
                return false;

            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            if (!scp.Switches.CanUseMimicryAs939)
                return false;

            __instance._syncOption = reader.ReadByte();
            __instance.Cooldown.Trigger(__instance._activationCooldown);
            __instance.ServerSendRpc(true);

            return false;
        }
    }
}