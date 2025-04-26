using LabExtended.API.CustomItems;

using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Player;
using LabExtended.Events.Firearms;

namespace LabExtended.API.CustomFirearms;

/// <summary>
/// Manages Custom Firearm items.
/// </summary>
public static class CustomFirearmManager
{
    private static void OnShooting(PlayerShootingFirearmEventArgs args)
    {
        if (!CustomItemManager.InventoryItems.TryGetValue<CustomFirearmInstance>(args.Firearm, out var customFirearm))
            return;
        
        customFirearm.OnShooting(args);
    }

    private static void OnShot(PlayerShotFirearmEventArgs args)
    {
        if (!CustomItemManager.InventoryItems.TryGetValue<CustomFirearmInstance>(args.Firearm, out var customFirearm))
            return;
        
        customFirearm.OnShot(args);
    }
    
    private static void OnFirearmRayCast(FirearmRayCastEventArgs args)
    {
        if (!CustomItemManager.InventoryItems.TryGetValue<CustomFirearmInstance>(args.Firearm, out var customFirearm))
            return;
        
        customFirearm.OnRayCast(args);
    }

    private static void OnChangingAttachments(PlayerChangingFirearmAttachmentsEventArgs args)
    {
        if (!CustomItemManager.InventoryItems.TryGetValue<CustomFirearmInstance>(args.Firearm, out var customFirearm))
            return;

        if (!customFirearm.CustomData.AllowsAttachmentsChange)
        {
            args.IsAllowed = false;
            return;
        }
        
        customFirearm.OnChangingAttachments(args);
    }

    private static void OnChangedAttachments(PlayerChangedFirearmAttachmentsEventArgs args)
    {
        if (!CustomItemManager.InventoryItems.TryGetValue<CustomFirearmInstance>(args.Firearm, out var customFirearm))
            return;
        
        customFirearm.OnChangedAttachments(args);
    }
    
    [LoaderInitialize(1)]
    private static void OnInit()
    {
        ExFirearmEvents.RayCast += OnFirearmRayCast;
        
        ExPlayerEvents.ShotFirearm += OnShot;
        ExPlayerEvents.ShootingFirearm += OnShooting;

        ExPlayerEvents.ChangingAttachments += OnChangingAttachments;
        ExPlayerEvents.ChangedAttachments += OnChangedAttachments;
    }
}