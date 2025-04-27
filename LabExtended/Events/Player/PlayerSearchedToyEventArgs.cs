using LabExtended.API;
using LabExtended.API.Toys;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called after a toy is interacted with.
/// </summary>
public class PlayerSearchedToyEventArgs : EventArgs
{
    /// <summary>
    /// Gets the interacting player.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the target toy.
    /// </summary>
    public InteractableToy Toy { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerSearchedToyEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The interacting player.</param>
    /// <param name="toy">The target toy.</param>
    public PlayerSearchedToyEventArgs(ExPlayer player, InteractableToy toy)
    {
        Player = player;
        Toy = toy;
    }
}