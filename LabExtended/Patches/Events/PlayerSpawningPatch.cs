using HarmonyLib;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.Core;
using LabExtended.Core.Hooking;
using LabExtended.Events;
using LabExtended.Events.Player;
using LabExtended.Extensions;
using LabExtended.Utilities;

using PlayerRoles;

using PluginAPI.Events;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.ServerSetRole))]
    public static class PlayerSpawningPatch
    {
        static PlayerSpawningPatch()
            => EventUtils<PlayerRoleManager.ServerRoleSet>.DefineEvent(typeof(PlayerRoleManager), "OnServerRoleSet");

        public static bool Prefix(PlayerRoleManager __instance, RoleTypeId newRole, RoleChangeReason reason, RoleSpawnFlags spawnFlags)
        {
            try
            {
                var player = ExPlayer.Get(__instance.Hub);

                if (player is null)
                    return true;

                EventManager.ExecuteEvent(new PlayerChangeRoleEvent(__instance.Hub, __instance.CurrentRole, newRole, reason));

                var spawningEv = new PlayerSpawningArgs(player, __instance.CurrentRole, newRole, reason, spawnFlags);

                spawningEv.IsAllowed = true;

                foreach (var item in player.Inventory.CustomItems)
                    item.OnOwnerSpawning(spawningEv);

                if ((player.Switches.CanChangeRoles && HookRunner.RunCancellable(spawningEv, true))
                    || (!__instance._anySet || (newRole is RoleTypeId.None && reason is RoleChangeReason.Destroyed))
                    || __instance.isLocalPlayer)
                {
                    if (!player.Stats.KeepMaxHealthOnRoleChange)
                        player.Stats._maxHealthOverride.ClearValue();

                    if (!player.Position.FakedList.KeepOnRoleChange || (!player.Position.FakedList.KeepOnDeath && newRole is RoleTypeId.Spectator && reason is RoleChangeReason.Died))
                        player.Position.FakedList.ClearValues();

                    if (!player.Role.FakedList.KeepOnRoleChange || (!player.Role.FakedList.KeepOnDeath && newRole is RoleTypeId.Spectator && reason is RoleChangeReason.Died))
                        player.Role.FakedList.ClearValues();

                    newRole = spawningEv.NewRole;
                    reason = spawningEv.ChangeReason;
                    spawnFlags = spawningEv.SpawnFlags;

                    InternalEvents.InternalHandleRoleChange(spawningEv);

                    EventUtils<PlayerRoleManager.ServerRoleSet>.InvokeEvent(typeof(PlayerRoleManager), "OnServerRoleSet", null, player.Hub, newRole, reason);

                    __instance.InitializeNewRole(newRole, reason, spawnFlags);
                }

                return false;
            }
            catch (Exception ex)
            {
                ApiLoader.Error("PlayerSpawningPatch", ex);
                return true;
            }
        }
    }
}
