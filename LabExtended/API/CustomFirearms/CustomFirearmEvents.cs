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

using NorthwoodLib.Pools;

namespace LabExtended.API.CustomFirearms;

/// <summary>
/// Provides events to custom firearms.
/// </summary>
public static class CustomFirearmEvents
{
    private static void OnChangingAttachments(PlayerChangingFirearmAttachmentsEventArgs args)
    {
        CustomItemUtils.ForEachInventoryBehaviour<CustomFirearmInventoryBehaviour>(args.Firearm.ItemSerial, item =>
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
        => CustomItemUtils.ForEachInventoryBehaviour<CustomFirearmInventoryBehaviour>(args.Firearm.ItemSerial,
            item => item.OnChangedAttachments(args));

    private static void OnReloading(PlayerReloadingWeaponEventArgs args)
        => CustomItemUtils.ForEachInventoryBehaviour<CustomFirearmInventoryBehaviour>(args.FirearmItem.Serial,
            item =>
            {
                if (!item.OnReloading())
                    args.IsAllowed = false;
            });
    
    private static void OnUnloading(PlayerUnloadingWeaponEventArgs args)
        => CustomItemUtils.ForEachInventoryBehaviour<CustomFirearmInventoryBehaviour>(args.FirearmItem.Serial,
            item =>
            {
                if (!item.OnUnloading())
                    args.IsAllowed = false;
            });

    private static void OnProcessingEvent(FirearmProcessingEventEventArgs args)
    {
        var behaviours = ListPool<CustomFirearmInventoryBehaviour>.Shared.Rent();
        
        CustomItemUtils.GetInventoryBehavioursNonAlloc(args.Firearm.ItemSerial, behaviours);

        if (behaviours.Count == 0)
        {
            ListPool<CustomFirearmInventoryBehaviour>.Shared.Return(behaviours);
            return;
        }
        
        if (args.Module is MagazineModule magazineModule)
        {
            if (string.Equals(args.Method, "ServerRemoveMagazine"))
            {
                args.IsAllowed = false;
                behaviours.ForEach(b => b.InternalRemoveMagazine(magazineModule));
            }
            else if (string.Equals(args.Method, "ServerInsertMagazine"))
            {
                args.IsAllowed = false;
                behaviours.ForEach(b => b.InternalInsertMagazine(magazineModule));
            }
        }
        else if (args.Module is CylinderAmmoModule cylinderModule)
        {
            if (string.Equals(args.Method, "UnloadAllChambers"))
            {
                args.IsAllowed = false;
                behaviours.ForEach(b => b.InternalUnloadAllChambers(cylinderModule));
            }
        }
        else if (args.Module is AutomaticActionModule automaticActionModule)
        {
            if (string.Equals(args.Method, "ServerUnloadChambered"))
            {
                args.IsAllowed = false;
                behaviours.ForEach(b => b.InternalUnloadChambered(automaticActionModule));
            }
            else if (string.Equals(args.Method, "ServerCycleAction"))
            {
                args.IsAllowed = false;
                behaviours.ForEach(b => b.InternalCycleAction(automaticActionModule));
            }
        }
        else if (args.Module is AnimatorReloaderModuleBase reloaderModule)
        {
            if (reloaderModule is RevolverClipReloaderModule clipReloaderModule)
            {
                if (string.Equals(args.Method, "ServerWithholdAmmo"))
                {
                    args.IsAllowed = false;
                    behaviours.ForEach(b => b.InternalWithholdAmmo(clipReloaderModule));
                }
                else if (string.Equals(args.Method, "InsertAmmoFromClip"))
                {
                    args.IsAllowed = false;
                    behaviours.ForEach(b => b.InternalInsertAmmoFromClip(clipReloaderModule));
                }
            }
            
            if (string.Equals(args.Method, "StopReloadingAndUnloading"))
            {
                args.IsAllowed = false;
                behaviours.ForEach(b => b.InternalStopReloadingAndUnloading(reloaderModule));
            }
        }
        
        behaviours.ForEach(b => b.OnProcessingEvent(args));
        
        ListPool<CustomFirearmInventoryBehaviour>.Shared.Return(behaviours);
    }

    private static void OnProcessedEvent(FirearmProcessedEventEventArgs args)
        => CustomItemUtils.ForEachInventoryBehaviour<CustomFirearmInventoryBehaviour>(args.Firearm.ItemSerial,
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
    }
}