using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin.Enums;

namespace LabExtended.API.RemoteAdmin.Interfaces
{
    public interface IRemoteAdminObject
    {
        RemoteAdminObjectFlags Flags { get; }
        RemoteAdminIconType Icons { get; }

        string Id { get; set; }
        string CustomId { get; set; }

        int ListId { get; set; }

        bool IsActive { get; set; }

        void OnEnabled();
        void OnDisabled();

        string GetName(ExPlayer player);
        string GetButton(ExPlayer player, RemoteAdminButtonType buttonType);
        string GetResponse(ExPlayer player, IEnumerable<ExPlayer> selectedPlayers, RemoteAdminButtonType button);

        bool GetVisiblity(ExPlayer player);
    }
}