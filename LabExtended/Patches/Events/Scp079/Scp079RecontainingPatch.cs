﻿using HarmonyLib;

using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Events;
using LabExtended.Events.Scp079;

using MapGeneration;

using NorthwoodLib.Pools;

using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;

using PlayerStatsSystem;

namespace LabExtended.Patches.Events.Scp079
{
    public static class Scp079RecontainingPatch
    {
        [EventPatch(typeof(Scp079RecontainingEventArgs))]
        [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.BeginOvercharge))]
        public static bool Prefix(Scp079Recontainer __instance)
        {
            var list = ListPool<ExPlayer>.Shared.Rent();

            list.AddRange(ExPlayer.Get(x => x.Role.Is(RoleTypeId.Scp079) && x.Toggles.CanBeRecontainedAs079));

            var recontainingArgs = new Scp079RecontainingEventArgs(ExPlayer.Get(__instance._activatorGlass.LastAttacker), list);

            if (!ExScp079Events.OnRecontaining(recontainingArgs))
            {
                __instance._alreadyRecontained = !recontainingArgs.PlayAnnouncement;
                return false;
            }

            ExRound.IsScp079Recontained = true;

            foreach (var player in list)
            {
                if (recontainingArgs.Activator is not null)
                    player.ReferenceHub.playerStats.DealDamage(new RecontainmentDamageHandler(recontainingArgs.Activator.Footprint));
                else
                    player.ReferenceHub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Recontained));
            }

            ListPool<ExPlayer>.Shared.Return(list);

            if (recontainingArgs.LockDoors)
            {
                foreach (var colliderPair in InteractableCollider.AllInstances)
                {
                    if (colliderPair.Key is not BasicDoor basicDoor || basicDoor == null)
                        continue;

                    if (basicDoor.RequiredPermissions.RequiredPermissions != KeycardPermissions.None)
                        continue;

                    if (!RoomIdentifier.RoomsByCoordinates.TryGetValue(RoomUtils.PositionToCoords(basicDoor.transform.position), out var room))
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

                    if (!RoomIdentifier.RoomsByCoordinates.TryGetValue(RoomUtils.PositionToCoords(light.transform.position), out var room))
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
