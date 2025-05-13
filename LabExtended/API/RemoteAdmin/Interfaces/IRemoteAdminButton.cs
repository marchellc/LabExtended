using System.Text;

namespace LabExtended.API.RemoteAdmin.Interfaces;

/// <summary>
/// The base interface for all Remote Admin buttons.
/// </summary>
public interface IRemoteAdminButton
{
    /// <summary>
    /// Binds an object to the button.
    /// </summary>
    /// <param name="remoteAdminObject">The object to bind.</param>
    /// <returns>true if the object was bound</returns>
    bool BindObject(IRemoteAdminObject remoteAdminObject);
    
    /// <summary>
    /// Unbinds an object from the button.
    /// </summary>
    /// <param name="remoteAdminObject">The object to unbind.</param>
    /// <returns>true if the object was unbound</returns>
    bool UnbindObject(IRemoteAdminObject remoteAdminObject);

    /// <summary>
    /// Gets called when the button is pressed.
    /// </summary>
    /// <param name="player">The player who pressed the button.</param>
    /// <param name="selectedObjects">The list of selected object IDs.</param>
    /// <returns>true if the press was handled</returns>
    bool OnPressed(ExPlayer player, IEnumerable<int> selectedObjects);

    /// <summary>
    /// Gets called when the panel gets opened.
    /// </summary>
    /// <param name="player">The player who opened the panel.</param>
    /// <param name="builder">The builder used to append data.</param>
    /// <param name="pos">The last button position.</param>
    /// <param name="appendedNames">The list of already appended objects.</param>
    void OnOpened(ExPlayer player, StringBuilder builder, int pos, List<IRemoteAdminObject> appendedNames);
}