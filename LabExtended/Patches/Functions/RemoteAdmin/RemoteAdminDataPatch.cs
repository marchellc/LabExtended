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
    [HarmonyPatch(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), typeof(CommandSender), typeof(string))]
    public static class RemoteAdminDataPatch
    {
        public static bool Prefix(RaPlayer __instance, CommandSender sender, string data)
        {
            try
            {
                ApiLoader.Debug("Remote Admin API", $"RA request: &1{data}&r");

                if (!ExPlayer.TryGet(sender, out var player))
                    return true;

                var array = data.Split(' ');

                if (array.Length != 2 || !int.TryParse(array[0], out var result))
                {
                    player.RemoteAdmin.SendObjectHelp();

                    ApiLoader.Debug("Remote Admin API", $"Array size or parsing failed ({array.Length} / 2) ({array[0]})");
                    return false;
                }

                if (result == 1 && !player.IsNorthwoodStaff && !player.Hub.authManager.BypassBansFlagSet && !CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess))
                {
                    ApiLoader.Debug("Remote Admin API", $"Missing permissions");
                    return false;
                }

                if (!array[1].TrySplit('.', true, null, out var args))
                {
                    player.RemoteAdmin.SendObjectHelp();

                    ApiLoader.Debug("Remote Admin API", $"Failed to split {array[1]}");
                    return false;
                }

                if (!RemoteAdminButtons.TryGetButton(result is 1 ? RemoteAdminButtonType.RequestIp : RemoteAdminButtonType.Request, out var dataButton))
                {
                    ApiLoader.Debug("Remote Admin API", $"Unknown button");
                    return true;
                }

                var list = ListPool<int>.Shared.Rent();

                foreach (var arg in args)
                {
                    if (!int.TryParse(arg.Remove("."), out var id))
                    {
                        ApiLoader.Debug("Remote Admin API", $"Failed to parse: {arg.Remove(".")}");
                        continue;
                    }

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