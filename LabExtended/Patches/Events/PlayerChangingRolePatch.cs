using HarmonyLib;

using PlayerRoles;

using Mirror;

using LabExtended.API;
using LabExtended.Events.Player;
using LabExtended.Core.Hooking;

using UnityEngine;

using PlayerRoles.SpawnData;
using PluginAPI.Events;

using CustomPlayerEffects;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities;

using PlayerRoles.FirstPersonControl.Spawnpoints;
using LabExtended.Attributes;

namespace LabExtended.Patches.Events
{
    public static class PlayerChangingRolePatch
    {
        static PlayerChangingRolePatch()
            => FastEvents<PlayerRoleManager.RoleChanged>.DefineEvent(typeof(PlayerRoleManager), "OnRoleChanged");

        [HookPatch(typeof(PlayerSpawnEvent))]
        [HookPatch(typeof(PlayerChangedRoleArgs))]
        [HookPatch(typeof(PlayerChangingRoleArgs))]
        [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.InitializeNewRole))]
        public static bool Prefix(PlayerRoleManager __instance, RoleTypeId targetId, RoleChangeReason reason, RoleSpawnFlags spawnFlags = RoleSpawnFlags.All, NetworkReader data = null)
        {
            try
            {
                var player = ExPlayer.Get(__instance.Hub);

                if (player is null)
                    return true;

                var prevRole = default(PlayerRoleBase);
                var wasSet = false;

                if (__instance._anySet)
                {
                    prevRole = __instance.CurrentRole;
                    prevRole.DisableRole(targetId);

                    wasSet = true;
                }

                var newRole = targetId.GetInstance();

                if (targetId != RoleTypeId.Destroyed || reason != RoleChangeReason.Destroyed)
                {
                    newRole.transform.parent = __instance.transform;
                    newRole.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                }

                __instance.CurrentRole = newRole;

                var dataPos = data?.Position ?? -1;
                var changingEv = new PlayerChangingRoleArgs(player, prevRole, newRole, reason, spawnFlags, data);

                HookRunner.RunEvent(changingEv);

                reason = changingEv.ChangeReason;
                spawnFlags = changingEv.SpawnFlags;

                newRole.Init(__instance.Hub, reason, spawnFlags);

                RoleSpawnpointManager.SetPosition(player.Hub, newRole);

                newRole.SetupPoolObject();

                if (newRole is ISpawnDataReader spawnDataReader && data != null)
                {
                    if (targetId is not RoleTypeId.Spectator && !__instance.isLocalPlayer && EventManager.ExecuteEvent(new PlayerSpawnEvent(__instance.Hub, targetId)))
                        spawnDataReader.ReadSpawnData(data);
                }
                else if (targetId != RoleTypeId.Spectator && !__instance.isLocalPlayer)
                {
                    EventManager.ExecuteEvent(new PlayerSpawnEvent(__instance.Hub, targetId));
                }

                var hasSpawnProtection = false;

                if (changingEv.GiveSpawnProtection)
                    hasSpawnProtection = SpawnProtected.TryGiveProtection(__instance.Hub);

                var changedArgs = new PlayerChangedRoleArgs(player, prevRole, newRole, reason, spawnFlags, data, hasSpawnProtection);

                HookRunner.RunEvent(changedArgs);

                if (wasSet)
                    FastEvents<PlayerRoleManager.RoleChanged>.InvokeEvent(typeof(PlayerRoleManager), "OnRoleChanged", null, player.Hub, prevRole, newRole);

                return false;
            }
            catch (Exception ex)
            {
                ApiLog.Error("PlayerChangingRolePatch", ex);
                return true;
            }
        }
    }
}