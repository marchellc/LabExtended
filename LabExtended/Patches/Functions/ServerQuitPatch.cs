using HarmonyLib;

using LabExtended.Events;

namespace LabExtended.Patches.Functions;

public static class ServerQuitPatch
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.OnApplicationQuit))]
    public static bool Prefix()
    {
        ServerEvents.OnQuitting();
        return true;
    }
}