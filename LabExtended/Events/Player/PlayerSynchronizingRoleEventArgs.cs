using LabExtended.API;

using Mirror;

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
    /// Gets the writer used to send spoofed role data.
    /// </summary>
    public NetworkWriter SpoofedData { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerSynchronizingRoleEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The source player.</param>
    /// <param name="receiver">The receiving player.</param>
    /// <param name="role">The target role type.</param>
    /// <param name="spoofedData">The writer for spoofed role data.</param>
    public PlayerSynchronizingRoleEventArgs(ExPlayer player, ExPlayer receiver, RoleTypeId role, NetworkWriter spoofedData)
    {
        Player = player;
        Receiver = receiver;
        Role = role;
        SpoofedData = spoofedData;
    }
}