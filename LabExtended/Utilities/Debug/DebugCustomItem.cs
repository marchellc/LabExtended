using LabExtended.API.CustomItems.Enums;
using LabExtended.API.CustomItems.Firearms;
using LabExtended.API.CustomItems.Info;

using LabExtended.Core;

using LabExtended.Events;
using LabExtended.Events.Other;
using LabExtended.Events.Player;

using PluginAPI.Events;

using UnityEngine;

namespace LabExtended.Utilities.Debug
{
    public class DebugCustomItem : CustomFirearm
    {
        internal static bool IsRegistered;

        public static void RegisterItem()
        {
            if (IsRegistered)
                return;

            IsRegistered = RegisterItem(
                new CustomFirearmInfo(typeof(DebugCustomItem), "debug_ci", "Debug Item", "A custom item for debugging purposes", ItemType.GunCOM18, CustomItemFlags.SelectOnPickup,
                new CustomItemPickupInfo(Vector3.one * 5f, ItemType.GunCom45))

                {
                    AmmoType = ItemType.Ammo12gauge,
                    FirearmFlags = CustomFirearmFlags.None,
                    MaxAmmo = 60,
                    StartAmmo = 40
                }
                );

            if (IsRegistered)
                ExLoader.Info("Debug Custom Item", "Registered debug item");
            else
                ExLoader.Warn("Debug Custom Item", "Failed to register debug item");
        }

        public override void OnAdded()
            => ExLoader.Info("Debug Custom Item", $"OnAdded");

        public override void OnAdding()
            => ExLoader.Info("Debug Custom Item", $"OnAdding");

        public override void OnDeselected(PlayerSelectedItemArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnDeselected");

        public override void OnDeselecting(PlayerSelectingItemArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnDeselecting");

        public override void OnDropped(PlayerDroppingItemArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnDropped");

        public override void OnDropping(PlayerDroppingItemArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnDropping");

        public override bool OnDying(PlayerDyingEvent args)
        {
            ExLoader.Info("Debug Custom Item", $"OnDying");
            return true;
        }

        public override void OnOwnerSpawned(PlayerChangedRoleArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnOwnerSpawned");

        public override void OnOwnerSpawning(PlayerSpawningArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnOwnerSpawning");

        public override void OnPickedUp(PlayerPickingUpItemArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnPickedUp");

        public override void OnPickingUp(PlayerPickingUpItemArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnPickingUp");

        public override void OnSelected(PlayerSelectedItemArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnSelected");

        public override void OnSelecting(PlayerSelectingItemArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnSelecting");

        public override void OnSpawned()
            => ExLoader.Info("Debug Custom Item", $"OnSpawned");

        public override void OnSpawning()
            => ExLoader.Info("Debug Custom Item", $"OnSpawning");

        public override void OnThrowing(PlayerThrowingItemArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnThrowing");

        public override void OnThrown(PlayerThrowingItemArgs args)
            => ExLoader.Info("Debug Custom Item", $"OnThrown");

        public override void OnDryFired()
            => ExLoader.Info("Debug Custom Item", "OnDryFired");

        public override bool OnDryFiring()
        {
            ExLoader.Info("Debug Custom Item", "OnDryFiring");
            return true;
        }

        public override void OnInspected()
            => ExLoader.Info("Debug Custom Item", "OnInspected");

        public override bool OnInspecting()
        {
            ExLoader.Info("Debug Custom Item", "OnInspecting");
            return true;
        }

        public override void OnReloaded(int addedAmmo)
            => ExLoader.Info("Debug Custom Item", $"OnReloaded addedAmmo={addedAmmo}");

        public override bool OnReloading(ref int ammoToReload)
        {
            ExLoader.Info("Debug Custom Item", $"OnReloading ammoToReload={ammoToReload}");
            return true;
        }

        public override void OnRequestProcessed(ProcessedFirearmRequestArgs args)
            => ExLoader.Info("Debug Custom Item", $"Processed request {args.Message.Request}");

        public override void OnRequestReceived(ProcessingFirearmRequestArgs args)
            => ExLoader.Info("Debug Custom Item", $"Received request {args.Message.Request}");

        public override void OnStoppedReload()
            => ExLoader.Info("Debug Custom Item", "OnStoppedReload");

        public override bool OnStoppingReload()
        {
            ExLoader.Info("Debug Custom Item", "OnStoppingReload");
            return true;
        }

        public override void OnToggledFlashlight(bool isEnabled)
        {
            ExLoader.Info("Debug Custom Item", $"OnToggledFlashlight isEnabled={isEnabled}");
        }

        public override bool OnTogglingFlashlight(bool isEnabled)
        {
            ExLoader.Info("Debug Custom Item", $"OnTogglingFlashlight isEnabled={isEnabled}");
            return true;
        }

        public override void OnUnloaded()
        {
            ExLoader.Info("Debug Custom Item", "OnUnloaded");
        }

        public override bool OnUnloading()
        {
            ExLoader.Info("Debug Custom Item", "OnUnloading");
            return true;
        }

        public override void OnZoomedIn()
        {
            ExLoader.Info("Debug Custom Item", "OnZoomedIn");
        }

        public override void OnZoomedOut()
        {
            ExLoader.Info("Debug Custom Item", "OnZoomedOut");
        }

        public override bool OnZoomingIn()
        {
            ExLoader.Info("Debug Custom Item", "OnZoomingIn");
            return true;
        }

        public override bool OnZoomingOut()
        {
            ExLoader.Info("Debug Custom Item", "OnZoomingOut");
            return true;
        }

        public void PrintState()
            => ExLoader.Info("Debug Custom Item",
                $"Debug Item State:\n" +
                $"Owner - {Owner?.Name ?? "null"}\n" +
                $"Item - {Item != null}\n" +
                $"Pickup - {Pickup != null}\n" +
                $"Serial - {Serial}\n" +
                $"IsSelected - {IsSelected}\n" +
                $"CurrentAmmo - {CurrentAmmo}");
    }
}