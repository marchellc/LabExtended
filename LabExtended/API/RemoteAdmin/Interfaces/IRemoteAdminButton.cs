using System.Text;

namespace LabExtended.API.RemoteAdmin.Interfaces
{
    public interface IRemoteAdminButton
    {
        bool BindObject(IRemoteAdminObject remoteAdminObject);
        bool UnbindObject(IRemoteAdminObject remoteAdminObject);

        bool OnPressed(ExPlayer player, IEnumerable<int> selectedObjects);

        void OnOpened(ExPlayer player, StringBuilder builder, int pos, List<IRemoteAdminObject> appendedNames);
    }
}