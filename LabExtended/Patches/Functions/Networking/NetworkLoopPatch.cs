using HarmonyLib;

using LabExtended.Core.Networking;

using Mirror;

namespace LabExtended.Patches.Functions.Networking;

public static class NetworkLoopPatch
{
    [HarmonyPatch(typeof(NetworkLoop), nameof(NetworkLoop.NetworkEarlyUpdate))]
    public static bool EarlyPrefix() => !MirrorUpdate.IsEnabled;
    
    [HarmonyPatch(typeof(NetworkLoop), nameof(NetworkLoop.NetworkLateUpdate))]
    public static bool LatePrefix() => !MirrorUpdate.IsEnabled;
}