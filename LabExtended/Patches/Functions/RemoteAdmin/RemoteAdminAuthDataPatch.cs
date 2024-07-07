using Common.Pooling.Pools;

using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin;

using LabExtended.Core;

using PluginAPI.Core;

using RemoteAdmin.Communication;

namespace LabExtended.Patches.Functions.RemoteAdmin
{
    [HarmonyPatch(typeof(RaPlayerAuth), nameof(RaPlayerAuth.ReceiveData), typeof(CommandSender), typeof(string))]
    public static class RemoteAdminAuthDataPatch
    {
        public static bool Prefix(RaPlayerAuth __instance, CommandSender sender, string data)
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

            var args = data.Split(' ');

            if (args.Length != 1)
            {
                ExLoader.Debug("Remote Admin API", $"Unexpected array size: {args.Length} / 1");
                return true;
            }

            if (!int.TryParse(args[0].Replace(".", string.Empty).Trim(), out var objectId))
            {
                ExLoader.Debug("Remote Admin API", $"Object ID parsing failure: {args[0]}");
                return true;
            }

            if (!RemoteAdminUtils.TryGetObject(objectId, out var remoteAdminPlayerObject) || !remoteAdminPlayerObject.IsActive || !remoteAdminPlayerObject.IsVisible(player))
                return true;

            var builder = StringBuilderPool.Shared.Rent();

            builder.Append($"$1 ");

            remoteAdminPlayerObject.OnUpdate();
            remoteAdminPlayerObject.OnRequest(player, RemoteAdminPlayerRequestType.PlayerAuth, builder);

            sender.RaReply(StringBuilderPool.Shared.ToStringReturn(builder), true, true, string.Empty);
            return false;
        }
    }
}