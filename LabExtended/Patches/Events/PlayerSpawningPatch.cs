using HarmonyLib;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.CustomRoles;
using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Hooking;

using LabExtended.Events;
using LabExtended.Events.Player;

using LabExtended.Utilities;

using PlayerRoles;

namespace LabExtended.Patches.Events
{
    public static class PlayerSpawningPatch
    {
        public static FastEvent<PlayerRoleManager.ServerRoleSet> OnServerRoleSet { get; } =
            FastEvents.DefineEvent<PlayerRoleManager.ServerRoleSet>(typeof(PlayerRoleManager),
                nameof(PlayerRoleManager.OnServerRoleSet));

        [HookPatch(typeof(PlayerSpawningArgs))]
        [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.ServerSetRole))]
        public static bool Prefix(PlayerRoleManager __instance, RoleTypeId newRole, RoleChangeReason reason, RoleSpawnFlags spawnFlags)
        {
            try
            {
                var player = ExPlayer.Get(__instance.Hub);

                if (player is null)
                    return true;

                var changingArgs = new PlayerChangingRoleEventArgs(player.Hub, __instance.CurrentRole, newRole, reason);

                PlayerEvents.OnChangingRole(changingArgs);

                if (!changingArgs.IsAllowed)
                    return false;

                var spawningEv = new PlayerSpawningArgs(player, __instance.CurrentRole, changingArgs.NewRole, changingArgs.ChangeReason, spawnFlags);
                var curRole = __instance.CurrentRole;
                
                if ((player.Switches.CanChangeRoles && HookRunner.RunEvent(spawningEv, true))
                    || (!__instance._anySet || (spawningEv.NewRole is RoleTypeId.None && spawningEv.ChangeReason is RoleChangeReason.Destroyed))
                    || __instance.isLocalPlayer)
                {
                    foreach (var customRole in CustomRole.GetRoles(player))
                    {
                        if (!customRole.IsEnabled)
                        {
                            if (customRole.EnableRole(spawningEv.ChangeReason, player.Role, spawningEv.NewRole))
                                customRole.InternalEnable();
                            else
                                continue;
                        }
                        else if (!customRole.EnableRole(spawningEv.ChangeReason, player.Role, spawningEv.NewRole))
                        {
                            customRole.InternalDisable();
                        }

                        customRole.OnSpawning(spawningEv);
                        
                        if (!spawningEv.IsAllowed)
                            return false;
                    }
                    
                    player.Stats._healthOverride = null;

                    if (!player.Position.FakedList.KeepOnRoleChange || (!player.Position.FakedList.KeepOnDeath && spawningEv.NewRole is RoleTypeId.Spectator && spawningEv.ChangeReason is RoleChangeReason.Died))
                        player.Position.FakedList.ClearValues();

                    if (!player.Role.FakedList.KeepOnRoleChange || (!player.Role.FakedList.KeepOnDeath && spawningEv.NewRole is RoleTypeId.Spectator && spawningEv.ChangeReason is RoleChangeReason.Died))
                        player.Role.FakedList.ClearValues();
                    
                    newRole = spawningEv.NewRole;
                    reason = spawningEv.ChangeReason;
                    spawnFlags = spawningEv.SpawnFlags;

                    InternalEvents.InternalHandleRoleChange(spawningEv);

                    OnServerRoleSet.InvokeEvent(null, player.Hub, newRole, reason);

                    __instance.InitializeNewRole(newRole, reason, spawnFlags);
                    __instance._sendNextFrame = true;

                    PlayerEvents.OnChangedRole(new PlayerChangedRoleEventArgs(player.Hub, curRole, newRole, reason));
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
