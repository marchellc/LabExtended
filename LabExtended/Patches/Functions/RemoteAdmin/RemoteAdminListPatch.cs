using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Enums;

using LabExtended.Core;

using NorthwoodLib.Pools;

using RemoteAdmin;
using RemoteAdmin.Communication;

namespace LabExtended.Patches.Functions.RemoteAdmin
{
    public static class RemoteAdminListPatch
    {
        public const string MutedIconPrefix = "<link=RA_Muted><color=white>[</color>\ud83d\udd07<color=white>]</color></link> ";
        public const string OverwatchIconPrefix = "<link=RA_OverwatchEnabled><color=white>[</color><color=#03f8fc>\uf06e</color><color=white>]</color></link> ";

        [HarmonyPatch(typeof(RaPlayerList), nameof(RaPlayerList.ReceiveData), typeof(CommandSender), typeof(string))]
        public static bool Prefix(RaPlayerList __instance, CommandSender sender, string data)
        {
            if (!ExPlayer.TryGet(sender, out var player))
                return true;

            player.RemoteAdmin.InternalRegisterRequest();

            var array = data.Split(' ');

            if (array.Length != 3)
            {
                ApiLog.Debug("Remote Admin API", $"Array size false ({array.Length} / 3)");
                return false;
            }

            if (!int.TryParse(array[0], out var num) || !int.TryParse(array[1], out var sortingType))
            {
                ApiLog.Debug("Remote Admin API", $"Failed to parse sorting type");
                return false;
            }

            if (!Enum.IsDefined(typeof(RaPlayerList.PlayerSorting), sortingType))
            {
                ApiLog.Debug("Remote Admin API", $"Sorting type is not defined");
                return false;
            }

            var hasHiddenBadges = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenBadges);
            var hasGlobalHiddenBadges = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenGlobalBadges);
            var sorting = (RaPlayerList.PlayerSorting)sortingType;
            var builder = StringBuilderPool.Shared.Rent("\n");

            player.RemoteAdmin.InternalPrependObjects(builder);

            foreach (var otherHub in array[2] == "1" ? __instance.SortPlayersDescending(sorting) : __instance.SortPlayers(sorting))
            {
                var other = ExPlayer.Get(otherHub);

                if (other is null)
                    continue;

                if (!other.Switches.IsVisibleInRemoteAdmin)
                    continue;

                var icons = other.RaIcons;

                builder.Append(RaPlayerList.GetPrefix(otherHub, hasHiddenBadges, hasGlobalHiddenBadges));

                if (icons != RemoteAdminIconType.None)
                {
                    if ((icons & RemoteAdminIconType.MutedIcon) != 0)
                        builder.Append(MutedIconPrefix);

                    if ((icons & RemoteAdminIconType.OverwatchIcon) != 0)
                        builder.Append(OverwatchIconPrefix);
                }

                builder.Append("<color={RA_ClassColor}>(");
                builder.Append(other.PlayerId);
                builder.Append(") ");
                builder.Append(other.Hub.nicknameSync.CombinedName.Replace("\n", string.Empty).Replace("RA_", string.Empty)).Append("</color>");
                builder.AppendLine();
            }

            player.RemoteAdmin.InternalAppendObjects(builder);

            sender.RaReply($"${__instance.DataId} {StringBuilderPool.Shared.ToStringReturn(builder)}", true, num != 1, string.Empty);
            return false;
        }
    }
}