using HarmonyLib;

using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Spawnpoints;

using LabApi.Events.Handlers;
using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.API;
using LabExtended.API.CustomRoles;

using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Events;

public static class PlayerSelectingSpawnPositionPatch
{
    [HookPatch(typeof(PlayerSelectingSpawnPositionArgs), true)]
    [HarmonyPatch(typeof(RoleSpawnpointManager), nameof(RoleSpawnpointManager.SetPosition))]
    public static bool Prefix(ReferenceHub hub, PlayerRoleBase newRole)
    {
        if (newRole is not IFpcRole fpcRole || fpcRole.SpawnpointHandler is null)
            return false;

        if (!ExPlayer.TryGet(hub, out var player))
            return false;

        if (!fpcRole.SpawnpointHandler.TryGetSpawnpoint(out var spawnPoint, out var spawnRot))
            return false;
        
        var selectingPositionArgs = new PlayerSelectingSpawnPositionArgs(player, spawnPoint, spawnRot);

        if (!HookRunner.RunEvent(selectingPositionArgs, true))
            return false;

        foreach (var customRole in CustomRole.GetRoles(player))
        {
            if (!customRole.IsEnabled)
                continue;
            
            customRole.OnSelectingPosition(selectingPositionArgs);

            if (!selectingPositionArgs.IsAllowed)
                return false;
        }

        var spawningArgs = new PlayerSpawningEventArgs(hub, newRole, true, selectingPositionArgs.Position, selectingPositionArgs.Rotation);
        
        PlayerEvents.OnSpawning(spawningArgs);

        if (!spawningArgs.IsAllowed)
            return false;

        if (!newRole.ServerSpawnFlags.HasFlag(RoleSpawnFlags.UseSpawnpoint))
        {
            PlayerEvents.OnSpawned(new PlayerSpawnedEventArgs(hub, newRole, true, spawningArgs.SpawnLocation, spawningArgs.HorizontalRotation));
            return false;
        }

        hub.transform.position = spawningArgs.SpawnLocation;
        fpcRole.FpcModule.MouseLook!.CurrentHorizontal = spawningArgs.HorizontalRotation;

        PlayerEvents.OnSpawned(new PlayerSpawnedEventArgs(hub, newRole, true, spawningArgs.SpawnLocation, spawningArgs.HorizontalRotation));
        return false;
    }
}