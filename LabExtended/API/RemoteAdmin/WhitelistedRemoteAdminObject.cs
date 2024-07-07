using Common.Extensions;
using LabExtended.API.Enums;

namespace LabExtended.API.RemoteAdmin
{
    /// <summary>
    /// A <see cref="RemoteAdminObject"/> class that allows showing objects to specific players only.
    /// </summary>
    public class WhitelistedRemoteAdminObject : RemoteAdminObject
    {
        internal readonly HashSet<uint> _visibleTo;

        /// <inheritdoc/>
        public WhitelistedRemoteAdminObject(string listName, string customId = null, bool isOnTop = false, bool keepOnRoundRestart = false, RemoteAdminIconType listIconType = RemoteAdminIconType.None) : base(listName, customId, isOnTop, keepOnRoundRestart, listIconType)
            => _visibleTo = new HashSet<uint>();

        /// <inheritdoc/>
        public override bool IsVisible(ExPlayer player)
            => _visibleTo.Contains(player.NetId);

        /// <summary>
        /// Shows this object to the specified <paramref name="player"/>.
        /// </summary>
        /// <param name="player">The player to show this object to.</param>
        /// <returns><see langword="true"/> if the player was succesfully added, otherwise <see langword="false"/>.</returns>
        public bool ShowFor(ExPlayer player)
            => _visibleTo.Add(player.NetId);

        /// <summary>
        /// Hides this object for the specified <paramref name="player"/>.
        /// </summary>
        /// <param name="player">The player to hide this object for.</param>
        /// <returns><see langword="true"/> if the player was succesfully removed, otherwise <see langword="false"/>.</returns>
        public bool HideFor(ExPlayer player)
            => _visibleTo.Add(player.NetId);

        /// <summary>
        /// Shows this object to all players on the server.
        /// </summary>
        public void ShowForAll()
            => _visibleTo.AddRange(ExPlayer.Players.Select(p => p.NetId));

        /// <summary>
        /// Hides this object to all players on the server.
        /// </summary>
        public void HideForAll()
            => _visibleTo.Clear();
    }
}