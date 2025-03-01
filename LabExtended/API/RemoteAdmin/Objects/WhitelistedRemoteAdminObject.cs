using LabExtended.API.Enums;

using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Interfaces;

namespace LabExtended.API.RemoteAdmin.Objects
{
    public class WhitelistedRemoteAdminObject : IRemoteAdminObject
    {
        public HashSet<uint> Whitelist { get; } = new();

        public virtual RemoteAdminObjectFlags Flags { get; } = RemoteAdminObjectFlags.ShowToNorthwoodStaff;
        public virtual RemoteAdminIconType Icons { get; } = RemoteAdminIconType.None;

        public virtual string CustomId { get; set; }

        public string Id { get; set; }
        public int ListId { get; set; }
        public bool IsActive { get; set; }

        public virtual string GetName(ExPlayer player)
            => string.Empty;

        public virtual string GetButton(ExPlayer player, RemoteAdminButtonType buttonType)
            => string.Empty;

        public virtual string GetResponse(ExPlayer player, IEnumerable<ExPlayer> selectedPlayers, RemoteAdminButtonType button)
            => string.Empty;

        public virtual bool GetVisibility(ExPlayer player)
            => player != null && Whitelist.Contains(player.NetId);

        public virtual void OnDisabled() { }
        public virtual void OnEnabled() { }
    }
}