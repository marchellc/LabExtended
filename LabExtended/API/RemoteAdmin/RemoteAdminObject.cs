using System.Text;

namespace LabExtended.API.RemoteAdmin
{
    /// <summary>
    /// A class used for managing fake Remote Admin objects.
    /// </summary>
    public class RemoteAdminObject
    {
        /// <summary>
        /// Gets or sets the object's name in the Remote Admin player list.
        /// </summary>
        public string ListName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to keep the object after the current round restarts.
        /// </summary>
        public bool KeepOnRoundRestart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this object is active. <b>Inactive objects get removed when the round restarts!</b>
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not to display this object on the top of the Remote Admin player list.
        /// </summary>
        public bool IsOnTop { get; set; }

        /// <summary>
        /// Gets or sets a custom ID for this object.
        /// </summary>
        public string CustomId { get; set; }

        /// <summary>
        /// Gets the automatically assigned ID.
        /// </summary>
        public int AssignedId { get; internal set; }

        /// <summary>
        /// Gets or sets the icons to display in the Remote Admin player list.
        /// </summary>
        public RemoteAdminIconType ListIconType { get; set; }

        /// <summary>
        /// Creates a new <see cref="RemoteAdminObject"/> instance.
        /// </summary>
        /// <param name="listName">The name to display in the player list.</param>
        /// <param name="customId">The custom ID to set.</param>
        /// <param name="isOnTop">Whether or not to show this object on the top of the player list.</param>
        /// <param name="keepOnRoundRestart">Whether or not to keep this object on round restart.</param>
        /// <param name="listIconType">The player list icon type.</param>
        public RemoteAdminObject(string listName, string customId = null, bool isOnTop = false, bool keepOnRoundRestart = false, RemoteAdminIconType listIconType = RemoteAdminIconType.None)
        {
            ListName = listName;
            ListIconType = listIconType;
            CustomId = customId;
            IsOnTop = isOnTop;
            KeepOnRoundRestart = keepOnRoundRestart;
        }

        /// <summary>
        /// When overriden, gets a value indicating whether or not this object should be shown to the specified <paramref name="player"/>.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns><see langword="true"/> if this object should be visible, otherwise <see langword="false"/>.</returns>
        public virtual bool IsVisible(ExPlayer player)
            => true;

        /// <summary>
        /// Gets invoked when a player makes a request on this object.
        /// </summary>
        /// <param name="player">The player who made the request.</param>
        /// <param name="remoteAdminPlayerRequestType">The request type.</param>
        /// <param name="stringBuilder">The builder used to show output.</param>
        public virtual void OnRequest(ExPlayer player, RemoteAdminPlayerRequestType remoteAdminPlayerRequestType, StringBuilder stringBuilder) { }

        /// <summary>
        /// Gets invoked when a player makes a Remote Admin request <i>(this includes the player list update which is called every second)</i>.
        /// </summary>
        public virtual void OnUpdate() { }
    }
}