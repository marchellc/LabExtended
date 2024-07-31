using HarmonyLib;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Map;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdUsePanel__AlphaPanelOperations))]
    public static class WarheadChangingLeverPatch
    {
        public static bool Prefix(PlayerInteract __instance, PlayerInteract.AlphaPanelOperations n)
        {
            if (!__instance.CanInteract)
                return false;

            if (!ExPlayer.TryGet(__instance._hub, out var player))
                return true;

            var nukesidePanel = AlphaWarheadOutsitePanel.nukeside;

            if (nukesidePanel is null)
                return false;

            if (!__instance.ChckDis(nukesidePanel.transform.position))
                return false;

            if (n is PlayerInteract.AlphaPanelOperations.Cancel)
            {
                __instance.OnInteract();
                AlphaWarheadController.Singleton?.CancelDetonation(__instance._hub);
                ServerLogs.AddLog(ServerLogs.Modules.Warhead, __instance._hub.LoggedNameFromRefHub() + " cancelled the Alpha Warhead detonation.", ServerLogs.ServerLogType.GameEvent);
                return false;
            }
            else
            {
                if (nukesidePanel.AllowChangeLevelState())
                {
                    var changingArgs = new WarheadChangingLeverArgs(nukesidePanel.Networkenabled, player);

                    if (!HookRunner.RunCancellable(changingArgs, true) || changingArgs.NewState == nukesidePanel.Networkenabled)
                        return false;

                    __instance.OnInteract();

                    nukesidePanel.Networkenabled = changingArgs.NewState;
                }
            }

            return false;
        }
    }
}