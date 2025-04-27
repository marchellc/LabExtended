using LabExtended.API;
using LabExtended.API.Toys;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called after a toy interaction is aborted.
/// </summary>
public class PlayerInteractingToyAbortedEventArgs : EventArgs
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
    /// Creates a new <see cref="PlayerInteractingToyAbortedEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The interacting player.</param>
    /// <param name="toy">The target toy.</param>
    public PlayerInteractingToyAbortedEventArgs(ExPlayer player, InteractableToy toy)
    {
        Player = player;
        Toy = toy;
    }
}