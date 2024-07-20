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
                var customItems = CustomItem.GetItems<CustomItem>(player);

                spawningEv.IsAllowed = true;

                foreach (var item in customItems)
                    item.OnOwnerSpawning(spawningEv);

                if ((player.Switches.CanChangeRoles && HookRunner.RunCancellable(spawningEv, true))
                    || (!__instance._anySet || (newRole is RoleTypeId.None && reason is RoleChangeReason.Destroyed))
                    || __instance.isLocalPlayer)
                {
                    if (!player.Stats.KeepMaxHealthOnRoleChange)
                        player.Stats._maxHealthOverride.ClearValue();

                    if (!player.FakePosition.KeepOnRoleChange || (!player.FakePosition.KeepOnDeath && newRole is RoleTypeId.Spectator && reason is RoleChangeReason.Died))
                        player.FakePosition.ClearValues();

                    if (!player.FakeRole.KeepOnRoleChange || (!player.FakeRole.KeepOnDeath && newRole is RoleTypeId.Spectator && reason is RoleChangeReason.Died))
                        player.FakeRole.ClearValues();

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
                ExLoader.Error("PlayerSpawningPatch", ex);
                return true;
            }
        }
    }
}
