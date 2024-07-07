namespace LabExtended.API.Enums
{
    /// <summary>
    /// Specifies the type of a Remote Admin player-specific request.
    /// </summary>
    public enum RemoteAdminPlayerRequestType : byte
    {
        /// <summary>
        /// A player data request (<b>REQUEST</b> button).
        /// </summary>
        PlayerData = 1,

        /// <summary>
        /// A player IP request (<b>REQUEST IP</b> button).
        /// </summary>
        PlayerIp = 0,

        /// <summary>
        /// A player authentification token request (<b>REQUEST AUTH</b> button).
        /// </summary>
        PlayerAuth = 3
    }
}