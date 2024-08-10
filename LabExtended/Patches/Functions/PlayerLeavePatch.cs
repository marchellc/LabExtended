using HarmonyLib;

using LabExtended.API;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.OnDestroy))]
    public static class PlayerLeavePatch
    {
        public static event Action<ExPlayer> OnLeaving;

        public static bool Prefix(ReferenceHub __instance)
        {
            try
            {
                if (__instance.isLocalPlayer || !ExPlayer.TryGet(__instance, out var player))
                    return true;

                OnLeaving.InvokeSafe(player);

                InternalEvents.InternalHandlePlayerLeave(player);
                return true;
            }
            catch (Exception ex)
            {
                ApiLoader.Error("Extended API", $"An error occured while handling player leave!\n{ex.ToColoredString()}");
                return true;
            }
        }
    }
}