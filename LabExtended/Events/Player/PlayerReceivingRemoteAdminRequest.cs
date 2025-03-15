using LabExtended.API;
using LabExtended.API.Enums;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called when a request is received from a player's Remote Admin.
/// </summary>
public class PlayerReceivingRemoteAdminRequest : BooleanEventArgs
{
    /// <summary>
    /// The player who sent the request.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the type of the request.
    /// </summary>
    public RemoteAdminRequestType Type { get; }
    
    /// <summary>
    /// Gets the message received.
    /// </summary>
    public string Data { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerReceivingRemoteAdminRequest"/> instance.
    /// </summary>
    /// <param name="player">The sending player.</param>
    /// <param name="type">The request's type.</param>
    /// <param name="data">The request's data.</param>
    public PlayerReceivingRemoteAdminRequest(ExPlayer player, RemoteAdminRequestType type, string data)
    {
        Player = player;
        Type = type;
        Data = data;
    }
}