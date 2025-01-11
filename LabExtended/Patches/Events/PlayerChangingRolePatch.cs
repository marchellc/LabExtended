using HarmonyLib;

using PlayerRoles;

using Mirror;

using UnityEngine;

using PlayerRoles.SpawnData;

using CustomPlayerEffects;

using LabExtended.Events.Player;
using LabExtended.Core.Hooking;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Attributes;
using LabExtended.Events;
using LabExtended.Extensions;

using PlayerRoles.FirstPersonControl.Spawnpoints;

namespace LabExtended.Patches.Events
{
    public static class PlayerChangingRolePatch
    {
        public static FastEvent<PlayerRoleManager.RoleChanged> OnRoleChanged { get; } =
            FastEvents.DefineEvent<PlayerRoleManager.RoleChanged>(typeof(PlayerRoleManager),
                nameof(PlayerRoleManager.OnRoleChanged));
        
        [HookPatch(typeof(PlayerChangedRoleArgs))]
        [HookPatch(typeof(PlayerChangingRoleArgs))]
        [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.InitializeNewRole))]
        public static bool Prefix(PlayerRoleManager __instance, RoleTypeId targetId, RoleChangeReason reason, RoleSpawnFlags spawnFlags = RoleSpawnFlags.All, NetworkReader data = null)
        {
            try
            {
                if (!ExPlayer.TryGet(__instance.Hub, out var player))
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
                
                var changingEv = new PlayerChangingRoleArgs(player, prevRole, newRole, reason, spawnFlags, data);

                HookRunner.RunEvent(changingEv);

                reason = changingEv.ChangeReason;
                spawnFlags = changingEv.SpawnFlags;

                newRole.Init(__instance.Hub, reason, spawnFlags);

                RoleSpawnpointManager.SetPosition(player.Hub, newRole);

                newRole.SetupPoolObject();
                
                if (__instance.CurrentRole is ISpawnDataReader currentRole && data != null && targetId != RoleTypeId.Spectator 
                    && !__instance.isLocalPlayer)
                    currentRole.ReadSpawnData(data);

                var hasSpawnProtection = false;

                if (changingEv.GiveSpawnProtection)
                    hasSpawnProtection = SpawnProtected.TryGiveProtection(__instance.Hub);

                var changedArgs = new PlayerChangedRoleArgs(player, prevRole, newRole, reason, spawnFlags, data, hasSpawnProtection);

                InternalEvents.InternalHandleRoleChange(changedArgs);
                HookRunner.RunEvent(changedArgs);

                if (wasSet)
                    OnRoleChanged.InvokeEvent(null, player.Hub, prevRole, newRole);

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