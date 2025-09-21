using CentralAuth;

using LabExtended.Extensions;

namespace LabExtended.Utilities;

/// <summary>
/// Used to retrieve information about a player's user ID.
/// </summary>
public static class UserIdHelper
{
    /// <summary>
    /// The minimum length of a Steam ID.
    /// </summary>
    public const byte SteamIdLength = 17;

    /// <summary>
    /// The minimum length of a Discord ID.
    /// </summary>
    public const byte DiscordIdLength = 18;

    /// <summary>
    /// Represents collected information about a user ID.
    /// </summary>
    public struct UserIdInfo
    {
        /// <summary>
        /// Gets the full string of the user ID, including its type.
        /// </summary>
        public string FullId { get; internal set; }

        /// <summary>
        /// Gets the string of the user ID, without it's type.
        /// </summary>
        public string ClearId { get; internal set; }

        /// <summary>
        /// Gets the type of the user ID (steam, discord, etc.).
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// The parsed value of the user ID.
        /// </summary>
        public ulong ParsedId { get; internal set; }

        /// <summary>
        /// Whether or not the ID belongs to the server player.
        /// </summary>
        public bool IsServer { get; internal set; }

        /// <summary>
        /// Whether or not the ID belongs to a member of the Northwood Staff.
        /// </summary>
        public bool IsNorthwood { get; internal set; }

        /// <summary>
        /// Whether or not the ID belongs to a Northwood Patreon supporter.
        /// </summary>
        public bool IsPatreon { get; internal set; }

        /// <summary>
        /// Whether or not the ID belongs to a player using the Discord authentification.
        /// </summary>
        public bool IsDiscord { get; internal set; }

        /// <summary>
        /// Whether or not the ID belongs to a player using the Steam authentification.
        /// </summary>
        public bool IsSteam { get; internal set; }

        /// <summary>
        /// Whether or not the ID belongs to an online player.
        /// </summary>
        public bool IsPlayer { get; internal set; }

        /// <summary>
        /// Whether or not the ID could be parsed (<see cref="ParsedId"/> property).
        /// </summary>
        public bool IsParsable { get; internal set; }

        /// <summary>
        /// Whether or not the ID belongs to a dummy player.
        /// </summary>
        public bool IsDummy { get; internal set; }

        /// <summary>
        /// Whether or not the ID matches another query.
        /// </summary>
        /// <param name="otherQuery">The other query.</param>
        /// <returns>true if the ID is a match</returns>
        public bool IsMatch(string otherQuery) 
            => !string.IsNullOrWhiteSpace(FullId) && GetInfo(otherQuery).FullId == FullId;

        /// <summary>
        /// Whether or not the ID matches another query.
        /// </summary>
        /// <param name="otherInfo">The other query.</param>
        /// <returns>true if the ID is a match</returns>
        public bool IsMatch(UserIdInfo otherInfo) 
            => !string.IsNullOrWhiteSpace(FullId) && !string.IsNullOrWhiteSpace(otherInfo.FullId) && otherInfo.FullId == FullId;
    }

    /// <summary>
    /// Gets the clear ID string.
    /// </summary>
    /// <param name="query">The user ID query.</param>
    /// <returns>The clear ID string.</returns>
    public static string GetClearId(string query) 
        => GetInfo(query).ClearId;

    /// <summary>
    /// Gets the full ID string.
    /// </summary>
    /// <param name="query">The user ID query.</param>
    /// <returns>The full ID string.</returns>
    public static string GetFullId(string query) 
        => GetInfo(query).FullId;

    /// <summary>
    /// Gets the parsed ID.
    /// </summary>
    /// <param name="query">The user ID query.</param>
    /// <returns>The parsed ID.</returns>
    public static ulong GetParsedId(string query) 
        => GetInfo(query).ParsedId;

    /// <summary>
    /// Gets the ID type string.
    /// </summary>
    /// <param name="query">The user ID query.</param>
    /// <returns>The ID type string.</returns>
    public static string GetIdType(string query) 
        => GetInfo(query).Type;

    /// <summary>
    /// Whether or not a query is a server player ID.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>true if the query is a server player's ID</returns>
    public static bool IsServerId(string query)
        => GetInfo(query).IsServer;

    /// <summary>
    /// Whether or not a query is a player ID.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>true if the query is a player's ID</returns>
    public static bool IsPlayerId(string query) 
        => GetInfo(query).IsPlayer;

    /// <summary>
    /// Whether or not a query is a Northwood Staff ID.
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>true if the query is a Northwood Staff's ID.</returns>
    public static bool IsNorthwoodId(string query) 
        => GetInfo(query).IsNorthwood;

    /// <summary>
    /// Whether or not a query is a Steam ID.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>true if the query is a Steam ID</returns>
    public static bool IsSteamId(string query) 
        => GetInfo(query).IsSteam;

    /// <summary>
    /// Whether or not a query is a Discord ID.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>true if the query is a Discord ID</returns>
    public static bool IsDiscordId(string query)
        => GetInfo(query).IsDiscord;

    /// <summary>
    /// Parses a string of a user ID to a <see cref="UserIdInfo"/> struct.
    /// </summary>
    /// <param name="query">The query to parse.</param>
    /// <returns>The parsed struct instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static UserIdInfo GetInfo(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) 
            throw new ArgumentNullException(nameof(query));

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