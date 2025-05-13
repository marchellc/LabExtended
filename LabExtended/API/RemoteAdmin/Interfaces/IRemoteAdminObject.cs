using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin.Enums;

namespace LabExtended.API.RemoteAdmin.Interfaces;

/// <summary>
/// Base interface for custom Remote Admin objects.
/// </summary>
public interface IRemoteAdminObject
{
    /// <summary>
    /// Gets the object's flags.
    /// </summary>
    RemoteAdminObjectFlags Flags { get; }
    
    /// <summary>
    /// Gets the object's icons.
    /// </summary>
    RemoteAdminIconType Icons { get; }

    /// <summary>
    /// Gets or sets the object's ID.
    /// </summary>
    string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the object's custom ID.
    /// </summary>
    string CustomId { get; set; }

    /// <summary>
    /// Gets or sets the object's player list ID.
    /// </summary>
    int ListId { get; set; }

    /// <summary>
    /// Gets or sets the object's activity status.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Gets called once the object is enabled.
    /// </summary>
    void OnEnabled();
    
    /// <summary>
    /// Gets called once the object is disabled.
    /// </summary>
    void OnDisabled();

    /// <summary>
    /// Gets the name of this object for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns>The object name.</returns>
    string GetName(ExPlayer player);
    
    /// <summary>
    /// Gets the name of the object's button for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="buttonType">The type of the button.</param>
    /// <returns>The button label.</returns>
    string GetButton(ExPlayer player, RemoteAdminButtonType buttonType);
    
    /// <summary>
    /// Gets called once a player presses a button that belongs to this object.
    /// </summary>
    /// <param name="player">The player who pressed the button.</param>
    /// <param name="selectedPlayers">The list of selected players.</param>
    /// <param name="button">The button which was pressed.</param>
    /// <returns>The response to show.</returns>
    string GetResponse(ExPlayer player, IEnumerable<ExPlayer> selectedPlayers, RemoteAdminButtonType button);

    /// <summary>
    /// Gets this button's visibility for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns>true if the button should be visible</returns>
    bool GetVisibility(ExPlayer player);
}