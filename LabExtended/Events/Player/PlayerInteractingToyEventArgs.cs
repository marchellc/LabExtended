using LabExtended.API;
using LabExtended.API.Toys;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called before a toy is interacted with.
/// </summary>
public class PlayerInteractingToyEventArgs : BooleanEventArgs
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
    /// Whether or not the toy can be interacted with.
    /// </summary>
    public bool CanInteract { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerInteractingToyEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The interacting player.</param>
    /// <param name="toy">The target toy.</param>
    /// <param name="canInteract">Whether or not the toy can be interacted with.</param>
    public PlayerInteractingToyEventArgs(ExPlayer player, InteractableToy toy, bool canInteract)
    {
        Player = player;
        Toy = toy;
        CanInteract = canInteract;
    }
}