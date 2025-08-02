using HarmonyLib;

using LabExtended.API;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.RemoteAdmin.Enums;

using LabExtended.Core;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

using RemoteAdmin;
using RemoteAdmin.Communication;

namespace LabExtended.Patches.Functions.RemoteAdmin;

/// <summary>
/// Provides functionality of the custom Request Auth button.
/// </summary>
public static class RemoteAdminAuthDataPatch
{
    [HarmonyPatch(typeof(RaPlayerAuth), nameof(RaPlayerAuth.ReceiveData), typeof(CommandSender), typeof(string))]
    private static bool Prefix(RaPlayerAuth __instance, CommandSender sender, string data)
    {
        try
        {
            if (!ExPlayer.TryGet(sender, out var player))
                return true;

            if (!RemoteAdminController.Buttons.TryGetValue(RemoteAdminButtonType.RequestAuth, out var dataButton))
                return true;

            if (!player.IsNorthwoodStaff
                && !player.ReferenceHub.authManager.BypassBansFlagSet
                && !CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess))
                return false;

            var array = data.Split(' ');

            if (array.Length != 1)
                return false;

            if (!array[0].TrySplit('.', true, null, out var args))
                return false;

            var list = ListPool<int>.Shared.Rent();

            foreach (var arg in args)
            {
                if (!int.TryParse(arg.Remove("."), out var id))
                    continue;

                list.Add(id);
            }

            var processed = dataButton.OnPressed(player, list);

            ListPool<int>.Shared.Return(list);
            return processed;
        }
        catch (Exception ex)
        {
            ApiLog.Error("Remote Admin API",
                $"An error occured while handling RA Player request:\n{ex.ToColoredString()}");
            return true;
        }
    }
}