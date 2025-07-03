using InventorySystem.Items.Firearms.Modules;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API.CustomFirearms.Behaviours;
using LabExtended.API.CustomItems;

using LabExtended.Events;
using LabExtended.Events.Player;
using LabExtended.Events.Firearms;

using LabExtended.Attributes; 
using LabExtended.Extensions;

namespace LabExtended.API.CustomFirearms;

/// <summary>
/// Provides events to custom firearms.
/// </summary>
public static class CustomFirearmEvents
{
    private static void OnChangingAttachments(PlayerChangingFirearmAttachmentsEventArgs args)
    {
        CustomItemUtils.ProcessEvent<CustomFirearmInventoryBehaviour>(args.Firearm.ItemSerial, item =>
        {
            if (item.Handler.FirearmInventoryProperties.Attachments != null)
            {
                args.ToDisable.AddRangeWhere(args.Current, 
                    x => !args.ToDisable.Contains(x) 
                         && item.Handler.FirearmInventoryProperties.Attachments.BlacklistedAttachments.Contains(x));
                
                if (item.Handler.FirearmInventoryProperties.Attachments.WhitelistedAttachments.Count > 0)
                    args.ToDisable.AddRangeWhere(args.Current, 
                        x => !args.ToDisable.Contains(x) 
                             && !item.Handler.FirearmInventoryProperties.Attachments.WhitelistedAttachments.Contains(x));
            }
            
            item.OnChangingAttachments(args);
        });
    }

    private static void OnChangedAttachments(PlayerChangedFirearmAttachmentsEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomFirearmInventoryBehaviour>(args.Firearm.ItemSerial,
            item => item.OnChangedAttachments(args));

    private static void OnReloading(PlayerReloadingWeaponEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomFirearmInventoryBehaviour>(args.FirearmItem.Serial,
            item =>
            {
                if (!item.OnReloading())
                    args.IsAllowed = false;
            });
    
    private static void OnUnloading(PlayerUnloadingWeaponEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomFirearmInventoryBehaviour>(args.FirearmItem.Serial,
            item =>
            {
                if (!item.OnUnloading())
                    args.IsAllowed = false;
            });
    
    private static void OnShooting(PlayerShootingWeaponEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomFirearmInventoryBehaviour>(args.FirearmItem.Serial,
            item =>
            {
                if (!item.OnShooting())
                    args.IsAllowed = false;
            });

    private static void OnShot(PlayerShotWeaponEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomFirearmInventoryBehaviour>(args.FirearmItem.Serial,
            item => item.OnShot());

    private static void OnProcessingEvent(FirearmProcessingEventEventArgs args)
    {
        if (CustomItemUtils.TryGetBehaviour<CustomFirearmInventoryBehaviour>(args.Firearm.ItemSerial,
                out var behaviour))
        {
            if (args.Module is MagazineModule magazineModule)
            {
                if (string.Equals(args.Method, "ServerRemoveMagazine"))
                {
                    args.IsAllowed = false;
                    behaviour.InternalRemoveMagazine(magazineModule);
                }
                else if (string.Equals(args.Method, "ServerInsertMagazine"))
                {
                    args.IsAllowed = false;
                    behaviour.InternalInsertMagazine(magazineModule);
                }
            }
            else if (args.Module is CylinderAmmoModule cylinderModule)
            {
                if (string.Equals(args.Method, "UnloadAllChambers"))
                {
                    args.IsAllowed = false;
                    behaviour.InternalUnloadAllChambers(cylinderModule);
                }
            }
            else if (args.Module is AutomaticActionModule automaticActionModule)
            {
                if (string.Equals(args.Method, "ServerUnloadChambered"))
                {
                    args.IsAllowed = false;
                    behaviour.InternalUnloadChambered(automaticActionModule);
                }
                else if (string.Equals(args.Method, "ServerCycleAction"))
                {
                    args.IsAllowed = false;
                    behaviour.InternalCycleAction(automaticActionModule);
                }
            }
            else if (args.Module is AnimatorReloaderModuleBase reloaderModule)
            {
                if (reloaderModule is RevolverClipReloaderModule clipReloaderModule)
                {
                    if (string.Equals(args.Method, "ServerWithholdAmmo"))
                    {
                        args.IsAllowed = false;
                        behaviour.InternalWithholdAmmo(clipReloaderModule);
                    }
                    else if (string.Equals(args.Method, "InsertAmmoFromClip"))
                    {
                        args.IsAllowed = false;
                        behaviour.InternalInsertAmmoFromClip(clipReloaderModule);
                    }
                }

                if (string.Equals(args.Method, "StopReloadingAndUnloading"))
                {
                    args.IsAllowed = false;
                    behaviour.InternalStopReloadingAndUnloading(reloaderModule);
                }
            }

            behaviour.OnProcessingEvent(args);
        }
    }

    private static void OnProcessedEvent(FirearmProcessedEventEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomFirearmInventoryBehaviour>(args.Firearm.ItemSerial,
            item => item.OnProcessedEvent(args));
    
    [LoaderInitialize(1)]
    private static void OnInit()
    {
        ExPlayerEvents.ChangingAttachments += OnChangingAttachments;
        ExPlayerEvents.ChangedAttachments += OnChangedAttachments;

        ExFirearmEvents.ProcessingEvent += OnProcessingEvent;
        ExFirearmEvents.ProcessedEvent += OnProcessedEvent;
        
        PlayerEvents.UnloadingWeapon += OnUnloading;
        PlayerEvents.ReloadingWeapon += OnReloading;

        PlayerEvents.ShootingWeapon += OnShooting;
        PlayerEvents.ShotWeapon += OnShot;
    }
}