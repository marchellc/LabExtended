using HarmonyLib;

using Interactables.Interobjects.DoorUtils;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Utilities;

using PlayerRoles;

using PluginAPI.Events;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.ServerInteract))]
    public static class DoorInteractingPatch
    {
        public static bool Prefix(DoorVariant __instance, ReferenceHub ply, byte colliderId)
        {
            if (!ExPlayer.TryGet(ply, out var player))
                return true;

            if (!ArgumentUtils.TryGet(() => ExMap.GetDoor(__instance), out var door))
                return true;

            if (__instance.ActiveLocks > 0 && !ply.serverRoles.BypassMode)
            {
                var mode = DoorLockUtils.GetMode((DoorLockReason)__instance.ActiveLocks);

                if ((!mode.HasFlagFast(DoorLockMode.CanClose) || !mode.HasFlagFast(DoorLockMode.CanOpen))
                    && (!mode.HasFlagFast(DoorLockMode.ScpOverride) || !ply.IsSCP(true)) && (mode == DoorLockMode.FullLock
                    || (__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanClose)) || (!__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanOpen))))
                {
                    if (!EventManager.ExecuteEvent(new PlayerInteractDoorEvent(ply, __instance, false)))
                        return false;

                    if (!HookRunner.RunCancellable(new PlayerInteractingDoorArgs(player, door, false), true))
                        return false;

                    __instance.LockBypassDenied(ply, colliderId);
                    return false;
                }
            }

            if (!__instance.AllowInteracting(ply, colliderId))
                return false;

            var canOpen = player.Role.Is(RoleTypeId.Scp079) || __instance.RequiredPermissions.CheckPermissions(ply.inventory.CurInstance, ply);
            var interactEvent = new PlayerInteractDoorEvent(ply, __instance, canOpen);

            if (!EventManager.ExecuteEvent(interactEvent))
                return false;

            var interactingArgs = new PlayerInteractingDoorArgs(player, door, interactEvent.CanOpen);

            if (!HookRunner.RunCancellable(interactingArgs, true))
                return false;

            if (interactingArgs.CanOpen)
            {
                __instance.NetworkTargetState = !__instance.TargetState;
                __instance._triggerPlayer = ply;

                if (__instance.NetworkTargetState)
                    DoorEvents.TriggerAction(__instance, DoorAction.Opened, ply);
                else
                    DoorEvents.TriggerAction(__instance, DoorAction.Closed, ply);

                return false;
            }

            __instance.PermissionsDenied(ply, colliderId);

            DoorEvents.TriggerAction(__instance, DoorAction.AccessDenied, ply);
            return false;
        }
    }
}