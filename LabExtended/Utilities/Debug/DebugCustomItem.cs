using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Enums;
using LabExtended.API.CustomItems.Info;
using LabExtended.Core;

using LabExtended.Events;
using LabExtended.Events.Player;

using PluginAPI.Events;

using UnityEngine;

namespace LabExtended.Utilities.Debug
{
    public class DebugCustomItem : CustomItem
    {
        internal static bool IsRegistered;

        public static void RegisterItem()
        {
            if (IsRegistered)
                return;

            IsRegistered = RegisterItem(
                new CustomItemInfo(typeof(DebugCustomItem), "debug_ci", "Debug Item", "A custom item for debugging purposes", ItemType.Adrenaline, CustomItemFlags.SelectOnPickup,
                new CustomItemPickupInfo(Vector3.one * 5f, ItemType.Medkit)));

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

        public void PrintState()
            => ExLoader.Info("Debug Custom Item",
                $"Debug Item State:\n" +
                $"Owner - {Owner?.Name ?? "null"}\n" +
                $"Item - {Item != null}\n" +
                $"Pickup - {Pickup != null}\n" +
                $"Serial - {Serial}\n" +
                $"IsSelected - {IsSelected}");
    }
}