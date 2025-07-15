namespace LabExtended.API.CustomTeams;

/// <summary>
/// Extensions targeting the Custom Teams API.
/// </summary>
public static class CustomTeamExtensions
{
    /// <summary>
    /// Whether or not a player is in a custom team.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>true if the player is in a custom team</returns>
    public static bool HasCustomTeam(this ExPlayer player)
        => player?.Role?.CustomTeam != null;

    /// <summary>
    /// Whether or not a player is in a custom team.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>true if the player is in a custom team</returns>
    public static bool HasCustomTeam<T>(this ExPlayer player) where T : CustomTeamInstance
        => player?.Role?.CustomTeam is T;

    /// <summary>
    /// Whether or not a player is in a custom team.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="team">The resulting team</param>
    /// <returns>true if the player is in a custom team</returns>
    public static bool HasCustomTeam<T>(this ExPlayer player, out T team) where T : CustomTeamInstance
        => (team = (player?.Role?.CustomTeam as T)) != null;
}