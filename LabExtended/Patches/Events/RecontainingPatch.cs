using HarmonyLib;

using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Core.Hooking;
using LabExtended.Events.Scp079;

using MapGeneration;

using NorthwoodLib.Pools;

using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;

using PlayerStatsSystem;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.BeginOvercharge))]
    public static class RecontainingPatch
    {
        public static bool Prefix(Scp079Recontainer __instance)
        {
            var list = ListPool<ExPlayer>.Shared.Rent();

            list.AddRange(ExPlayer.Get(x => x.Role.Is(RoleTypeId.Scp079) && x.Switches.CanBeRecontainedAs079));

            var recontainingArgs = new Scp079RecontainingArgs(ExPlayer.Get(__instance._activatorGlass.LastAttacker), list);

            if (!HookRunner.RunCancellable(recontainingArgs, true))
            {
                __instance._alreadyRecontained = !recontainingArgs.PlayAnnouncement;
                return false;
            }

            ExRound.IsScp079Recontained = true;

            foreach (var player in list)
            {
                if (recontainingArgs.Activator is not null)
                    player.Hub.playerStats.DealDamage(new RecontainmentDamageHandler(recontainingArgs.Activator.Footprint));
                else
                    player.Hub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Recontained));
            }

            ListPool<ExPlayer>.Shared.Return(list);

            if (recontainingArgs.LockDoors)
            {
                foreach (var colliderPair in InteractableCollider.AllInstances)
                {
                    if (colliderPair.Key is null || colliderPair.Key is not BasicDoor basicDoor || basicDoor == null)
                        continue;

                    if (basicDoor.RequiredPermissions.RequiredPermissions != KeycardPermissions.None)
                        continue;

                    if (!RoomIdentifier.RoomsByCoordinates.TryGetValue(RoomIdUtils.PositionToCoords(basicDoor.transform.position), out var room))
                        continue;

                    if (room.Zone != FacilityZone.HeavyContainment || __instance._containmentGates.Contains(basicDoor))
                        continue;

                    basicDoor.NetworkTargetState = basicDoor.TargetState && AlphaWarheadController.InProgress;
                    basicDoor.ServerChangeLock(DoorLockReason.NoPower, true);

                    __instance._lockedDoors.Add(basicDoor);
                }
            }

            if (recontainingArgs.FlickerLights)
            {
                foreach (var light in RoomLightController.Instances)
                {
                    if (light is null)
                        continue;

                    if (!RoomIdentifier.RoomsByCoordinates.TryGetValue(RoomIdUtils.PositionToCoords(light.transform.position), out var room))
                        continue;

                    if (room.Zone != FacilityZone.HeavyContainment)
                        continue;

                    light.ServerFlickerLights(__instance._lockdownDuration);
                }
            }

            __instance.SetContainmentDoors(true, false);
            return false;
        }
    }
}
