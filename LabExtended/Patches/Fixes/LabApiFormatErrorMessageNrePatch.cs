using HarmonyLib;

using LabApi.Events;

namespace LabExtended.Patches.Fixes;

public static class LabApiFormatErrorMessageNrePatch
{
    [HarmonyPatch(typeof(EventManager), nameof(EventManager.FormatErrorMessage))]
    public static bool Prefix(Delegate eventHandler, Exception exception, ref string __result)
    {
        __result = string.Concat(new string[]
        {
            "'",
            exception.GetType().Name,
            "' occured while invoking '",
            eventHandler?.Method?.Name ?? "(null)",
            "' on '",
            eventHandler?.Method?.DeclaringType?.FullName ?? "(null)",
            "': '",
            exception.Message,
            "', stack trace:\n",
            exception.StackTrace
        });

        return false;
    }
}