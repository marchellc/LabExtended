namespace LabExtended.API.RemoteAdmin
{
    /// <summary>
    /// Specifies the type of a general Remote Admin request type.
    /// </summary>
    public enum RemoteAdminRequestType : byte
    {
        /// <summary>
        /// A player list request.
        /// </summary>
        PlayerList = 0,

        /// <summary>
        /// A player data request.
        /// </summary>
        PlayerData = 1,

        /// <summary>
        /// A player authentification token request.
        /// </summary>
        PlayerAuth = 3,

        /// <summary>
        /// A global ban request.
        /// </summary>
        GlobalBan = 5,

        /// <summary>
        /// A server status request.
        /// </summary>
        ServerStatus = 7,

        /// <summary>
        /// A team status request.
        /// </summary>
        TeamStatus = 8
    }
}