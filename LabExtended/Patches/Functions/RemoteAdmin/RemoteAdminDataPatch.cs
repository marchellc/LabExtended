using Common.Pooling.Pools;

using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin;

using LabExtended.Core;
using LabExtended.Extensions;

using PluginAPI.Core;

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
                if (!Player.TryGet(sender, out var apiPlayer))
                {
                    ExLoader.Debug("Remote Admin API", $"Failed to fetch NW API player");
                    return true;
                }

                var player = ExPlayer.Get(apiPlayer.ReferenceHub);

                if (player is null)
                {
                    ExLoader.Debug("Remote Admin API", $"Failed to fetch Ex Player");
                    return true;
                }

                var array = data.Split(' ');

                if (array.Length != 2 || !int.TryParse(array[0], out var result))
                {
                    ExLoader.Debug("Remote Admin API", $"Array size or parsing failed ({array.Length} / 2) ({array[0]})");
                    return false;
                }

                var args = array.Skip(1).ToArray();

                if (!int.TryParse(args[0].Replace(".", string.Empty).Trim(), out var objId))
                {
                    ExLoader.Debug("Remote Admin API", $"Object ID parsing failed ({args[0]})");
                    return true;
                }

                if (!RemoteAdminUtils.TryGetObject(objId, out var remoteAdminPlayerObject) || !remoteAdminPlayerObject.IsActive || !remoteAdminPlayerObject.IsVisible(player))
                    return true;

                var builder = StringBuilderPool.Shared.Rent();

                builder.Append($"${__instance.DataId} ");

                remoteAdminPlayerObject.OnUpdate();
                remoteAdminPlayerObject.OnRequest(player, (RemoteAdminPlayerRequestType)result, builder);

                sender.RaReply(StringBuilderPool.Shared.ToStringReturn(builder), true, true, string.Empty);
                return false;
            }
            catch (Exception ex)
            {
                ExLoader.Error("Remote Admin API", $"An error occured while handling RA Player request:\n{ex.ToColoredString()}");
                return true;
            }
        }
    }
}