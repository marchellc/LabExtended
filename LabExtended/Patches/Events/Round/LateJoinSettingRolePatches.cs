using CentralAuth;

using GameCore;

using HarmonyLib;

using LabExtended.API;

using LabExtended.Events;
using LabExtended.Events.Round;

using PlayerRoles;
using PlayerRoles.RoleAssign;

namespace LabExtended.Patches.Events.Round
{
    /// <summary>
    /// Implements the <see cref="ExRoundEvents.LateJoinSettingRole"/> and <see cref="ExRoundEvents.LateJoinSetRole"/> events.
    /// </summary>
    public static class LateJoinSettingRolePatches
    {
        [HarmonyPatch(typeof(RoleAssigner), nameof(RoleAssigner.CheckLateJoin))]
        private static bool Prefix(ReferenceHub hub, ClientInstanceMode cim)
        {
            if (!ExPlayer.TryGet(hub, out var player)
                || !RoleAssigner.CheckPlayer(hub)
                || !RoleAssigner._spawned)
                return false;

            var lateJoinTime = ConfigFile.ServerConfig.GetFloat("late_join_time", 0f);
            var lateJoinRole = RoleTypeId.None;

            if (!RoleAssigner.AlreadySpawnedPlayers.Add(player.UserId)
                || RoleAssigner.LateJoinTimer.Elapsed.TotalSeconds > lateJoinTime)
                lateJoinRole = RoleTypeId.Spectator;
            else
                lateJoinRole = HumanSpawner.NextHumanRoleToSpawn;

            var settingEventArgs = new LateJoinSettingRoleEventArgs(player, lateJoinRole, RoleSpawnFlags.All);

            if (!ExRoundEvents.OnLateJoinSettingRole(settingEventArgs)
                || settingEventArgs.Role is RoleTypeId.None)
                return false;

            player.Role.LateJoinRole = settingEventArgs.Role;
            player.Role.Set(settingEventArgs.Role, RoleChangeReason.LateJoin, settingEventArgs.Flags);

            ExRoundEvents.OnLateJoinSetRole(new LateJoinSetRoleEventArgs(player, settingEventArgs.Role, settingEventArgs.Flags));

            return false;
        }
    }
}