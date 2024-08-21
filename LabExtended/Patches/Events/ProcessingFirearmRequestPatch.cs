using HarmonyLib;

using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Attachments.Components;
using InventorySystem.Items.Firearms.BasicMessages;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Other;
using LabExtended.Extensions;

using Mirror;

using PlayerRoles.Spectating;

using PluginAPI.Events;

using Utils.Networking;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerRequestReceived))]
    public static class ProcessingFirearmRequestPatch
    {
        public static bool Prefix(NetworkConnection conn, RequestMessage msg)
        {
            if (!ExPlayer.TryGet(conn, out var player))
                return true;

            if (msg.Request is RequestType.RequestStatuses && FirearmBasicMessagesHandler.AlreadyRequestedStatuses.Add(player.PlayerId))
            {
                foreach (var other in ExPlayer.Players)
                {
                    if (other.Inventory.CurrentItem != null && other.Inventory.CurrentItem is Firearm firearm)
                        conn.Send(new StatusMessage(firearm.ItemSerial, firearm.Status));
                }

                ReflexSightDatabase.HandleNewClient(player.Hub);
            }

            if (msg.Serial != player.Inventory.CurrentItemIdentifier.SerialNumber || player.Inventory.CurrentItem is not Firearm curFirearm)
                return false;

            var primaryBlocked = player.Hub.HasBlock(BlockedInteraction.ItemPrimaryAction);
            var usageBlocked = player.Hub.HasBlock(BlockedInteraction.ItemUsage);

            var processingArgs = new ProcessingFirearmRequestArgs(player, msg);

            if (!HookRunner.RunEvent(processingArgs, true))
                return false;

            msg = processingArgs.Message;

            if (msg.Request is RequestType.Reload)
            {
                if (!primaryBlocked && EventManager.ExecuteEvent(new PlayerReloadWeaponEvent(player.Hub, curFirearm)) && curFirearm.AttachmentsValue(AttachmentParam.PreventReload) <= 0f && curFirearm.AmmoManagerModule.ServerTryReload())
                    msg.SendToAuthenticated();

            }
            else if (msg.Request is RequestType.Unload)
            {
                if (!primaryBlocked && EventManager.ExecuteEvent(new PlayerUnloadWeaponEvent(player.Hub, curFirearm)) && curFirearm.AttachmentsValue(AttachmentParam.PreventReload) <= 0f && curFirearm.AmmoManagerModule.ServerTryUnload())
                    msg.SendToAuthenticated();
            }
            else if (msg.Request is RequestType.ReloadStop)
            {
                if (!curFirearm.AmmoManagerModule.Standby && curFirearm.AmmoManagerModule.ServerTryStopReload())
                    msg.SendToAuthenticated();
            }
            else if (msg.Request is RequestType.Dryfire)
            {
                if (!usageBlocked && EventManager.ExecuteEvent(new PlayerDryfireWeaponEvent(player.Hub, curFirearm)) && curFirearm.ActionModule.ServerAuthorizeDryFire())
                {
                    msg.SendToAuthenticated();
                    curFirearm.OnWeaponDryfired();
                }
            }
            else if (msg.Request is RequestType.AdsIn)
            {
                if (!usageBlocked)
                {
                    var isEnabled = curFirearm.HasFlashlightEnabled();

                    if (curFirearm.HasAdvantageFlag(AttachmentDescriptiveAdvantages.Flashlight) && EventManager.ExecuteEvent(new PlayerToggleFlashlightEvent(player.Hub, curFirearm, !isEnabled)))
                    {
                        if (isEnabled)
                            curFirearm.DisableFlashlight();
                        else
                            curFirearm.EnableFlashlight();
                    }
                }
            }
            else if (msg.Request is RequestType.Inspect)
            {
                if (!usageBlocked)
                    msg.SendToHubsConditionally(hub => hub.roleManager.CurrentRole is SpectatorRole);
            }

            HookRunner.RunEvent(new ProcessedFirearmRequestArgs(player, msg));
            return false;
        }
    }
}