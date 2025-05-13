using HarmonyLib;

using LabExtended.API;

using NetworkManagerUtils.Dummies;
using RemoteAdmin;
using RemoteAdmin.Communication;

namespace LabExtended.Patches.Functions.RemoteAdmin;

/// <summary>
/// Implements custom Remote Admin actions.
/// </summary>
public static class RemoteAdminDummyReceivePatch
{
    [HarmonyPatch(typeof(RaDummyActions), nameof(RaDummyActions.ReceiveData), typeof(CommandSender), typeof(string))]
    private static bool Prefix(RaDummyActions __instance, CommandSender sender, string data)
    {
        if (sender is not PlayerCommandSender pcs)
            return true;

        __instance._senderNetId = pcs.ReferenceHub.netId;

        var isDirty = false;

        foreach (var hub in ReferenceHub.AllHubs)
        {
            if (hub.IsHost)
                continue;

            var receivers = RaDummyActions.NonDirtyReceivers.GetOrAddNew(hub.netId);

            if (DummyActionCollector.IsDirty(hub))
            {
                receivers.Clear();

                isDirty = true;
            }
            else if (!receivers.Contains(hub.netId))
            {
                isDirty = true;
            }
        }
        
        if (isDirty)
            ReceiveData(__instance, sender, data);
        
        return false;
    }
    
    private static void ReceiveData(RaDummyActions actions, CommandSender sender, string data)
    {
        actions._stringBuilder.Clear();
        
        actions._stringBuilder
            .Append("$")
            .Append(actions.DataId)
            .Append(" ");
        
        actions.GatherData();
        
        sender.RaReply($"${actions.DataId} {actions._stringBuilder}", true, false, string.Empty);
    }
}