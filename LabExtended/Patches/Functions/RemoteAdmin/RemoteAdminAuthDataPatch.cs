using HarmonyLib;

using LabExtended.API;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.RemoteAdmin.Enums;

using LabExtended.Core;
using LabExtended.Extensions;
using NorthwoodLib.Pools;
using RemoteAdmin;
using RemoteAdmin.Communication;

namespace LabExtended.Patches.Functions.RemoteAdmin
{
    [HarmonyPatch(typeof(RaPlayerAuth), nameof(RaPlayerAuth.ReceiveData), typeof(CommandSender), typeof(string))]
    public static class RemoteAdminAuthDataPatch
    {
        public static bool Prefix(RaPlayerAuth __instance, CommandSender sender, string data)
        {
            try
            {
                ApiLoader.Debug("Remote Admin API", $"RA request: &1{data}&r");

                if (!ExPlayer.TryGet(sender, out var player))
                    return true;

                if (!RemoteAdminButtons.TryGetButton(RemoteAdminButtonType.RequestAuth, out var dataButton))
                    return true;

                if (!player.IsNorthwoodModerator && !player.Hub.authManager.BypassBansFlagSet && !CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess))
                    return false;

                var array = data.Split(' ');

                if (array.Length != 1)
                {
                    ApiLoader.Debug("Remote Admin API", $"Array size or parsing failed ({array.Length} / 2) ({array[0]})");
                    return false;
                }

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
                ApiLoader.Error("Remote Admin API", $"An error occured while handling RA Player request:\n{ex.ToColoredString()}");
                return true;
            }
        }
    }
}