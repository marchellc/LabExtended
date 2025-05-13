using InventorySystem.Items;
using InventorySystem.Items.Autosync;

using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using InventorySystem.Items.Pickups;
using LabApi.Events.Handlers;

using LabExtended.API.CustomItems.Behaviours;

using LabExtended.Events.Firearms;
using LabExtended.Events.Player;

using LabExtended.Extensions;
using LabExtended.Utilities.Firearms;

using UnityEngine;

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
    /// Gets the amount of ammo available to load into the firearm.
    /// </summary>
    public int AvailableInventoryAmmo
    {
        get
        {
            if (Handler.UsesCustomAmmo)
                return Player.Inventory.CustomAmmo.Get(Handler.FirearmInventoryProperties.AmmoId);

            var ammoType = GetAmmoType(Item.GetAmmoType());
            
            if (ammoType.IsAmmo())
                return Player.Ammo.GetAmmo(ammoType);

            return Player.Inventory.CountItems(ammoType);
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
            Player.Inventory.CustomAmmo.Add(Handler.FirearmInventoryProperties.AmmoId, amount);
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
            return Player.Inventory.CustomAmmo.Remove(Handler.FirearmInventoryProperties.AmmoId, amount);

        var ammoType = GetAmmoType(Item.GetAmmoType());

        if (ammoType.IsAmmo())
        {
            var maxAmount = Mathf.Clamp(amount, 0, Player.Ammo.GetAmmo(ammoType));
            
            Player.Ammo.SubstractAmmo(ammoType, (ushort)maxAmount);
            return maxAmount;
        }
        else
        {
            var maxAmount = Mathf.Clamp(amount, 0, Player.Inventory.CountItems(ammoType));

            if (maxAmount > 0)
                Player.Inventory.RemoveItems(ammoType, maxAmount);
            
            return maxAmount;
        }
    }

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
}