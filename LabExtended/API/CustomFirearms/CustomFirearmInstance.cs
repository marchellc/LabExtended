using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Pickups;
using LabExtended.API.CustomItems;
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
    /// Gets the firearm's action module.
    /// </summary>
    public IActionModule? ActionModule { get; private set; }
    
    /// <summary>
    /// Gets the firearm's ADS preventer module.
    /// </summary>
    public IAdsPreventerModule? AdsPreventerModule { get; private set; }
    
    /// <summary>
    /// Gets the firearm's ADS module.
    /// </summary>
    public LinearAdsModule? AdsModule { get; private set; }

    /// <summary>
    /// Gets the firearm's inaccuracy while aiming.
    /// </summary>
    public float AimInaccuracy => AdsModule?.Inaccuracy ?? 0f;

    /// <summary>
    /// Whether or not the user is currently aiming.
    /// </summary>
    public bool IsAiming => AdsModule?._userInput ?? false;

    /// <summary>
    /// Whether or not the user is allowed to aim.
    /// </summary>
    public bool IsAimingAllowed => !IsAiming && (AdsPreventerModule?.AdsAllowed ?? true);
    
    /// <summary>
    /// Whether or not the firearm is cocked.
    /// </summary>
    public bool IsCocked
    {
        get => ActionModule?.IsCocked() ?? false;
        set => ActionModule?.SetCocked(value);
    }

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
    /// Gets called before the weapon is dry-fired.
    /// </summary>
    /// <returns>true if the weapon should be allowed to dry-fire</returns>
    public virtual bool OnDryFiring() => true;
    
    /// <summary>
    /// Gets called after the weapon is dry-fired.
    /// </summary>
    public virtual void OnDryFired() { }

    internal override void OnItemSet()
    {
        base.OnItemSet();
        
        Item = base.Item as Firearm;
        Pickup = null;

        if (Item != null)
        {
            ActionModule = Item.GetModule<IActionModule>();
            
            AdsModule = Item.GetModule<LinearAdsModule>();
            AdsPreventerModule = Item.GetModule<IAdsPreventerModule>();
        }
    }

    internal override void OnPickupSet()
    {
        base.OnPickupSet();
        
        Pickup = base.Pickup as FirearmPickup;
        
        Item = null;

        AdsModule = null;
        ActionModule = null;
        AdsPreventerModule = null;
    }
}