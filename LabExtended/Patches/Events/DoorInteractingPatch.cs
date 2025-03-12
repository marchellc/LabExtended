using HarmonyLib;

using Interactables.Interobjects.DoorUtils;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

using PlayerRoles;

namespace LabExtended.Patches.Events
{
    public static class DoorInteractingPatch
    {
        [HookPatch(typeof(PlayerInteractingDoorArgs))]
        [HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.ServerInteract))]
        public static bool Prefix(DoorVariant __instance, ReferenceHub ply, byte colliderId)
        {
            if (!ExPlayer.TryGet(ply, out var player))
                return true;

            if (!Door.TryGet(__instance, out var door))
                return true;
            
            if (!__instance.AllowInteracting(ply, colliderId))
                return false;

            if (__instance.ActiveLocks > 0 && !ply.serverRoles.BypassMode)
            {
                var mode = DoorLockUtils.GetMode((DoorLockReason)__instance.ActiveLocks);

                if ((!mode.HasFlagFast(DoorLockMode.CanClose) || !mode.HasFlagFast(DoorLockMode.CanOpen))
                    && (!mode.HasFlagFast(DoorLockMode.ScpOverride) || !ply.IsSCP(true)) && (mode == DoorLockMode.FullLock
                    || (__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanClose)) || (!__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanOpen))))
                {
                    var interactingEventArgs = new PlayerInteractingDoorEventArgs(player.ReferenceHub, __instance, false);

                    PlayerEvents.OnInteractingDoor(interactingEventArgs);

                    if (!interactingEventArgs.IsAllowed)
                        return false;

                    var interactingArgs = new PlayerInteractingDoorArgs(player, door, interactingEventArgs.CanOpen);

                    if (!HookRunner.RunEvent(interactingArgs, true))
                        return false;

                    if (!interactingArgs.CanOpen)
                    {
                        __instance.LockBypassDenied(ply, colliderId);
                        return false;
                    }
                }
            }

            var canOpen = player.Role.Is(RoleTypeId.Scp079) || __instance.RequiredPermissions.CheckPermissions(ply.inventory.CurInstance, ply);
            
            var interactingDoorEvArgs = new PlayerInteractingDoorEventArgs(player.ReferenceHub, __instance, canOpen);

            PlayerEvents.OnInteractingDoor(interactingDoorEvArgs);

            if (!interactingDoorEvArgs.IsAllowed)
                return false;
            
            var interactingDoorArgs = new PlayerInteractingDoorArgs(player, door, interactingDoorEvArgs.CanOpen);

            if (!HookRunner.RunEvent(interactingDoorArgs, true))
                return false;

            if (interactingDoorArgs.CanOpen)
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