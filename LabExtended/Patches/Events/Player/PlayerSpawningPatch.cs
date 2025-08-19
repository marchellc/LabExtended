using HarmonyLib;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Extensions;

using PlayerRoles;

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements the functionality of <see cref="SwitchContainer.CanChangeRoles"/>.
/// </summary>
public static class PlayerSpawningPatch
{
    private static FastEvent<PlayerRoleManager.ServerRoleSet> OnServerRoleSet { get; } =
        FastEvents.DefineEvent<PlayerRoleManager.ServerRoleSet>(typeof(PlayerRoleManager),
            nameof(PlayerRoleManager.OnServerRoleSet));

    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.ServerSetRole))]
    private static bool Prefix(PlayerRoleManager __instance, RoleTypeId newRole, RoleChangeReason reason,
        RoleSpawnFlags spawnFlags, Action<PlayerRoleBase> beforeSendAction)
    {
        try
        {
            var player = ExPlayer.Get(__instance.Hub);

            if (player is null)
                return true;

            var changingArgs = new PlayerChangingRoleEventArgs(player.ReferenceHub, __instance.CurrentRole, newRole,
                reason, spawnFlags);

            PlayerEvents.OnChangingRole(changingArgs);

            if (!changingArgs.IsAllowed)
                return false;

            var curRole = __instance.CurrentRole;

            if (player.Toggles.CanChangeRoles
                || (!__instance._anySet || (changingArgs.NewRole is RoleTypeId.None
                                            && changingArgs.ChangeReason is RoleChangeReason.Destroyed))
                || __instance.isLocalPlayer)
            {
                var clearPositionValues = !player.Position.FakedList.KeepOnRoleChange ||
                                          (!player.Position.FakedList.KeepOnDeath &&
                                           changingArgs.NewRole is RoleTypeId.Spectator &&
                                           changingArgs.ChangeReason is RoleChangeReason.Died);
                var clearRoleValues = !player.Role.FakedList.KeepOnRoleChange || (!player.Role.FakedList.KeepOnDeath &&
                    changingArgs.NewRole is RoleTypeId.Spectator && changingArgs.ChangeReason is RoleChangeReason.Died);

                player.Position.FakedList.ClearValues(clearPositionValues, !player.Position.FakedList.KeepGlobalOnRoleChange);
                player.Role.FakedList.ClearValues(clearRoleValues, !player.Role.FakedList.KeepGlobalOnRoleChange);

                newRole = changingArgs.NewRole;
                reason = changingArgs.ChangeReason;
                spawnFlags = changingArgs.SpawnFlags;

                OnServerRoleSet.InvokeEvent(null!, player.ReferenceHub, newRole, reason);

                __instance.InitializeNewRole(newRole, reason, spawnFlags);
                
                beforeSendAction?.InvokeSafe(__instance.CurrentRole);
                
                if (RoleSync.IsEnabled)
                    RoleSync.Internal_Resync(player);
                else
                    __instance.SendNewRoleInfo();
                
                if (PositionSync.IsEnabled)
                    PositionSync.Internal_Reset(player);

                PlayerEvents.OnChangedRole(new PlayerChangedRoleEventArgs(player.ReferenceHub,
                    curRole?.RoleTypeId ?? RoleTypeId.None,
                    __instance.CurrentRole, reason, spawnFlags));
            }

            return false;
        }
        catch (Exception ex)
        {
            ApiLog.Error("PlayerSpawningPatch", ex);
            return true;
        }
    }
}
