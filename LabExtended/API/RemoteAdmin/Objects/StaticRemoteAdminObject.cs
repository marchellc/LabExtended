using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Interfaces;

namespace LabExtended.API.RemoteAdmin.Objects
{
    public class StaticRemoteAdminObject : IRemoteAdminObject
    {
        public StaticRemoteAdminObject(string name, string response, RemoteAdminObjectFlags flags = RemoteAdminObjectFlags.ShowToNorthwoodStaff, RemoteAdminIconType icons = RemoteAdminIconType.None)
        {
            Name = name;
            Response = response;

            Flags = flags;
            Icons = icons;
        }

        public string Name { get; set; }
        public string Response { get; set; }

        public RemoteAdminObjectFlags Flags { get; set; } = RemoteAdminObjectFlags.ShowToNorthwoodStaff;
        public RemoteAdminIconType Icons { get; set; } = RemoteAdminIconType.None;

        public string CustomId { get; set; }

        public string Id { get; set; }
        public int ListId { get; set; }
        public bool IsActive { get; set; }

        public string GetName(ExPlayer player)
            => Name;

        public string GetResponse(ExPlayer player, IEnumerable<ExPlayer> selectedPlayers, RemoteAdminButtonType button)
            => Response;

        public bool GetVisiblity(ExPlayer player)
            => true;

        public virtual void OnDisabled() { }
        public virtual void OnEnabled() { }
    }
}