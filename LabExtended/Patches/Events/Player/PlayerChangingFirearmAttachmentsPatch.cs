﻿using HarmonyLib;

using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabExtended.API;
using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Events.Player;
using LabExtended.Utilities.Firearms;

using Mirror;

using NorthwoodLib.Pools;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements <see cref="PlayerChangingFirearmAttachmentsEventArgs"/>
/// </summary>
public static class PlayerChangingFirearmAttachmentsPatch
{
    [HarmonyPatch(typeof(AttachmentsServerHandler), nameof(AttachmentsServerHandler.ServerReceiveChangeRequest))]
    private static bool Prefix(NetworkConnection conn, AttachmentsChangeRequest msg)
    {
        if (!ExPlayer.TryGet(conn, out var player))
            return false;

        if (player.Inventory.CurrentItem is not Firearm firearm || firearm.ItemSerial != msg.WeaponSerial)
            return false;

        if (!AttachmentsServerHandler.AnyWorkstationsNearby(player.ReferenceHub))
            return false;

        var old = firearm.GetCurrentAttachmentsCode();
        var code = firearm.ValidateAttachmentsCode(msg.AttachmentsCode);
        
        var current = ListPool<AttachmentName>.Shared.Rent();
        var toEnable = ListPool<AttachmentName>.Shared.Rent();
        var toDisable = ListPool<AttachmentName>.Shared.Rent();

        var labApiChangingAttachmentsArgs = new PlayerChangingAttachmentsEventArgs(player.ReferenceHub, firearm, old, code);
        
        PlayerEvents.OnChangingAttachments(labApiChangingAttachmentsArgs);

        if (!labApiChangingAttachmentsArgs.IsAllowed)
            return false;

        code = labApiChangingAttachmentsArgs.NewAttachments;

        firearm.GetAttachmentsDiff(code, current, toEnable, toDisable);
        
        var changingAttachmentsArgs =
            new PlayerChangingFirearmAttachmentsEventArgs(player, firearm, code, current, toEnable, toDisable);

        if (!ExPlayerEvents.OnChangingAttachments(changingAttachmentsArgs))
            return false;
        
        firearm.ApplyAttachmentsDiff(toEnable, toDisable, true);
        
        ExPlayerEvents.OnChangedAttachments(new(player, firearm, code, current, toEnable, toDisable));
        PlayerEvents.OnChangedAttachments(new(player.ReferenceHub, firearm, old, firearm.GetCurrentAttachmentsCode()));
        
        ListPool<AttachmentName>.Shared.Return(current);
        ListPool<AttachmentName>.Shared.Return(toEnable);
        ListPool<AttachmentName>.Shared.Return(toDisable);

        return false;
    }
}