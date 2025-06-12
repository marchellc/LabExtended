using LabExtended.API;

using PlayerRoles;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called after a player's current role data is sent to another player.
/// </summary>
public class PlayerSynchronizedRoleEventArgs : EventArgs
{
    /// <summary>
    /// Gets the player that owns the role.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the player who is received the role data of <see cref="Player"/>.
    /// </summary>
    public ExPlayer Receiver { get; }
    
    /// <summary>
    /// Gets the type of the role that was sent to <see cref="Receiver"/>.
    /// </summary>
    public RoleTypeId Role { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerSynchronizedRoleEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The source player.</param>
    /// <param name="receiver">The receiving player.</param>
    /// <param name="role">The target role type.</param>
    public PlayerSynchronizedRoleEventArgs(ExPlayer player, ExPlayer receiver, RoleTypeId role)
    {
        Player = player;
        Receiver = receiver;
        Role = role;
    }
}