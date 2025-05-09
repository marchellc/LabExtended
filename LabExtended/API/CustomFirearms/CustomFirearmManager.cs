using LabExtended.API.CustomItems;

using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Player;
using LabExtended.Events.Firearms;
using PlayerStatsSystem;

namespace LabExtended.API.CustomFirearms;

/// <summary>
/// Manages Custom Firearm items.
/// </summary>
public static class CustomFirearmManager
{
    private static void OnShooting(PlayerShootingFirearmEventArgs args)
    {
        if (args.Firearm.GetTracker().CustomItem is not CustomFirearmInstance customFirearm)
            return;
        
        customFirearm.OnShooting(args);
    }

    private static void OnShot(PlayerShotFirearmEventArgs args)
    {
        if (args.Firearm.GetTracker().CustomItem is not CustomFirearmInstance customFirearm)
            return;
        
        customFirearm.OnShot(args);
    }
    
    private static void OnChangingAttachments(PlayerChangingFirearmAttachmentsEventArgs args)
    {
        if (args.Firearm.GetTracker().CustomItem is not CustomFirearmInstance customFirearm)
            return;
        
        customFirearm.OnChangingAttachments(args);
    }

    private static void OnChangedAttachments(PlayerChangedFirearmAttachmentsEventArgs args)
    {
        if (args.Firearm.GetTracker().CustomItem is not CustomFirearmInstance customFirearm)
            return;
        
        customFirearm.OnChangedAttachments(args);
    }
    
    [LoaderInitialize(1)]
    private static void OnInit()
    {
        ExPlayerEvents.ShootingFirearm += OnShooting;
        ExPlayerEvents.ShotFirearm += OnShot;
        
        ExPlayerEvents.ChangingAttachments += OnChangingAttachments;
        ExPlayerEvents.ChangedAttachments += OnChangedAttachments;
    }
}