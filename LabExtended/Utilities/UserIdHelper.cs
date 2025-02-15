using CentralAuth;

using LabExtended.Extensions;

namespace LabExtended.Utilities
{
    public static class UserIdHelper
    {
        public const byte SteamIdLength = 17;
        public const byte DiscordIdLength = 18;

        public struct UserIdInfo
        {
            public string FullId { get; internal set; }
            public string ClearId { get; internal set; }

            public string Type { get; internal set; }

            public ulong ParsedId { get; internal set; }

            public bool IsServer { get; internal set; }
            public bool IsNorthwood { get; internal set; }
            public bool IsPatreon { get; internal set; }
            public bool IsDiscord { get; internal set; }
            public bool IsSteam { get; internal set; }
            public bool IsPlayer { get; internal set; }
            public bool IsParsable { get; internal set; }
            public bool IsDummy { get; internal set; }

            public bool IsMatch(string otherQuery) => !string.IsNullOrWhiteSpace(FullId) && GetInfo(otherQuery).FullId == FullId;
            public bool IsMatch(UserIdInfo otherInfo) => !string.IsNullOrWhiteSpace(FullId) && !string.IsNullOrWhiteSpace(otherInfo.FullId) && otherInfo.FullId == FullId;
        }

        public static string GetClearId(string query) => GetInfo(query).ClearId;
        public static string GetFullId(string query) => GetInfo(query).FullId;

        public static ulong GetParsedId(string query) => GetInfo(query).ParsedId;

        public static string GetIdType(string query) => GetInfo(query).Type;

        public static bool IsServerId(string query) => GetInfo(query).IsServer;
        public static bool IsPlayerId(string query) => GetInfo(query).IsPlayer;
        public static bool IsNorthwoodId(string query) => GetInfo(query).IsNorthwood;
        public static bool IsSteamId(string query) => GetInfo(query).IsSteam;
        public static bool IsDiscordId(string query) => GetInfo(query).IsDiscord;

        public static UserIdInfo GetInfo(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentNullException(nameof(query));

            query = query.Trim();

            var info = new UserIdInfo();

            if (query.TrySplit('@', true, 2, out var parts))
            {
                var lowerPart = parts[1].ToLower();

                info.IsDiscord = lowerPart == "discord";
                info.IsSteam = lowerPart == "steam";
                info.IsNorthwood = lowerPart == "northwood";
                info.IsPatreon = lowerPart == "patreon";

                info.IsServer = parts[0] == PlayerAuthenticationManager.DedicatedId;
                info.IsDummy = parts[0] == PlayerAuthenticationManager.DummyId;

                info.IsPlayer = !info.IsServer && !info.IsDummy;

                info.Type = lowerPart;

                info.FullId = query;
                info.ClearId = parts[0];

                if (!info.IsServer && ulong.TryParse(parts[0], out var parsedId))
                {
                    info.ParsedId = parsedId;
                    info.IsParsable = true;
                }
                else
                {
                    info.ParsedId = 0;
                    info.IsParsable = false;
                }

                return info;
            }
            else
            {
                if (query == PlayerAuthenticationManager.DedicatedId || query == PlayerAuthenticationManager.HostId
                    || query == $"{PlayerAuthenticationManager.DedicatedId}@server" || query == $"{PlayerAuthenticationManager.HostId}@server"
                    || query == PlayerAuthenticationManager.DummyId || query == $"{PlayerAuthenticationManager.DummyId}@dummy")
                {
                    info.IsDiscord = false;
                    info.IsPlayer = false;
                    info.IsParsable = false;
                    info.IsNorthwood = false;
                    info.IsPatreon = false;
                    info.IsSteam = false;
                    info.IsServer = true;

                    if (query == PlayerAuthenticationManager.DummyId || query == $"{PlayerAuthenticationManager.DummyId}@dummy")
                    {
                        info.IsDummy = true;

                        info.ClearId = PlayerAuthenticationManager.DummyId;
                        info.FullId = $"{PlayerAuthenticationManager.DummyId}@dummy";

                        info.Type = "dummy";
                    }
                    else
                    {
                        info.ClearId = PlayerAuthenticationManager.DedicatedId;
                        info.FullId = $"{PlayerAuthenticationManager.DedicatedId}@server";

                        info.Type = "server";
                    }

                    info.ParsedId = 0;
                    return info;
                }
                else
                {
                    info.IsDiscord = query.Length >= DiscordIdLength;
                    info.IsSteam = query.Length == SteamIdLength;

                    info.IsPlayer = info.IsDiscord || info.IsSteam;

                    info.IsServer = false;
                    info.IsNorthwood = false;
                    info.IsPatreon = false;

                    info.ClearId = query;

                    if (info.IsDiscord)
                    {
                        info.FullId = $"{query}@discord";
                        info.Type = "discord";
                    }
                    else if (info.IsSteam)
                    {
                        info.FullId = $"{query}@steam";
                        info.Type = "steam";
                    }
                    else
                    {
                        info.FullId = $"{query}@unknown";
                        info.Type = "unknown";
                    }

                    if (ulong.TryParse(query, out var parsedId))
                    {
                        info.IsParsable = true;
                        info.ParsedId = parsedId;
                    }
                    else
                    {
                        info.IsParsable = false;
                        info.ParsedId = 0;
                    }

                    return info;
                }
            }
        }
    }
}