﻿using HarmonyLib;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Utilities;

using PlayerRoles;

namespace LabExtended.Patches.Events.Player
{
    public static class PlayerSpawningPatch
    {
        public static FastEvent<PlayerRoleManager.ServerRoleSet> OnServerRoleSet { get; } =
            FastEvents.DefineEvent<PlayerRoleManager.ServerRoleSet>(typeof(PlayerRoleManager),
                nameof(PlayerRoleManager.OnServerRoleSet));
        
        [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.ServerSetRole))]
        public static bool Prefix(PlayerRoleManager __instance, RoleTypeId newRole, RoleChangeReason reason, RoleSpawnFlags spawnFlags)
        {
            try
            {
                var player = ExPlayer.Get(__instance.Hub);

                if (player is null)
                    return true;

                var changingArgs = new PlayerChangingRoleEventArgs(player.ReferenceHub, __instance.CurrentRole, newRole, reason);

                PlayerEvents.OnChangingRole(changingArgs);

                if (!changingArgs.IsAllowed)
                    return false;
                
                var curRole = __instance.CurrentRole;
                
                if (player.Toggles.CanChangeRoles
                    || (!__instance._anySet || (changingArgs.NewRole is RoleTypeId.None 
                                                && changingArgs.ChangeReason is RoleChangeReason.Destroyed))
                    || __instance.isLocalPlayer)
                {
                    bool clearPositionValues = !player.Position.FakedList.KeepOnRoleChange || (!player.Position.FakedList.KeepOnDeath &&
                        changingArgs.NewRole is RoleTypeId.Spectator && changingArgs.ChangeReason is RoleChangeReason.Died);
                    player.Position.FakedList.ClearValues(clearPositionValues, !player.Position.FakedList.KeepGlobalOnRoleChange);

                    bool clearRoleValues = !player.Role.FakedList.KeepOnRoleChange || (!player.Role.FakedList.KeepOnDeath &&
                        changingArgs.NewRole is RoleTypeId.Spectator && changingArgs.ChangeReason is RoleChangeReason.Died);
                    player.Role.FakedList.ClearValues(clearRoleValues, !player.Role.FakedList.KeepGlobalOnRoleChange);
                    
                    newRole = changingArgs.NewRole;
                    reason = changingArgs.ChangeReason;

                    OnServerRoleSet.InvokeEvent(null, player.ReferenceHub, newRole, reason);

                    __instance.InitializeNewRole(newRole, reason, spawnFlags);
                    __instance._sendNextFrame = true;

                    PlayerEvents.OnChangedRole(new PlayerChangedRoleEventArgs(player.ReferenceHub, curRole, newRole, reason));
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
}
