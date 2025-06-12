using LabExtended.API;

using PlayerRoles;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called before a player's current role data is sent to another player.
/// </summary>
public class PlayerSynchronizingRoleEventArgs : BooleanEventArgs
{
    /// <summary>
    /// Gets the player that owns the role.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the player who is supposed to receive the role data of <see cref="Player"/>.
    /// </summary>
    public ExPlayer Receiver { get; }
    
    /// <summary>
    /// Gets or sets the type of the role to sent to <see cref="Receiver"/>.
    /// </summary>
    public RoleTypeId Role { get; set; }

    /// <summary>
    /// Creates a new <see cref="PlayerSynchronizingRoleEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The source player.</param>
    /// <param name="receiver">The receiving player.</param>
    /// <param name="role">The target role type.</param>
    public PlayerSynchronizingRoleEventArgs(ExPlayer player, ExPlayer receiver, RoleTypeId role)
    {
        Player = player;
        Receiver = receiver;
        Role = role;
    }
}