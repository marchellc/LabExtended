using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Pickups;
using LabExtended.API.CustomItems;
using LabExtended.Events.Firearms;
using LabExtended.Events.Player;
using LabExtended.Utilities;
using LabExtended.Utilities.Firearms;
using UnityEngine;
using IActionModule = InventorySystem.Items.Firearms.Modules.IActionModule;

namespace LabExtended.API.CustomFirearms;

/// <summary>
/// Represents an active Custom Firearm item instance.
/// </summary>
public class CustomFirearmInstance : CustomItemInstance
{
    /// <summary>
    /// Gets the associated Firearm item.
    /// </summary>
    public new Firearm? Item { get; private set; }
    
    /// <summary>
    /// Gets the associated Firearm pickup.
    /// </summary>
    public new FirearmPickup? Pickup { get; private set; }
    
    /// <summary>
    /// Gets the Custom Firearm configuration.
    /// </summary>
    public new CustomFirearmData? CustomData => base.CustomData as CustomFirearmData;
    
    /// <summary>
    /// Gets the firearm's unloaded custom ammo.
    /// </summary>
    public int UnloadedAmmo { get; internal set; }

    /// <summary>
    /// Gets called once a shot processing starts. 
    /// </summary>
    /// <returns>true if the firearm should be allowed to shoot</returns>
    public virtual bool OnProcessingShot(ExPlayer? target, Vector3? targetPosition) => true;

    /// <summary>
    /// Gets called once a shot processing finishes.
    /// </summary>
    public virtual void OnProcessedShot(ExPlayer? target, Vector3? targetPosition) { }
    
    /// <summary>
    /// Gets called when a custom firearm is shot.
    /// </summary>
    /// <param name="eventArgs">The <see cref="PlayerShootingFirearmEventArgs"/> event arguments.</param>
    public virtual void OnShooting(PlayerShootingFirearmEventArgs eventArgs) { }
    
    /// <summary>
    /// Gets called after a custom firearm is shot.
    /// </summary>
    /// <param name="eventArgs">The <see cref="PlayerShotFirearmEventArgs"/> event arguments.</param>
    public virtual void OnShot(PlayerShotFirearmEventArgs eventArgs) { }
    
    /// <summary>
    /// Gets called when a custom firearm is shot and trajectory is being calculated.
    /// </summary>
    /// <param name="eventArgs">The <see cref="FirearmRayCastEventArgs"/> event arguments.</param>
    public virtual void OnRayCast(FirearmRayCastEventArgs eventArgs) { }

    /// <summary>
    /// Gets called before a player is damaged.
    /// </summary>
    /// <param name="target">The player to damage.</param>
    /// <param name="damage">The damage to be dealt.</param>
    /// <returns>true if the player should be damaged</returns>
    public virtual bool OnHurting(ExPlayer target, ref float damage) => true;
    
    /// <summary>
    /// Gets called after a player is damaged.
    /// </summary>
    /// <param name="target">The player that was damaged.</param>
    /// <param name="damage">The damage the player received.</param>
    public virtual void OnHurt(ExPlayer target, float damage) { }
    
    /// <summary>
    /// Gets called before the weapon is dry-fired.
    /// </summary>
    /// <returns>true if the weapon should be allowed to dry-fire</returns>
    public virtual bool OnDryFiring() => true;
    
    /// <summary>
    /// Gets called after the weapon is dry-fired.
    /// </summary>
    public virtual void OnDryFired() { }

    /// <summary>
    /// Gets called before the firearm is reloaded.
    /// </summary>
    /// <returns>true if the firearm should be allowed to reload</returns>
    public virtual bool OnReloading() => true;
    
    /// <summary>
    /// Gets called after the firearm is reloaded.
    /// </summary>
    public virtual void OnReloaded() { }

    /// <summary>
    /// Gets called before the firearm's ammo is unloaded.
    /// </summary>
    /// <returns>true if the firearm's ammo should be unloaded</returns>
    public virtual bool OnUnloading() => true;
    
    /// <summary>
    /// Gets called after the firearm's ammo is unloaded.
    /// </summary>
    public virtual void OnUnloaded() { }

    internal override void OnItemSet()
    {
        base.OnItemSet();
        
        Item = base.Item as Firearm;
        Pickup = null;
    }

    internal override void OnPickupSet()
    {
        base.OnPickupSet();
        
        Pickup = base.Pickup as FirearmPickup;
        Item = null;
    }
}