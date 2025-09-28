using InventorySystem.Items;
using InventorySystem.Items.Autosync;

using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using InventorySystem.Items.Firearms.ShotEvents;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Events.Handlers;

using LabExtended.API.CustomItems.Behaviours;

using LabExtended.Events.Firearms;
using LabExtended.Events.Player;

using LabExtended.Extensions;
using LabExtended.Utilities.Firearms;
using NorthwoodLib.Pools;
using PlayerStatsSystem;
using UnityEngine;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.

namespace LabExtended.API.CustomFirearms.Behaviours;

/// <summary>
/// The behaviour of a Custom Firearm while in inventory.
/// </summary>
public class CustomFirearmInventoryBehaviour : CustomItemInventoryBehaviour
{
    /// <summary>
    /// Gets the base firearm.
    /// </summary>
    public new Firearm Item => base.Item as Firearm;
    
    /// <summary>
    /// Gets the custom firearm handler.
    /// </summary>
    public new CustomFirearmHandler Handler => base.Handler as CustomFirearmHandler;
    
    /// <summary>
    /// Gets the target firearm's module cache.
    /// </summary>
    public FirearmModuleCache Modules { get; internal set; }

    /// <summary>
    /// Gets the amount of ammo available to load into the firearm.
    /// </summary>
    public int AvailableInventoryAmmo
    {
        get
        {
            if (Handler.UsesCustomAmmo)
                return Player.Ammo.GetCustomAmmo(Handler.FirearmInventoryProperties.AmmoId!);

            var ammoType = GetAmmoType(Item.GetAmmoType());
            
            if (ammoType.IsAmmo())
                return Player.Ammo.GetAmmo(ammoType);

            var ammoCount = Player.Inventory.CountItems(ammoType);

            if (Handler.FirearmInventoryProperties.UseNearbyAmmo)
                ammoCount += ExMap.Pickups.Count(x =>
                    Player.Position.DistanceTo(x.Position) <= Handler.FirearmInventoryProperties.NearbyAmmoRange);

            return ammoCount;
        }
    }

    /// <summary>
    /// Deals damage to the target player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="damageHandler">The created damage handler.</param>
    /// <param name="damage">The amount of damage to deal.</param>
    /// <param name="hit">The raycast hit pair.</param>
    /// <returns>true if the target received damage</returns>
    public virtual bool DamagePlayer(ExPlayer player, DamageHandlerBase damageHandler, float damage, DestructibleHitPair hit)
    {
        return player.ReferenceHub.playerStats.DealDamage(damageHandler);
    }

    /// <summary>
    /// Deals damage to the target obstacle.
    /// </summary>
    /// <param name="hit">The raycast hit pair.</param>
    public virtual bool DamageObstacle(HitRayPair hit)
    {
        return false;
    }

    /// <summary>
    /// Randomizes a ray.
    /// </summary>
    /// <param name="ray">The ray to randomize.</param>
    /// <param name="inaccuracy">Current firearm inaccuracy.</param>
    public virtual void RandomizeRay(ref Ray ray, float inaccuracy)
    {
        var random = Mathf.Max(UnityEngine.Random.value, UnityEngine.Random.value);
        var vector = UnityEngine.Random.insideUnitSphere * random;
        
        ray.direction = Quaternion.Euler(inaccuracy * vector) * ray.direction;
    }

    /// <summary>
    /// Appends raycast hits to the target.
    /// </summary>
    /// <param name="ray">The source ray.</param>
    /// <param name="result">The raycast result cache.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual void ScanTargets(Ray ray, HitscanResult result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        if (Modules.DisruptorHitreg != null
            && Modules.DisruptorHitreg.LastFiringState is DisruptorActionModule.FiringState.FiringSingle)
        {
            Modules.DisruptorHitreg._serverPenetrations = 0;

            var hits = Physics.SphereCastNonAlloc(ray, Modules.DisruptorHitreg._singleShotThickness,
                DisruptorHitregModule.NonAllocHits,
                Modules.DisruptorHitreg.DamageFalloffDistance + Modules.DisruptorHitreg.FullDamageDistance,
                HitscanHitregModuleBase.HitregMask);

            DisruptorHitregModule.SortedByDistanceHits.Clear();
            DisruptorHitregModule.SortedByDistanceHits.AddRange(new ArraySegment<RaycastHit>(
                DisruptorHitregModule.NonAllocHits,
                0, hits));
            DisruptorHitregModule.SortedByDistanceHits.Sort((x, y) => x.distance.CompareTo(y.distance));

            RaycastHit? raycastHit = null;

            foreach (var hit in DisruptorHitregModule.SortedByDistanceHits)
            {
                raycastHit = hit;

                if (hit.collider.TryGetComponent<IDestructible>(out var destructible))
                {
                    if (Modules.DisruptorHitreg.ValidateTarget(destructible, result))
                    {
                        result.Destructibles.Add(new DestructibleHitPair(destructible, hit, ray));

                        Modules.DisruptorHitreg._serverPenetrations++;
                    }
                }
                else
                {
                    if (!Modules.DisruptorHitreg.TryGetDoor(hit, out _))
                        break;

                    result.Obstacles.Add(new HitRayPair(ray, hit));
                }
            }

            if (raycastHit.HasValue)
            {
                Modules.DisruptorHitreg.RestoreHitboxes();

                var vector = raycastHit.Value.point + 0.15f * -ray.direction;

                ExplosionGrenade.Explode(Modules.DisruptorHitreg.DisruptorShotData.HitregFootprint, vector,
                    Modules.DisruptorHitreg._singleShotExplosionSettings, ExplosionType.Disruptor);
            }
        }
        else
        {
            var maxDistance = Modules.HitscanHitreg.DamageFalloffDistance + Modules.HitscanHitreg.FullDamageDistance;

            if (!Physics.Raycast(ray, out var hit, maxDistance, HitscanHitregModuleBase.HitregMask))
                return;

            if (!hit.collider.TryGetComponent<IDestructible>(out var destructible))
            {
                result.Obstacles.Add(new HitRayPair(ray, hit));
                return;
            }
            
            if (Modules.HitscanHitreg.ValidateTarget(destructible, result))
                result.Destructibles.Add(new DestructibleHitPair(destructible, hit, ray));
        }
    }
    
    /// <summary>
    /// Gets called when a player starts changing attachments on this firearm.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnChangingAttachments(PlayerChangingFirearmAttachmentsEventArgs args) { }
    
    /// <summary>
    /// Gets called when a player changes attachments on this firearm.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnChangedAttachments(PlayerChangedFirearmAttachmentsEventArgs args) { }

    /// <summary>
    /// Gets called when the player starts reloading.
    /// </summary>
    /// <returns>true if the player should be allowed to reload.</returns>
    public virtual bool OnReloading() => true;
    
    /// <summary>
    /// Gets called when the player finishes reloading.
    /// </summary>
    public virtual void OnReloaded() { }

    /// <summary>
    /// Gets called when the player starts unloading.
    /// </summary>
    /// <returns>true if the player should be allowed to unload.</returns>
    public virtual bool OnUnloading() => true;
    
    /// <summary>
    /// Gets called when the player finishes unloading.
    /// </summary>
    public virtual void OnUnloaded() { }

    /// <summary>
    /// Gets called before a shot is performed.
    /// </summary>
    /// <returns>true if the shot should be allowed to proceed</returns>
    public virtual bool OnShooting() => true;
    
    /// <summary>
    /// Gets called after a shot is performed.
    /// </summary>
    public virtual void OnShot() { }
    
    /// <summary>
    /// Gets called when the firearm starts processing an event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnProcessingEvent(FirearmProcessingEventEventArgs args) { }
    
    /// <summary>
    /// Gets called when the firearm finishes processing an event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public virtual void OnProcessedEvent(FirearmProcessedEventEventArgs args) { }

    /// <summary>
    /// Adds the specified amount of ammo to inventory.
    /// </summary>
    /// <param name="amount">The amount of ammo to add.</param>
    /// <returns>The amount of added ammo.</returns>
    public int AddInventoryAmmo(int amount)
    {
        if (Handler.UsesCustomAmmo)
        {
            Player.Ammo.AddCustomAmmo(Handler.FirearmInventoryProperties.AmmoId!, amount);
            return amount;
        }
        
        var ammoType = GetAmmoType(Item.GetAmmoType());

        if (ammoType.IsAmmo())
        {
            amount = Mathf.Clamp(amount, ushort.MinValue, ushort.MaxValue - Player.Ammo.GetAmmo(ammoType));
            
            Player.Ammo.AddAmmo(ammoType, (ushort)amount);
            return amount;
        }

        var inventoryAmount = Mathf.Clamp(amount, 0, 8 - Player.Inventory.ItemCount);
        var pickupAmount = Handler.FirearmInventoryProperties.DropExcessAmmo ? amount - inventoryAmount : 0;
        
        for (var i = 0; i < inventoryAmount; i++)
            Player.Inventory.AddItem(ammoType, ItemAddReason.AdminCommand);

        for (var i = 0; i < pickupAmount; i++)
            ExMap.SpawnItem<ItemPickupBase>(ammoType, Player.Position, Vector3.one, Player.Rotation);
        
        return amount;
    }

    /// <summary>
    /// Removes a specified amount of ammo from inventory.
    /// </summary>
    /// <returns>The removed ammo amount.</returns>
    public int RemoveInventoryAmmo(int amount)
    {
        if (Handler.UsesCustomAmmo)
        {
            Player.Ammo.RemoveCustomAmmo(Handler.FirearmInventoryProperties.AmmoId!, amount);
            return amount;
        }

        var ammoType = GetAmmoType(Item.GetAmmoType());

        if (ammoType.IsAmmo())
        {
            var maxAmount = Mathf.Clamp(amount, 0, Player.Ammo.GetAmmo(ammoType));

            Player.Ammo.SubstractAmmo(ammoType, (ushort)maxAmount);
            return maxAmount;
        }

        var removedAmount = 0;
        var inventoryAmount = Mathf.Min(amount, Player.Inventory.CountItems(ammoType));
        var remainingAmount = amount - inventoryAmount;

        if (inventoryAmount > 0)
        {
            removedAmount += inventoryAmount;
            Player.Inventory.RemoveItems(ammoType, inventoryAmount);
        }

        if (Handler.FirearmInventoryProperties.UseNearbyAmmo && remainingAmount > 0)
        {
            var nearbyItems = ListPool<ItemPickupBase>.Shared.Rent();

            for (var index = 0; index < ExMap.Pickups.Count; index++)
            {
                if (nearbyItems.Count >= remainingAmount)
                    break;
                
                var pickup = ExMap.Pickups[index];
                
                if (Player.Position.DistanceTo(pickup.Position) >
                    Handler.FirearmInventoryProperties.NearbyAmmoRange)
                    continue;

                nearbyItems.Add(pickup);
            }

            nearbyItems.ForEach(pickup =>
            {
                pickup.DestroySelf();
                removedAmount++;
            });

            ListPool<ItemPickupBase>.Shared.Return(nearbyItems);
        }

        return removedAmount;
    }

    #region Shot Management

    internal void InternalBuckshotFire(BuckshotHitreg buckshotHitreg)
    {
        buckshotHitreg._hitCounter.Clear();

        // var ray = buckshotHitreg.RandomizeRay(buckshotHitreg.ForwardRay, buckshotHitreg.CurrentInaccuracy);

        var ray = buckshotHitreg.ForwardRay;
        var spread = buckshotHitreg.Firearm.AttachmentsValue(AttachmentParam.SpreadPredictability);
        
        RandomizeRay(ref ray, buckshotHitreg.CurrentInaccuracy);
        
        spread = 1f - Mathf.Clamp01(1f - buckshotHitreg.ActivePattern.Randomness) * spread;
        
        buckshotHitreg.ResultNonAlloc.Clear();

        for (var i = 0; i < buckshotHitreg.ActivePattern.PredefinedPellets.Length; i++)
        {
            var pellet = buckshotHitreg.ActivePattern.PredefinedPellets[i];
            var direction =
                buckshotHitreg.GetPelletDirection(pellet, buckshotHitreg.BuckshotScale, spread, ray.direction);
            
            // buckshotHitreg.ServerAppendPrescan(new(ray.origin, direction), buckshotHitreg.ResultNonAlloc);
            
            ScanTargets(new(ray.origin, direction), buckshotHitreg.ResultNonAlloc);
        }
        
        buckshotHitreg.ServerApplyDamage(buckshotHitreg.ResultNonAlloc);
        
        buckshotHitreg._hitmarkerMaxHits += buckshotHitreg.ActivePattern.MaxHits;
        buckshotHitreg._hitmarkerMisses += buckshotHitreg.ResultNonAlloc.Obstacles.Count;
    }

    internal void InternalDisruptorFire(DisruptorHitregModule disruptorHitreg)
    {
        // var ray = disruptorHitreg.RandomizeRay(disruptorHitreg.ForwardRay, disruptorHitreg.CurrentInaccuracy);

        var ray = disruptorHitreg.ForwardRay;
        
        RandomizeRay(ref ray, disruptorHitreg.CurrentInaccuracy);
        
        disruptorHitreg._lastShotRay = ray;
        disruptorHitreg.ResultNonAlloc.Clear();

        /*
        if (disruptorHitreg.LastFiringState is DisruptorActionModule.FiringState.FiringSingle)
        {
            disruptorHitreg.PrescanSingle(ray, disruptorHitreg.ResultNonAlloc);
        }
        else
        {
            disruptorHitreg.ServerAppendPrescan(ray, disruptorHitreg.ResultNonAlloc);
        }
        */
        
        ScanTargets(ray, disruptorHitreg.ResultNonAlloc);
        
        disruptorHitreg.ServerApplyDamage(disruptorHitreg.ResultNonAlloc);
    }

    internal void InternalMultiBarrelFire(MultiBarrelHitscan multiBarrelHitscan)
    {
        var ray = multiBarrelHitscan._lastRay.GetValueOrDefault();

        if (!multiBarrelHitscan._lastRay.HasValue)
        {
            // ray = multiBarrelHitscan.RandomizeRay(multiBarrelHitscan.ForwardRay, multiBarrelHitscan.CurrentInaccuracy);

            ray = multiBarrelHitscan.ForwardRay;
            
            RandomizeRay(ref ray, multiBarrelHitscan.CurrentInaccuracy);
        }

        multiBarrelHitscan._lastRay = ray;
        
        var shotEvent = multiBarrelHitscan.LastShotEvent as BulletShotEvent;

        if (shotEvent is null || !multiBarrelHitscan._barrels.TryGet(shotEvent.BarrelId, out var barrelOffset))
        {
            // multiBarrelHitscan.ServerApplyDamage(multiBarrelHitscan.ServerPrescan(multiBarrelHitscan._lastRay.Value));

            InternalApplyDamage(multiBarrelHitscan, InternalPrescan(multiBarrelHitscan._lastRay.Value,
                multiBarrelHitscan.ResultNonAlloc, true));
            return;
        }

        var origin = multiBarrelHitscan._lastRay.Value.origin;
        var direction = multiBarrelHitscan._lastRay.Value.direction;
        
        origin += Player.CameraTransform.up * barrelOffset.TopPosition;
        origin += Player.CameraTransform.right * barrelOffset.RightPosition;
        
        direction += Vector3.up * barrelOffset.TopDirection;
        direction += Vector3.right * barrelOffset.RightDirection;

        var newRay = new Ray(origin, direction.normalized);

        InternalApplyDamage(multiBarrelHitscan, InternalPrescan(newRay, multiBarrelHitscan.ResultNonAlloc));
    }

    internal void InternalSingleBarrelFire(SingleBulletHitscan singleBulletHitscan)
    {
        // var ray = singleBulletHitscan.RandomizeRay(singleBulletHitscan.ForwardRay,
        //     singleBulletHitscan.CurrentInaccuracy);

        var ray = singleBulletHitscan.ForwardRay;
        
        RandomizeRay(ref ray, singleBulletHitscan.Inaccuracy);

        // singleBulletHitscan.ServerApplyDamage(singleBulletHitscan.ServerPrescan(ray));
        
        InternalApplyDamage(singleBulletHitscan, InternalPrescan(ray, singleBulletHitscan.ResultNonAlloc, true));
    }

    internal void InternalApplyDamage(HitscanHitregModuleBase module, HitscanResult result)
    {
        for (var i = 0; i < result.Destructibles.Count; i++)
        {
            var destructible = result.Destructibles[i];
            var damage = module.DamageAtDistance(destructible.Hit.distance);
            var handler = module.GetHandler(damage);

            if (destructible.Destructible is HitboxIdentity hitboxIdentity)
            {
                if (!ExPlayer.TryGet(hitboxIdentity.TargetHub, out var player))
                    continue;

                if (!player.IsAlive)
                {
                    result.RegisterDamage(destructible.Destructible, damage, handler);
                    continue;
                }

                handler.Hitbox = hitboxIdentity._dmgMultiplier;
                
                if (!DamagePlayer(player, handler, damage, destructible))
                    continue;
                
                result.RegisterDamage(destructible.Destructible, damage, handler);
                
                module.ServerPlayImpactEffects(destructible.Raycast, damage > 0f);
            }
        }

        for (var i = 0; i < result.Obstacles.Count; i++)
        {
            var obstacle = result.Obstacles[i];
            
            if (!DamageObstacle(obstacle))
                continue;
            
            module.ServerPlayImpactEffects(obstacle, false);
        }

        InternalSendHitmarkers(module, result);
    }

    internal void InternalSendHitmarkers(HitscanHitregModuleBase module, HitscanResult result)
    {
        var countedDestructibles = HashSetPool<uint>.Shared.Rent();

        if (!module._scheduledHitmarker.HasValue)
            module._scheduledHitmarker = 0f;

        module._scheduledHitmarker += result.OtherDamage;

        for (var i = 0; i < result.DamagedDestructibles.Count; i++)
        {
            var record = result.DamagedDestructibles[i];

            if (countedDestructibles.Add(record.Destructible.NetworkId))
            {
                module.SendDamageIndicator(record.Destructible.NetworkId, result.CountDamage(record.Destructible));
            }

            if (record.Destructible is not HitboxIdentity hitboxIdentity
                || Hitmarker.CheckHitmarkerPerms(record.Handler, hitboxIdentity.TargetHub))
            {
                if (!module._scheduledHitmarker.HasValue)
                    module._scheduledHitmarker = 0f;

                module._scheduledHitmarker += record.AppliedDamage;
            }
        }
        
        module.AlwaysUpdate();
        
        HashSetPool<uint>.Shared.Return(countedDestructibles);
    }
    
    internal HitscanResult InternalPrescan(Ray ray, HitscanResult nonAllocResult, bool nonAlloc = true)
    {
        HitscanResult hitscanResult;
        
        if (nonAlloc)
        {
            hitscanResult = nonAllocResult;
            hitscanResult.Clear();
        }
        else
        {
            hitscanResult = new HitscanResult();
        }
        
        // this.ServerAppendPrescan(targetRay, hitscanResult);

        ScanTargets(ray, hitscanResult);
        return hitscanResult;
    }
    #endregion

    #region Ammo Management
    internal int GetMaxAmmo(int defaultAmmo)
        => Handler.FirearmInventoryProperties.MaxAmmo ?? defaultAmmo;

    internal ItemType GetAmmoType(ItemType defaultType)
    {
        if (Handler.FirearmInventoryProperties.AmmoType != ItemType.None)
            return Handler.FirearmInventoryProperties.AmmoType;

        return defaultType;
    }

    internal void InternalRemoveMagazine(MagazineModule magazineModule)
    {
        // magazineModule.UserInv.ServerAddAmmo(magazineModule.AmmoType, magazineModule.AmmoStored);

        if (magazineModule.AmmoStored > 0)
            AddInventoryAmmo(magazineModule.AmmoStored);
        
        magazineModule.MagazineInserted = false;
        magazineModule.ServerResyncData();
    }

    internal void InternalInsertMagazine(MagazineModule magazineModule)
    {
        magazineModule.MagazineInserted = true;
        magazineModule.ServerResyncData();

        // var amount = magazineModule.AmmoMax - magazineModule.AmmoStored;
        // var available = Mathf.Min(Mathf.Min(magazineModule.UserInv.GetCurAmmo(magazineModule.AmmoType), amount),
        //     int.MaxValue);

        var amount = GetMaxAmmo(magazineModule.AmmoMax) - magazineModule.AmmoStored;
        var available = Mathf.Min(AvailableInventoryAmmo, amount);
        
        // magazineModule.UserInv.ServerAddAmmo(magazineModule.AmmoType, -available);

        RemoveInventoryAmmo(available);
        
        magazineModule.AmmoStored += available;
        magazineModule.ServerResyncData();
    }

    internal void InternalUnloadChambered(AutomaticActionModule automaticActionModule)
    {
        if (!automaticActionModule.Firearm.TryGetModule<IPrimaryAmmoContainerModule>(out _, true))
            return;
        
        // automaticActionModule.Firearm.OwnerInventory.ServerAddAmmo(primaryAmmoContainer.AmmoType, automaticActionModule.AmmoStored);

        if (automaticActionModule.AmmoStored > 0)
            AddInventoryAmmo(automaticActionModule.AmmoStored);
        
        automaticActionModule.AmmoStored = 0;
        automaticActionModule.ServerResync();
    }

    internal void InternalCycleAction(AutomaticActionModule automaticActionModule)
    {
        automaticActionModule.Cocked = true;

        if (!automaticActionModule.OpenBolt)
        {
            // var amount = Mathf.Min(automaticActionModule.PrimaryAmmoContainer.AmmoStored,
            //     automaticActionModule.ChamberSize - automaticActionModule.AmmoStored);

            var amount = Mathf.Min(automaticActionModule.AmmoStored,
                GetMaxAmmo(automaticActionModule.ChamberSize) - automaticActionModule.AmmoStored);
            
            automaticActionModule.AmmoStored += amount;
            
            automaticActionModule.PrimaryAmmoContainer.ServerModifyAmmo(-amount);
            
            automaticActionModule.BoltLocked = automaticActionModule.AmmoStored == 0 &&
                                               automaticActionModule.MagLocksBolt && automaticActionModule.MagInserted;
        }
        
        automaticActionModule.ServerResync();
    }

    internal void InternalUnloadAllChambers(CylinderAmmoModule ammoModule)
    {
        if (!ammoModule.Firearm.TryGetChambers(out var chambers)
            || chambers?.Length < 1)
            return;

        var amount = ammoModule.AmmoStored;
        
        for (var i = 0; i < chambers.Length; i++)
            chambers[i].ContextState = CylinderAmmoModule.ChamberState.Empty;

        ammoModule._needsResyncing = true;
        
        // ammoModule.Firearm.OwnerInventory.ServerAddAmmo(ammoModule.AmmoType, amount);
        
        AddInventoryAmmo(amount);
    }

    internal void InternalWithholdAmmo(RevolverClipReloaderModule reloaderModule)
    {
        if (reloaderModule.ServerWithheld > 0)
        {
            AddInventoryAmmo(reloaderModule.ServerWithheld);
            
            // reloaderModule.Firearm.OwnerInventory.ServerAddAmmo(reloaderModule.AmmoContainer.AmmoType,
            //     reloaderModule.ServerWithheld);
            
            reloaderModule.ServerWithheld = 0;
        }

        // reloaderModule.ServerWithheld =
        //    Mathf.Min(reloaderModule.Firearm.OwnerInventory.GetCurAmmo(reloaderModule.AmmoContainer.AmmoType),
        //        reloaderModule.AmmoContainer.AmmoMax);

        reloaderModule.ServerWithheld =
            Mathf.Min(AvailableInventoryAmmo, GetMaxAmmo(reloaderModule.AmmoContainer.AmmoMax));
        
        // reloaderModule.Firearm.OwnerInventory.ServerAddAmmo(reloaderModule.AmmoContainer.AmmoType, reloaderModule.ServerWithheld);

        RemoveInventoryAmmo(reloaderModule.ServerWithheld);
    }

    internal void InternalInsertAmmoFromClip(RevolverClipReloaderModule reloaderModule)
    {
        // var amount = Mathf.Min(reloaderModule.WithheldAmmo, reloaderModule.AmmoContainer.AmmoMax);
        var amount = Mathf.Min(reloaderModule.WithheldAmmo, GetMaxAmmo(reloaderModule.AmmoContainer.AmmoMax));
        
        reloaderModule.AmmoContainer.ServerModifyAmmo(amount);
        reloaderModule.ServerWithheld -= amount;

        if (reloaderModule.ServerWithheld > 0)
        {
            AddInventoryAmmo(reloaderModule.ServerWithheld);
            
            // reloaderModule.Firearm.OwnerInventory.ServerAddAmmo(reloaderModule.AmmoContainer.AmmoType,
            //     reloaderModule.ServerWithheld);
            
            reloaderModule.ServerWithheld = 0;
        }
    }

    internal void InternalStopReloadingAndUnloading(AnimatorReloaderModuleBase reloaderModule)
    {
        var currentEvent = FirearmEvent.CurrentlyInvokedEvent;

        if (currentEvent != null)
        {
            if (currentEvent.LastInvocation.RawAnimator.IsInTransition(currentEvent.LastInvocation.Layer)
                && currentEvent.LastInvocation.RawAnimator.GetNextAnimatorStateInfo(currentEvent.LastInvocation.Layer)
                    .tagHash == FirearmAnimatorHashes.Reload)
                return;
        }

        if (reloaderModule.IsReloading)
        {
            PlayerEvents.OnReloadedWeapon(new(reloaderModule.Firearm.Owner, reloaderModule.Firearm));
            OnReloaded();
        }

        if (reloaderModule.IsUnloading)
        {
            PlayerEvents.OnUnloadedWeapon(new(reloaderModule.Firearm.Owner, reloaderModule.Firearm));
            OnUnloaded();
        }

        reloaderModule.IsReloading = false;
        reloaderModule.IsUnloading = false;
        
        reloaderModule.SendRpc(x => x.WriteSubheader(AnimatorReloaderModuleBase.ReloaderMessageHeader.Stop));
    }
    #endregion
}