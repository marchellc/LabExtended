using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;

using LabExtended.Events;
using LabExtended.Events.Map;

namespace LabExtended.Patches.Events
{
    public static class WarheadChangingLeverPatch
    {
        [EventPatch(typeof(WarheadChangingLeverEventArgs))]
        [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdUsePanel__AlphaPanelOperations))]
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
            }
            else
            {
                if (nukesidePanel.AllowChangeLevelState())
                {
                    var changingArgs = new WarheadChangingLeverEventArgs(nukesidePanel.Networkenabled, player);

                    if (!ExMapEvents.OnWarheadChangingLever(changingArgs) || changingArgs.NewState == nukesidePanel.Networkenabled)
                        return false;

                    __instance.OnInteract();

                    nukesidePanel.Networkenabled = changingArgs.NewState;
                }
            }

            return false;
        }
    }
}