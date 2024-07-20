using CustomPlayerEffects;

using HarmonyLib;

using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Firearms.Modules;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Other;
using LabExtended.Events.Player;

using Mirror;

using PlayerRoles.FirstPersonControl;

using PlayerStatsSystem;

using PluginAPI.Events;

using RelativePositioning;

using UnityEngine;

using Utils.Networking;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    public static class ShootingPatch
    {
        public static readonly LayerMask DisruptorMask = LayerMask.GetMask("Default", "Hitbox", "CCTV", "Door", "InteractableNoPlayerCollision");

        public static bool Prefix(NetworkConnection conn, ShotMessage msg)
        {
            if (!ExPlayer.TryGet(conn, out var player))
                return false;

            if (msg.ShooterWeaponSerial != player.CurrentItemIdentifier.SerialNumber)
                return false;

            if (player.CurrentItem is null || player.CurrentItem is not Firearm firearm)
                return false;

            var shootingArgs = new PlayerShootingArgs(player, firearm, msg);

            if (!HookRunner.RunCancellable(shootingArgs, true))
                return false;

            msg = shootingArgs.Message;

            if (!AuthorizingShotPatch.AuthorizeShot(player, firearm, msg))
                return false;

            if (SpawnProtected.CheckPlayer(player.Hub) && !SpawnProtected.CanShoot)
                player.DisableEffect<SpawnProtected>();

            InternalProcessShot(player, firearm, msg);
            return false;
        }

        private static void InternalProcessShot(ExPlayer player, Firearm firearm, ShotMessage msg)
        {
            if (firearm.HitregModule is null || firearm.HitregModule is not StandardHitregBase standardHitreg)
                return;

            if (!WaypointBase.TryGetWaypoint(msg.ShooterPosition.WaypointId, out var shooterWaypoint))
                return;

            standardHitreg.SetHitboxes(player.Hub, false);

            var worldPosition = shooterWaypoint.GetWorldspacePosition(msg.ShooterPosition.Relative);
            var worldRotation = shooterWaypoint.GetWorldspaceRotation(msg.ShooterCameraRotation);

            using (var backtracker = new FpcBacktracker(player.Hub, worldPosition, worldRotation))
            {
                var isPlayer = ExPlayer.TryGet(msg.TargetNetId, out var targetPlayer);

                var transform = default(Transform);
                var rotation = default(Quaternion);
                var tracker = default(FpcBacktracker);

                if (isPlayer && WaypointBase.TryGetWaypoint(msg.TargetPosition.WaypointId, out var targetWaypoint))
                {
                    transform = player.Transform;
                    rotation = player.Transform.rotation;

                    tracker = new FpcBacktracker(targetPlayer.Hub, targetWaypoint.GetWorldspacePosition(msg.TargetPosition.Relative));

                    targetPlayer.Transform.rotation = targetWaypoint.GetWorldspaceRotation(msg.TargetRotation);

                    if (targetPlayer.Hub.isLocalPlayer)
                        standardHitreg.SetHitboxes(targetPlayer.Hub, true);
                }

                var ray = new Ray(player.Camera.position, player.Camera.forward);
                var processingArgs = new ProcessingFirearmShotArgs(player, firearm, ray);

                standardHitreg.PrimaryTargetNetId = msg.TargetNetId;
                standardHitreg.SetHitboxes(player.Hub, !player.Hub.isLocalPlayer);

                if (!HookRunner.RunCancellable(processingArgs, true))
                {
                    if (isPlayer)
                    {
                        tracker.RestorePosition();
                        transform.rotation = rotation;
                    }

                    return;
                }

                ray = processingArgs.Ray;

                switch (firearm.HitregModule)
                {
                    case MultiShotHitreg multiShotHitreg:
                        MultiShotPerformShot(player, firearm, ray, multiShotHitreg);
                        break;

                    case SingleBulletHitreg singleBulletHitreg:
                        SingleBulletPerformShot(player, firearm, ray, singleBulletHitreg);
                        break;

                    case DisruptorHitreg disruptorHitreg:
                        DisruptorPerformShot(player, firearm, ray, disruptorHitreg);
                        break;

                    case BuckshotHitreg buckshotHitreg:
                        BuckshotPerformShot(player, firearm, ray, buckshotHitreg);
                        break;
                }

                if (isPlayer)
                {
                    tracker.RestorePosition();
                    transform.rotation = rotation;
                }
            }

            firearm.OnWeaponShot();
        }

        private static void BuckshotPerformShot(ExPlayer player, Firearm firearm, Ray ray, BuckshotHitreg buckshotHitreg)
        {
            var grounded = player.Role.MovementModule?.IsGrounded ?? true;
            var velocity = player.Role.Motor?.Velocity ?? Vector3.zero;
            var inaccuracy = firearm.BaseStats.GetInaccuracy(firearm, firearm.AdsModule.ServerAds, velocity.magnitude, grounded) * 0.4f;
            var vector = (new Vector2(UnityEngine.Random.value, UnityEngine.Random.value) - Vector2.one / 2f).normalized * UnityEngine.Random.value * inaccuracy;

            BuckshotHitreg.Hits.Clear();

            for (int i = 0; i < buckshotHitreg.LastFiredAmount; i++)
            {
                foreach (var pellet in buckshotHitreg.CurBuckshotSettings.PredefinedPellets)
                {
                    if (!EventManager.ExecuteEvent(new PlayerShotWeaponEvent(player.Hub, firearm)))
                        continue;

                    var pelletVector = Vector2.Lerp(pellet, buckshotHitreg.GenerateRandomPelletDirection, buckshotHitreg.BuckshotRandomness) * buckshotHitreg.BuckshotScale;
                    var pelletDirection = ray.direction;

                    pelletDirection = Quaternion.AngleAxis(vector.x + vector.x, player.Camera.up) * pelletDirection;
                    pelletDirection = Quaternion.AngleAxis(vector.y + vector.y, player.Camera.right) * pelletDirection;

                    var newRay = new Ray(ray.origin, pelletDirection);

                    if (!Physics.Raycast(newRay, out var hit, firearm.BaseStats.MaxDistance(), StandardHitregBase.HitregMask))
                        continue;

                    if (!hit.collider.TryGetComponent<IDestructible>(out var destructible))
                    {
                        var performingArgs = new PlayerPerformingShotArgs(player, null, firearm, newRay, hit, null, 0f);

                        if (!HookRunner.RunCancellable(performingArgs, true))
                            continue;

                        if (performingArgs.PlaceBulletDecal)
                            buckshotHitreg.PlaceBulletholeDecal(newRay, hit);

                        continue;
                    }

                    if (!buckshotHitreg.CanShoot(destructible))
                        continue;

                    var damage = firearm.BaseStats.DamageAtDistance(firearm, hit.distance) / buckshotHitreg.CurBuckshotSettings.MaxHits;

                    BuckshotHitreg.Hits.Add(new BuckshotHitreg.ShotgunHit(destructible, damage, newRay, hit));
                }
            }

            var num3 = 0f;

            foreach (var hit in BuckshotHitreg.Hits)
            {
                var num = 0f;
                var handler = new FirearmDamageHandler(firearm, hit.Damage, false);
                var hitboxIdentity = default(HitboxIdentity);
                var hasHub = (hitboxIdentity = hit.Target as HitboxIdentity) != null && Hitmarker.CheckHitmarkerPerms(handler, hitboxIdentity.TargetHub);

                var performingArgs = new PlayerPerformingShotArgs(player, (hasHub ? ExPlayer.Get(hitboxIdentity.TargetHub) : null), firearm, hit.RcRay, hit.RcResult, hit.Target, hit.Damage);

                if (!HookRunner.RunCancellable(performingArgs, true))
                    continue;

                if (performingArgs.Damage < -1f || !hit.Target.Damage(performingArgs.Damage, handler, hit.RcResult.point))
                    continue;

                if (performingArgs.PlaceBloodDecal)
                    buckshotHitreg.PlaceBloodDecal(hit.RcRay, hit.RcResult, hit.Target);

                if (!hasHub)
                    num3 += performingArgs.Damage;

                if (performingArgs.ShowIndicator)
                    buckshotHitreg.ShowHitIndicator(hit.Target.NetworkId, hit.Damage, hit.RcRay.origin);
            }
        }

        private static void DisruptorPerformShot(ExPlayer player, Firearm firearm, Ray ray, DisruptorHitreg disruptorHitreg)
        {
            if (!EventManager.ExecuteEvent(new PlayerShotWeaponEvent(player.Hub, firearm)))
                return;

            var grounded = player.Role.MovementModule?.IsGrounded ?? true;
            var velocity = player.Role.Motor?.Velocity ?? Vector3.zero;
            var vector = (new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) - Vector3.one / 2f).normalized * UnityEngine.Random.value;
            var inaccurary = firearm.BaseStats.GetInaccuracy(firearm, firearm.AdsModule.ServerAds, velocity.magnitude, grounded);

            ray.direction = Quaternion.Euler(inaccurary * vector) * ray.direction;

            if (!Physics.Raycast(ray, out var hit, firearm.BaseStats.MaxDistance(), DisruptorMask))
            {
                var performingArgs = new PlayerPerformingShotArgs(player, null, firearm, ray, hit, null, 0f);

                if (!HookRunner.RunCancellable(performingArgs, true))
                    return;

                if (performingArgs.SpawnExplosion)
                    disruptorHitreg.CreateExplosion(hit.point);

                return;
            }

            if (!hit.collider.TryGetComponent<IDestructible>(out var destructible))
            {
                var performingArgs = new PlayerPerformingShotArgs(player, null, firearm, ray, hit, null, 0f);

                if (!HookRunner.RunCancellable(performingArgs, true))
                    return;

                new DisruptorHitreg.DisruptorHitMessage
                {
                    Position = hit.point + hit.normal * 0.1f,
                    Rotation = new LowPrecisionQuaternion(Quaternion.LookRotation(-hit.normal))
                }.SendToAuthenticated();

                if (performingArgs.SpawnExplosion)
                    disruptorHitreg.CreateExplosion(hit.point);
            }
            else
            {
                var damage = firearm.BaseStats.DamageAtDistance(firearm, hit.distance);
                var handler = new DisruptorDamageHandler(player.Footprint, damage);

                var performingArgs = new PlayerPerformingShotArgs(player, ExPlayer.Get(destructible.NetworkId), firearm, ray, hit, destructible, damage);

                if (!HookRunner.RunCancellable(performingArgs, true))
                    return;

                if (destructible.Damage(performingArgs.Damage, handler, hit.point))
                {
                    if (performingArgs.ShowIndicator)
                    {
                        if (destructible is HitboxIdentity hitboxIdentity)
                            Hitmarker.SendHitmarkerConditionally(2f, handler, hitboxIdentity.TargetHub);
                        else
                            Hitmarker.SendHitmarkerDirectly(player.Connection, 2f);
                    }
                }

                if (performingArgs.ShowIndicator)
                    disruptorHitreg.ShowHitIndicator(destructible.NetworkId, damage, ray.origin);
            }
        }

        private static void MultiShotPerformShot(ExPlayer player, Firearm firearm, Ray ray, MultiShotHitreg multiShotHitreg)
        {
            if (!EventManager.ExecuteEvent(new PlayerShotWeaponEvent(player.Hub, firearm)))
                return;

            ray = multiShotHitreg.ServerRandomizeRay(ray);
            multiShotHitreg._offsets.ForEach(offset =>
            {
                var newRay = new Ray(ray.origin + offset, ray.direction);

                if (!Physics.Raycast(newRay, out var hit, firearm.BaseStats.MaxDistance(), StandardHitregBase.HitregMask))
                    return;

                SingleBulletProcessHit(player, firearm, newRay, hit, multiShotHitreg);
            });
        }

        private static void SingleBulletPerformShot(ExPlayer player, Firearm firearm, Ray ray, SingleBulletHitreg singleBulletHitreg)
        {
            if (!EventManager.ExecuteEvent(new PlayerShotWeaponEvent(player.Hub, firearm)))
                return;

            ray = singleBulletHitreg.ServerRandomizeRay(ray);

            if (Physics.Raycast(ray, out var hit, firearm.BaseStats.MaxDistance(), StandardHitregBase.HitregMask))
                SingleBulletProcessHit(player, firearm, ray, hit, singleBulletHitreg);
        }

        private static void SingleBulletProcessHit(ExPlayer player, Firearm firearm, Ray ray, RaycastHit hit, SingleBulletHitreg singleBulletHitreg)
        {
            if (hit.collider.TryGetComponent<IDestructible>(out var destructible) && singleBulletHitreg.CheckInaccurateFriendlyFire(destructible))
            {
                var damage = firearm.BaseStats.DamageAtDistance(firearm, hit.distance);
                var handler = new FirearmDamageHandler(firearm, damage);

                var performingArgs = new PlayerPerformingShotArgs(player, ExPlayer.Get(destructible.NetworkId), firearm, ray, hit, destructible, damage);

                if (!HookRunner.RunCancellable(performingArgs, true))
                    return;

                if (destructible.Damage(performingArgs.Damage, handler, hit.point))
                {
                    if (performingArgs.ShowIndicator)
                    {
                        if (destructible is HitboxIdentity hitboxIdentity)
                            Hitmarker.SendHitmarkerConditionally(1f, handler, hitboxIdentity.TargetHub);
                        else
                            Hitmarker.SendHitmarkerDirectly(player.Connection, 1f);

                        singleBulletHitreg.ShowHitIndicator(destructible.NetworkId, damage, ray.origin);
                    }

                    if (performingArgs.PlaceBloodDecal)
                        singleBulletHitreg.PlaceBloodDecal(ray, hit, destructible);
                }
            }
            else
            {
                var performingArgs = new PlayerPerformingShotArgs(player, null, firearm, ray, hit, null, 0f);

                if (!HookRunner.RunCancellable(performingArgs, true))
                    return;

                if (performingArgs.PlaceBulletDecal)
                    singleBulletHitreg.PlaceBulletholeDecal(ray, hit);
            }
        }
    }
}